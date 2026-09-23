using Maydan.Application.DTOs.Auth;

namespace Maydan.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(AuthUserDto user);

    // Real 3-week session persistence / "remember me" (MAYD-131/132, 2026-09-23). The raw refresh-
    // token VALUE and its sliding-window expiry — not a JWT, just cryptographically random like the
    // password-reset tokens (AuthService hashes it the same way before persisting, same convention
    // as PasswordResetToken). Deliberately lives here rather than as a hardcoded const in
    // AuthService: Maydan.Application has no Microsoft.Extensions.Configuration reference (the same
    // layering boundary Program.cs's own comment documents for IFrontendLinkBuilder), so
    // Jwt:RefreshTokenDays is read from real configuration in this Maydan.API-layer implementation,
    // the same way Jwt:AccessTokenMinutes already is for GenerateAccessToken above.
    (string RawToken, DateTime ExpiresAtUtc) GenerateRefreshToken();
}
