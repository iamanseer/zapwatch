using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

public record AlertNotifyResult(string SucceededChannels, string? FailureDetails);

public interface IAlertNotifier
{
    /// <summary>Sends the alert for a newly-overdue automation.</summary>
    Task<AlertNotifyResult> NotifyAsync(Automation automation, CancellationToken ct = default);
}
