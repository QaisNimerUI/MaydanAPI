using Maydan.Domain.Enums;

namespace Maydan.Application.DTOs.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public record ResetPasswordDto(string Email, string CurrentPassword, string NewPassword);

// Forgot-password recovery (MAYD-128/129/130, 2026-09-23): deliberately separate from
// ResetPasswordDto above — that flow requires knowing the CURRENT password (forced-first-login /
// the existing "تغيير كلمة المرور" link); this one is the genuine "I don't know my password at
// all" recovery path, gated by proving control of the account's email instead.
public record ForgotPasswordDto(string Email);

// Always the same message whether or not the email is registered — see
// AuthService.ForgotPasswordAsync's own comment on why (email-enumeration safety, this story's own
// requirement).
public record ForgotPasswordResponseDto(string Message);

public record ResetPasswordWithTokenDto(string Token, string NewPassword, string ConfirmPassword);

public record ResetPasswordWithTokenResponseDto(string Message);

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

// Entity onboarding Stage 1 (2026-09-22): public production-company self-registration — company +
// its first admin in one submission, active immediately (confirmed product decision). Matches
// production-house-signup.component.ts's form fields field-for-field.
// Stage 3 (2026-09-22): AdminFirstNameAr/AdminLastNameAr added now that the form collects the
// admin's real Arabic name — naming mirrors AdminFirstName/AdminLastName's own "Admin" prefix,
// plus the "NameAr" suffix Stage 2's OnboardAssociationAdminDto already established.
public record RegisterProductionCompanyDto(
    string CompanyNameEn,
    string CompanyNameAr,
    string RegistrationNumber,
    int CityId,
    string AdminFirstName,
    string AdminLastName,
    string AdminFirstNameAr,
    string AdminLastNameAr,
    string MobileCountryCode,
    string MobileNumber,
    string Email,
    string Password);

public record RegisterProductionCompanyResponseDto(
    int ProductionCompanyId,
    string CompanyNameEn,
    string CompanyNameAr,
    int AdminUserId,
    string AdminEmail,
    string Message);
