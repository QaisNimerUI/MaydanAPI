using System.Security.Claims;
using Maydan.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Maydan.API.Filters;

// System Configuration gate (MAYD-133, 2026-09-24) — Business Rule #5: "If the super admin login
// and the required system configuration has not been completed, the system shall redirect the user
// to the Configuration page after login (can't do anything without set the configuration - all
// api's are blocked)."
//
// Scope confirmed with Yousef (2026-09-23): "all api's are blocked" means only the super admin's
// OWN account is blocked, not the whole system — gating every role behind one admin's setup step
// was rejected as too high-blast-radius (would stop worker attendance/QR scanning, payroll,
// association/production-company operations, etc. if the super admin simply hasn't gotten to it
// yet). ShouldBlockAsync (Application layer, unit tested there) already returns false for anyone
// who isn't the super admin — this filter adds no additional role logic of its own, it only wires
// that decision into the MVC pipeline and produces the distinct blocked response.
//
// Registered globally (Program.cs: options.Filters.Add<SystemConfigurationGateFilter>()) rather
// than as a per-controller [Authorize]-style attribute, so a newly added controller is gated by
// default and has to opt OUT (via [BypassSystemConfigurationGate]) rather than opt in and risk
// being forgotten — matches Business Rule #5's "all api's" framing more literally than an opt-in
// mechanism would.
public class SystemConfigurationGateFilter : IAsyncActionFilter
{
    // 423 Locked: semantically "the resource is locked until a precondition is met" — a real,
    // distinct HTTP status the frontend can key off reliably, deliberately NOT reused from 401/403
    // (which a real permission failure also returns, and which the frontend's existing
    // auth.interceptor already treats as "log the user out" — see that interceptor's own comment).
    // The response body's `code` field is the actual contract the frontend checks; the status is a
    // second, defense-in-depth signal.
    public const int BlockedStatusCode = 423;
    public const string BlockedResponseCode = "SYSTEM_CONFIGURATION_REQUIRED";

    private readonly ISystemConfigurationGateService _gateService;

    public SystemConfigurationGateFilter(ISystemConfigurationGateService gateService)
    {
        _gateService = gateService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (HasBypass(context.ActionDescriptor) || context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        if (!TryGetCurrentUserId(context.HttpContext.User, out var currentUserId))
        {
            await next();
            return;
        }

        if (await _gateService.ShouldBlockAsync(currentUserId, context.HttpContext.RequestAborted))
        {
            context.Result = new ObjectResult(new
            {
                code = BlockedResponseCode,
                message = "System configuration is required before this account can be used."
            })
            {
                StatusCode = BlockedStatusCode
            };
            return;
        }

        await next();
    }

    private static bool HasBypass(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            return false;
        }

        return controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(BypassSystemConfigurationGateAttribute), true).Length > 0
            || controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(BypassSystemConfigurationGateAttribute), true).Length > 0;
    }

    // Same claim precedence as ApiControllerBase.TryGetCurrentUserId (sub/NameIdentifier), minus
    // the X-Current-User-Id debug-header fallback — that fallback exists for controllers to work
    // without a real bearer token in ad hoc testing, which would defeat the point of a gate that's
    // specifically about authenticated identity.
    private static bool TryGetCurrentUserId(ClaimsPrincipal user, out int currentUserId)
    {
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return int.TryParse(claimValue, out currentUserId);
    }
}
