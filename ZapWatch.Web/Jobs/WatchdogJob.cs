using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

namespace ZapWatch.Web.Jobs;

public class WatchdogJob(ApplicationDbContext db, IAlertNotifier alertNotifier)
{
    public async Task ScanForOverdueAutomations()
    {
        var now = DateTime.UtcNow;

        var candidates = await db.Automations
            .Include(a => a.User)
            .Where(a => a.Status != AutomationStatus.Paused)
            .ToListAsync();

        foreach (var automation in candidates)
        {
            var isOverdue = WatchdogService.IsOverdue(
                automation.LastSeenAt, automation.ExpectedFrequencyMinutes, automation.GracePeriodMinutes, now);

            if (!isOverdue)
            {
                continue;
            }

            var hasOpenAlert = await db.AlertEvents
                .AnyAsync(a => a.AutomationId == automation.Id && a.ResolvedAt == null);

            if (hasOpenAlert)
            {
                // Already alerted for this quiet episode - dedup, don't spam on every tick.
                continue;
            }

            automation.Status = AutomationStatus.Overdue;

            var channel = await alertNotifier.NotifyAsync(automation);

            db.AlertEvents.Add(new AlertEvent
            {
                AutomationId = automation.Id,
                TriggeredAt = now,
                Channel = channel
            });
        }

        await db.SaveChangesAsync();
    }
}
