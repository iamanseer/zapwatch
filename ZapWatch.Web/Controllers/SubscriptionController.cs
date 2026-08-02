using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Models;
using ZapWatch.Web.Models.Subscriptions;
using ZapWatch.Web.Services;

namespace ZapWatch.Web.Controllers;

[Authorize]
public class SubscriptionController(
    ApplicationDbContext db,
    IConfiguration config,
    UserManager<ApplicationUser> userManager,
    RazorpayClient razorpay) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    public async Task<IActionResult> Index()
    {
        var current = await db.Subscriptions
            .Where(s => s.UserId == CurrentUserId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        return View(new SubscriptionIndexViewModel
        {
            Plans = PlanCatalog.All(config),
            Current = current,
            CurrentPlanDefinition = current is null ? null : PlanCatalog.Find(config, current.Plan)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string plan)
    {
        var planDef = PlanCatalog.Find(config, plan);
        if (planDef is null)
        {
            return BadRequest();
        }

        var user = await userManager.GetUserAsync(User);
        var razorpaySubscriptionId = await razorpay.CreateSubscriptionAsync(planDef.RazorpayPlanId, user!.Email!);

        db.Subscriptions.Add(new Subscription
        {
            UserId = CurrentUserId,
            Plan = planDef.Name,
            AutomationLimit = planDef.AutomationLimit,
            RazorpaySubscriptionId = razorpaySubscriptionId,
            Status = SubscriptionStatus.Created
        });
        await db.SaveChangesAsync();

        return View("Checkout", new CheckoutViewModel
        {
            RazorpayKeyId = config["Razorpay:KeyId"]!,
            RazorpaySubscriptionId = razorpaySubscriptionId,
            PlanName = planDef.Name,
            PriceInRupees = planDef.PriceInRupees,
            UserEmail = user.Email!
        });
    }
}
