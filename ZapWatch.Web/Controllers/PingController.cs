using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Hubs;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Controllers;

[AllowAnonymous]
[Route("ping")]
public class PingController(ApplicationDbContext db, IHubContext<AutomationStatusHub> hub) : Controller
{
    [HttpGet("{token}")]
    [HttpPost("{token}")]
    public async Task<IActionResult> Receive(string token)
    {
        var automation = await db.Automations
            .FirstOrDefaultAsync(a => a.PingToken == token);

        if (automation is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        automation.LastSeenAt = now;
        db.PingEvents.Add(new PingEvent { AutomationId = automation.Id, ReceivedAt = now });

        if (automation.Status == AutomationStatus.Overdue)
        {
            automation.Status = AutomationStatus.Ok;

            var openAlert = await db.AlertEvents
                .Where(a => a.AutomationId == automation.Id && a.ResolvedAt == null)
                .OrderByDescending(a => a.TriggeredAt)
                .FirstOrDefaultAsync();

            if (openAlert is not null)
            {
                openAlert.ResolvedAt = now;
            }
        }

        await db.SaveChangesAsync();

        await AutomationStatusBroadcast.SendAsync(hub, automation);

        return Ok();
    }
}
