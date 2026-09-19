using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Maydan.Domain.Entities;

namespace Maydan.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
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
