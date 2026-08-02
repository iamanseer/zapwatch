namespace ZapWatch.Web.Models;

public class Subscription
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public string Plan { get; set; } = null!;
    public int AutomationLimit { get; set; }
    public string RazorpaySubscriptionId { get; set; } = null!;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Created;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
