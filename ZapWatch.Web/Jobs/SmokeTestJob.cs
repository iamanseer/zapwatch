using Microsoft.AspNetCore.SignalR;
using ZapWatch.Web.Hubs;

namespace ZapWatch.Web.Jobs;

public class SmokeTestJob
{
    private readonly IHubContext<AutomationStatusHub> _hubContext;

    public SmokeTestJob(IHubContext<AutomationStatusHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Ping()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        await _hubContext.Clients.All.SendAsync("SmokeTestTick", timestamp);
    }
}
