using Microsoft.AspNetCore.SignalR;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Hubs;

/// <summary>
/// The one place that knows the "AutomationStatusChanged" event shape, shared by the watchdog
/// job and the ping controller (TRD's two emit points) so they can't drift out of sync with
/// each other or with the JS client's handler signature.
/// </summary>
public static class AutomationStatusBroadcast
{
    public static Task SendAsync(IHubContext<AutomationStatusHub> hub, Automation automation, CancellationToken ct = default)
    {
        return hub.Clients.Group(automation.UserId).SendAsync(
            "AutomationStatusChanged",
            automation.Id,
            automation.Status.ToString(),
            automation.LastSeenAt,
            ct);
    }
}
