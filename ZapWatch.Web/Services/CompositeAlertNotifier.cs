using Microsoft.Extensions.Logging;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

/// <summary>
/// Sends both email and SMS independently for every alert. Per TRD: an SMS failure must not
/// block the email attempt, and vice versa - a monitoring tool that fails silently on its own
/// alert path is the one failure mode that can't be tolerated. If both channels fail, that's
/// logged at Critical so it's visible wherever the operator actually looks.
/// </summary>
public class CompositeAlertNotifier(
    IEmailSender emailSender,
    ISmsSender smsSender,
    ILogger<CompositeAlertNotifier> logger) : IAlertNotifier
{
    public async Task<string> NotifyAsync(Automation automation, CancellationToken ct = default)
    {
        var succeededChannels = new List<string>();
        var subject = $"ZapWatch alert: \"{automation.Name}\" has gone quiet";
        var body = $"\"{automation.Name}\" hasn't pinged in over {automation.ExpectedFrequencyMinutes + automation.GracePeriodMinutes} minutes. " +
                   "Check the automation - the next successful ping will clear this alert automatically.";

        var emailOk = false;
        var smsOk = false;

        if (!string.IsNullOrWhiteSpace(automation.User.Email))
        {
            try
            {
                await emailSender.SendAsync(automation.User.Email, subject, $"<p>{body}</p>", ct);
                emailOk = true;
                succeededChannels.Add("email");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email alert failed for automation {AutomationId}", automation.Id);
            }
        }

        if (!string.IsNullOrWhiteSpace(automation.User.PhoneNumber))
        {
            try
            {
                await smsSender.SendAsync(automation.User.PhoneNumber, body, ct);
                smsOk = true;
                succeededChannels.Add("sms");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SMS alert failed for automation {AutomationId}", automation.Id);
            }
        }

        if (!emailOk && !smsOk)
        {
            logger.LogCritical(
                "Both email and SMS alerts failed for automation {AutomationId} ({Name}) - operator was not notified.",
                automation.Id, automation.Name);
        }

        return string.Join(",", succeededChannels);
    }
}
