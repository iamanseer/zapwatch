namespace ZapWatch.Web.Models;

public class PingEvent
{
    public int Id { get; set; }

    public int AutomationId { get; set; }
    public Automation Automation { get; set; } = null!;

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
