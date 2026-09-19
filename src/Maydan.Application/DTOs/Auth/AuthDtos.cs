using Maydan.Domain.Enums;

namespace Maydan.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record ResetPasswordDto(string Email, string CurrentPassword, string NewPassword);

public record LoginResponseDto(
    bool IsAuthenticated,
    bool MustResetPassword,
    string? AccessToken,
    DateTime? AccessTokenExpiresAtUtc,
    AuthUserDto? User,
    string? Message);

public record AuthUserDto(
    int UserId,
    string Email,
    string FirstNameEn,
    string LastNameEn,
    string FirstNameAr,
    string LastNameAr,
    int RoleId,
    string RoleNameEn,
    string RoleNameAr,
    EntityType EntityType,
    int EntityId,
    IReadOnlyList<string> Permissions);
