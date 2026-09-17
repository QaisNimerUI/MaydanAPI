using Maydan.Application.DTOs.Auth;

namespace Maydan.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(AuthUserDto user);
}
