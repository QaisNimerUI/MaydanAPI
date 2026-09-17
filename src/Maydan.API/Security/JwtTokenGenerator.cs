using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Maydan.Application.DTOs.Auth;
using Maydan.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Maydan.API.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(AuthUserDto user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var accessTokenMinutes = int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var configuredMinutes)
            ? configuredMinutes
            : 15;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new("roleId", user.RoleId.ToString()),
            new(ClaimTypes.Role, user.RoleNameEn),
            new("roleNameEn", user.RoleNameEn),
            new("roleNameAr", user.RoleNameAr),
            new("entityType", user.EntityType.ToString()),
            new("entityId", user.EntityId.ToString())
        };

        claims.AddRange(user.Permissions.Select(permission => new Claim("permission", permission)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
