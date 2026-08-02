using ZapWatch.Web.Services;

namespace ZapWatch.Web.Models.Subscriptions;

public class SubscriptionIndexViewModel
{
    public IReadOnlyList<PlanDefinition> Plans { get; set; } = [];
    public Subscription? Current { get; set; }
}
