using Maydan.API.Controllers;
using Maydan.API.Filters;

namespace Maydan.API.IntegrationTests;

// System Configuration gate (MAYD-133, 2026-09-24): the actual block/allow DECISION is a plain
// Application-layer service (SystemConfigurationGateServiceTests, Maydan.Application.Tests) —
// this project covers the one thing that lives only in the API layer: which controllers are wired
// to bypass the gate filter entirely. SystemConfigurationGateFilter itself only special-cases
// [BypassSystemConfigurationGate] before ever calling into the (already-tested) gate service, so
// proving the attribute lands on the right controllers is real coverage of the wiring, not a
// re-test of the decision logic itself.
public class SystemConfigurationGateFilterTests
{
    [Fact]
    public void SystemConfigurationController_bypasses_the_gate_so_the_super_admin_can_always_reach_it()
    {
        Assert.True(HasBypassAttribute(typeof(SystemConfigurationController)));
    }

    [Fact]
    public void AuthController_bypasses_the_gate_so_login_and_logout_always_work()
    {
        Assert.True(HasBypassAttribute(typeof(AuthController)));
    }

    [Theory]
    [InlineData(typeof(ProjectsController))]
    [InlineData(typeof(UsersController))]
    [InlineData(typeof(GroupsController))]
    [InlineData(typeof(EntityOnboardingController))]
    public void An_ordinary_controller_does_not_carry_the_bypass_attribute(Type controllerType)
    {
        // Business Rule #5 reads as "all api's are blocked" by default — the gate is registered
        // globally (Program.cs) and controllers opt OUT via the attribute, rather than opting in,
        // specifically so a newly added controller is gated unless someone deliberately exempts it.
        Assert.False(HasBypassAttribute(controllerType));
    }

    private static bool HasBypassAttribute(Type controllerType) =>
        controllerType.GetCustomAttributes(typeof(BypassSystemConfigurationGateAttribute), true).Length > 0;
}
