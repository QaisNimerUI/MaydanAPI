namespace Maydan.Application.Interfaces;

// System Configuration gate (MAYD-133, 2026-09-24): the actual Business Rule #5 decision, kept as
// a plain Application-layer service (no ASP.NET Core types) so it's directly unit-testable without
// standing up a fake HTTP pipeline. SystemConfigurationGateFilter (Maydan.API) is the thin MVC glue
// that calls this and turns "true" into the distinct blocked response — see that class's own
// comment for the bypass-attribute mechanism it also applies before ever calling this.
public interface ISystemConfigurationGateService
{
    // True only when the caller IS the super admin (Bayt-AlUrdon, RoleId 1) AND configuration is
    // not yet complete. Every other role/entity type always gets false, regardless of configuration
    // state — this is deliberately NOT a system-wide gate (Business Rule #5, scope confirmed with
    // Yousef 2026-09-23: only the super admin's own account is restricted).
    Task<bool> ShouldBlockAsync(int currentUserId, CancellationToken cancellationToken = default);
}
