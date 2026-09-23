namespace Maydan.Application.Interfaces;

// Forgot-password recovery (2026-09-23): keeps AuthService (Application layer) free of any direct
// configuration/hosting dependency, matching this codebase's existing boundary (Maydan.Application
// has no EF Core reference either — see its own .csproj, and Program.cs's own comment on keeping
// Maydan.Infrastructure free of ASP.NET Core hosting types for the same reason, one layer over).
// The real implementation (Maydan.Infrastructure/Web/FrontendLinkBuilder.cs) just reads the
// frontend origin from config — the same Cors:AllowedOrigins setting CORS itself already uses, so
// no new config key was introduced for this.
public interface IFrontendLinkBuilder
{
    string BuildResetPasswordLink(string token);
}
