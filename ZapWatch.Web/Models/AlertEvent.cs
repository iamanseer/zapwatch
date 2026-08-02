namespace ZapWatch.Web.Models;

public class AlertEvent
{
    public int Id { get; set; }

    public int AutomationId { get; set; }
    public Automation Automation { get; set; } = null!;

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Comma-separated list of channels successfully notified for this episode, e.g. "email,sms".</summary>
    public string Channel { get; set; } = "";

    /// <summary>Error details for any channel that failed, e.g. "sms: Twilio send failed (400): ...".
    /// Null if every attempted channel succeeded.</summary>
    public string? FailureDetails { get; set; }
}
