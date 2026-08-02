using Hangfire.Dashboard;

namespace ZapWatch.Web.Infrastructure;

/// <summary>
/// Replaces Hangfire's default local-requests-only dashboard filter, which blocks any remote
/// access (including the operator's own browser). The dashboard isn't scoped per-tenant, so
/// this is restricted to the operator's own account specifically - not "any authenticated
/// user" - since a real customer could otherwise see every other customer's job history.
/// </summary>
public class HangfireDashboardAuthFilter(string adminEmail) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var email = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        return httpContext.User.Identity?.IsAuthenticated == true
            && string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase);
    }
}
