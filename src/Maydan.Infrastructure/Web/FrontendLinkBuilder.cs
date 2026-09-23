using Maydan.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Maydan.Infrastructure.Web;

public class FrontendLinkBuilder : IFrontendLinkBuilder
{
    private readonly IConfiguration _configuration;

    public FrontendLinkBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Reuses Cors:AllowedOrigins[0] (the same setting Program.cs's CORS policy already reads) as
    // the frontend origin, rather than introducing a second, easy-to-drift config key for the same
    // value. /auth/reset-password-with-token is the frontend route Step 2 of this ticket adds
    // (auth.routes.ts) — reads the token from this exact query param name.
    public string BuildResetPasswordLink(string token)
    {
        // GetChildren() rather than .Get&lt;string[]&gt;() — that binder extension lives in a
        // package this project doesn't otherwise need; this reads the same config values without it.
        var firstOrigin = _configuration.GetSection("Cors:AllowedOrigins").GetChildren().FirstOrDefault()?.Value;
        var baseUrl = (firstOrigin ?? "http://localhost:4200").TrimEnd('/');

        return $"{baseUrl}/auth/reset-password-with-token?token={Uri.EscapeDataString(token)}";
    }
}
