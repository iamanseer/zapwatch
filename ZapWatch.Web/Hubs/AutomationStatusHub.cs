using Microsoft.AspNetCore.SignalR;

namespace ZapWatch.Web.Hubs;

public class AutomationStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
