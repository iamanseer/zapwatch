using System.ComponentModel.DataAnnotations;

namespace ZapWatch.Web.Models;

public class Automation
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int ExpectedFrequencyMinutes { get; set; }

    [Range(0, int.MaxValue)]
    public int GracePeriodMinutes { get; set; }

    [Required]
    public string PingToken { get; set; } = null!;

    public DateTime? LastSeenAt { get; set; }

    public AutomationStatus Status { get; set; } = AutomationStatus.Ok;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PingEvent> PingEvents { get; set; } = [];
    public List<AlertEvent> AlertEvents { get; set; } = [];
}
