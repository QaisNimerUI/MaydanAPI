using System.Security.Cryptography;
using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;

namespace Maydan.Application.Services;

public class AuthService : IAuthService
{
    // Forgot-password recovery (2026-09-23): per the story's own AC.
    private const int ResetTokenValidityHours = 24;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IFrontendLinkBuilder _frontendLinkBuilder;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailSender emailSender,
        IFrontendLinkBuilder frontendLinkBuilder)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailSender = emailSender;
        _frontendLinkBuilder = frontendLinkBuilder;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new InvalidOperationException("Email and password are required.");
        }

        var email = dto.Email.Trim();
        var user = await _unitOfWork.Users.GetByEmailWithAccessAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var authUser = MapAuthUser(user);

        if (user.MustResetPassword)
        {
            return new LoginResponseDto(
                false,
                true,
                null,
                null,
                authUser,
                "Password reset is required before accessing the system.");
        }

        var token = _jwtTokenGenerator.GenerateAccessToken(authUser);

        return new LoginResponseDto(
            true,
            false,
            token.AccessToken,
            token.ExpiresAtUtc,
            authUser,
            "Login successful.");
    }

    public async Task<LoginResponseDto> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            throw new InvalidOperationException("Email, current password, and new password are required.");
        }

        var email = dto.Email.Trim();
        var user = await _unitOfWork.Users.GetByEmailWithAccessAsync(email, cancellationToken);

        // Same identity check as LoginAsync, and deliberately the same failure shape/message: this
        // endpoint is reached before any token exists (see AuthController), so knowledge of the
        // current password is the only thing that can stand in for a Bearer token here.
        if (user is null || !user.IsActive || !_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Decision: reject rather than silently allow when MustResetPassword is already false. This
        // endpoint exists specifically to satisfy the forced-reset flow; an account that isn't
        // flagged for it has no business changing its password through an unauthenticated,
        // current-password-only endpoint. A general "change my password" feature for already-fine
        // accounts is a different, separate concern (see AuthService.ts's own `changePassword`,
        // which targets a different, not-yet-implemented endpoint) and isn't what this ticket adds.
        if (!user.MustResetPassword)
        {
            throw new InvalidOperationException("Password reset is not required for this account.");
        }

        // GetByEmailWithAccessAsync above is AsNoTracking (needed for the full Role/Permissions
        // graph MapAuthUser reads), so it can't be mutated and saved directly. Re-fetch a tracked
        // instance for the write; the no-tracking `user` from above remains valid for MapAuthUser.
        var trackedUser = await _unitOfWork.Users.GetByIdAsync(user.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        trackedUser.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        trackedUser.MustResetPassword = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var authUser = MapAuthUser(user);
        var token = _jwtTokenGenerator.GenerateAccessToken(authUser);

        return new LoginResponseDto(
            true,
            false,
            token.AccessToken,
            token.ExpiresAtUtc,
            authUser,
            "Password reset successful.");
    }

    // Forgot-password recovery, step 1 (MAYD-128, 2026-09-23). Security requirement from the
    // story's own domain: the response is identical whether or not the email is registered — an
    // attacker probing emails against this endpoint learns nothing either way. Only a genuinely
    // missing/blank email (a real input error, not an enumeration probe) short-circuits before
    // that point with its own message, same as LoginAsync's own required-field check above.
    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        const string genericMessage = "If an account with that email exists, a password reset link has been sent to it.";

        var email = dto.Email.Trim();
        var user = await _unitOfWork.Users.GetByEmailWithAccessAsync(email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new ForgotPasswordResponseDto(genericMessage);
        }

        // Invalidate every unused, unexpired token this user still has outstanding before issuing a
        // new one — otherwise an old, forgotten link (still sitting in an inbox or browser history)
        // stays usable right alongside the new one until it separately expires. Reusing IsUsed for
        // this (rather than a new column/reason) means ResetPasswordWithTokenAsync's existing
        // IsUsed check already rejects a superseded token with the same real "already used" message,
        // with no changes needed there.
        var outstandingTokens = await _unitOfWork.PasswordResetTokens.GetAllAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var outstanding in outstandingTokens.Where(t => t.UserId == user.UserId && !t.IsUsed && t.ExpiresAtUtc >= now))
        {
            outstanding.IsUsed = true;
            outstanding.UsedAtUtc = now;
        }

        var rawToken = GenerateRawResetToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = _passwordHasher.HashPassword(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.AddHours(ResetTokenValidityHours),
            IsUsed = false
        };

        await _unitOfWork.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resetLink = _frontendLinkBuilder.BuildResetPasswordLink(rawToken);
        var bodyHtml = $"""
            <p>Hi {System.Net.WebUtility.HtmlEncode(user.FirstNameEn)},</p>
            <p>Click the link below to reset your Maydan password. This link expires in {ResetTokenValidityHours} hours and can only be used once.</p>
            <p><a href="{resetLink}">{resetLink}</a></p>
            <p>If you didn't request this, you can safely ignore this email.</p>
            """;

        await _emailSender.SendAsync(user.Email, "Reset your Maydan password", bodyHtml, cancellationToken);

        return new ForgotPasswordResponseDto(genericMessage);
    }

    // Forgot-password recovery, step 2 (MAYD-129/130, 2026-09-23). Deliberately separate from
    // ResetPasswordAsync above (different route, different DTO) — no current password is asked for
    // or checked; proving control of the token (received by email) is what stands in for it here.
    public async Task<ResetPasswordWithTokenResponseDto> ResetPasswordWithTokenAsync(ResetPasswordWithTokenDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword) || string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            throw new InvalidOperationException("Token, new password, and confirmation are required.");
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        // TokenHash is a salted hash (same IPasswordHasher format as passwords), so it can't be
        // looked up by direct equality — the same raw token hashes differently every time it's
        // hashed. Scanning candidates and calling VerifyPassword() against each is the same
        // trade-off LoginAsync already makes for passwords; this table's expected size (one row per
        // forgot-password request, most already used or expired) keeps it cheap in practice.
        var candidates = await _unitOfWork.PasswordResetTokens.GetAllAsync(cancellationToken);
        var matched = candidates.FirstOrDefault(t => _passwordHasher.VerifyPassword(dto.Token, t.TokenHash));

        if (matched is null)
        {
            throw new InvalidOperationException("This password reset link is invalid.");
        }

        // Checked as two distinct, separately-worded conditions on purpose — the story's own AC
        // calls for clear, distinct messaging on expiry specifically, and "already used" is a
        // materially different situation (this exact link already did its job once) from "expired"
        // (it simply timed out) from the user's point of view.
        if (matched.IsUsed)
        {
            throw new InvalidOperationException("This password reset link has already been used.");
        }

        if (matched.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new InvalidOperationException("This password reset link has expired.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(matched.UserId, cancellationToken)
            ?? throw new InvalidOperationException("This password reset link is invalid.");

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        // A real recovery, not the forced-first-login case ResetPasswordAsync above exists for —
        // clear this so the user isn't immediately forced through THAT flow too right after just
        // resetting via email (confirmed behavior this ticket asks for explicitly).
        user.MustResetPassword = false;

        matched.IsUsed = true;
        matched.UsedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ResetPasswordWithTokenResponseDto("Your password has been reset. You can now log in.");
    }

    // 256 bits of entropy, base64url-encoded so it's safe to place directly in a URL query string
    // with no extra escaping needed (+ and / replaced, padding trimmed).
    private static string GenerateRawResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static AuthUserDto MapAuthUser(User user)
    {
        var permissions = user.Role.RolePermissions
            .Where(rp => rp.IsActive && rp.Permission.IsActive)
            .Select(rp => rp.Permission.PermissionNameEn)
            .Concat(user.UserPermissions
                .Where(up => up.IsActive && up.Permission.IsActive)
                .Select(up => up.Permission.PermissionNameEn))
            .Concat(user.UserGroups
                .Where(ug => ug.Group.IsActive)
                .SelectMany(ug => ug.Group.GroupPermissions)
                .Where(gp => gp.IsActive && gp.Permission.IsActive)
                .Select(gp => gp.Permission.PermissionNameEn))
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToList();

        return new AuthUserDto(
            user.UserId,
            user.Email,
            user.FirstNameEn,
            user.LastNameEn,
            user.FirstNameAr,
            user.LastNameAr,
            user.RoleId,
            user.Role.RoleNameEn,
            user.Role.RoleNameAr,
            user.EntityType,
            user.EntityId,
            permissions);
    }
}
