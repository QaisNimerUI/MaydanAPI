using System.Security.Cryptography;
using System.Text;
using Maydan.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Maydan.Infrastructure.Security;

public class HmacCivilIdHasher : ICivilIdHasher
{
    private readonly byte[] _key;

    public HmacCivilIdHasher(IConfiguration configuration)
    {
        var key = configuration["Security:CivilIdHashKey"]?? throw new InvalidOperationException("Security:CivilIdHashKey is not configured. Set it via user-secrets or the Security__CivilIdHashKey environment variable.");
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string ComputeHash(string civilId)
    {
        using var hmac = new HMACSHA256(_key);
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(civilId));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
