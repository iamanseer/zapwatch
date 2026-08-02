using ZapWatch.Web.Services;

namespace ZapWatch.Web.Models.Subscriptions;

public class SubscriptionIndexViewModel
{
    public IReadOnlyList<PlanDefinition> Plans { get; set; } = [];
    public Subscription? Current { get; set; }

    // Current.Plan is just a name/AutomationLimit snapshot on the Subscription row - this is
    // the live PlanCatalog entry for it, so the view can show its price without importing
    // IConfiguration itself.
    public PlanDefinition? CurrentPlanDefinition { get; set; }
}
