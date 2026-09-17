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
