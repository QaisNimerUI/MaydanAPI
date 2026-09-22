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

// Entity onboarding Stage 1 (2026-09-22): public production-company self-registration — company +
// its first admin in one submission, active immediately (confirmed product decision). Matches
// production-house-signup.component.ts's form fields field-for-field: that form collects a single
// (Latin-script) FirstName/LastName for the admin, not the Ar/En pair User.cs requires — see
// ProductionCompanyOnboardingService's own comment on how that gap is bridged for now.
public record RegisterProductionCompanyDto(
    string CompanyNameEn,
    string CompanyNameAr,
    string RegistrationNumber,
    int CityId,
    string AdminFirstName,
    string AdminLastName,
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
