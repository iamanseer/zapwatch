namespace ZapWatch.Web.Services;

public record PlanDefinition(string Name, string RazorpayPlanId, int AutomationLimit, int PriceInRupees);

/// <summary>
/// The 3 fixed v1 tiers per PRD §6 (flat monthly subscription, tiered by automation count).
/// Razorpay plan IDs come from the plans created once via their API - see appsettings for the
/// actual IDs; this catalog just maps our plan names to what the UI/backend needs to display and
/// gate on. A real Plans table would be overkill for 3 fixed tiers that don't change per-request.
/// </summary>
public static class PlanCatalog
{
    public static IReadOnlyList<PlanDefinition> All(IConfiguration config) =>
    [
        new("Starter", config["Razorpay:PlanIds:Starter"]!, 5, 5),
        new("Growth", config["Razorpay:PlanIds:Growth"]!, 15, 10),
        new("Scale", config["Razorpay:PlanIds:Scale"]!, 40, 20)
    ];

    public static PlanDefinition? Find(IConfiguration config, string planName) =>
        All(config).FirstOrDefault(p => string.Equals(p.Name, planName, StringComparison.OrdinalIgnoreCase));
}
