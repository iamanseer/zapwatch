namespace ZapWatch.Web.Models.Subscriptions;

public class CheckoutViewModel
{
    public string RazorpayKeyId { get; set; } = null!;
    public string RazorpaySubscriptionId { get; set; } = null!;
    public string PlanName { get; set; } = null!;
    public int PriceInRupees { get; set; }
    public string UserEmail { get; set; } = null!;
}
