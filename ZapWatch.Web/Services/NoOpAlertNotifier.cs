using Microsoft.Extensions.Logging;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

/// <summary>Placeholder until Slice 5 wires up Resend/Twilio. Logs instead of sending.</summary>
public class NoOpAlertNotifier(ILogger<NoOpAlertNotifier> logger) : IAlertNotifier
{
    public Task<AlertNotifyResult> NotifyAsync(Automation automation, CancellationToken ct = default)
    {
        logger.LogWarning(
            "ALERT (not yet sent - notifier not configured): automation {AutomationId} ({Name}) is overdue.",
            automation.Id, automation.Name);
        return Task.FromResult(new AlertNotifyResult("", "notifier not configured"));
    }
}
