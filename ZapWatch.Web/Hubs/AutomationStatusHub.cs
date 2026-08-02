using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ZapWatch.Web.Hubs;

/// <summary>
/// One hub, scoped per authenticated user via SignalR groups keyed to the user's id - not a
/// plain broadcast. Getting this wrong leaks one customer's automation status to another
/// customer's browser (see TRD.md), so every connection is placed into its own user-id group
/// on connect, and every emit targets that group specifically, never Clients.All.
/// </summary>
[Authorize]
public class AutomationStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier!);
        await base.OnConnectedAsync();
    }
}
