namespace Maydan.API.Filters;

// System Configuration gate (MAYD-133, 2026-09-24): marks an action (or a whole controller) as
// always reachable by the super admin regardless of configuration state — Business Rule #5's own
// carve-out ("all api's are blocked" except the configuration screen itself and auth/logout). See
// SystemConfigurationGateFilter for where this is actually checked.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class BypassSystemConfigurationGateAttribute : Attribute
{
}
