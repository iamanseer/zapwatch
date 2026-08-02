using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

public interface IAlertNotifier
{
    /// <summary>Sends the alert for a newly-overdue automation. Returns the comma-separated
    /// list of channels that succeeded (e.g. "email,sms"), for AlertEvent.Channel.</summary>
    Task<string> NotifyAsync(Automation automation, CancellationToken ct = default);
}
