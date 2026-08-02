using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

public static class SubscriptionLimits
{
    /// <summary>Automations a user can create with no active paid subscription - lets them
    /// verify the ping mechanic works (PRD's core flow) before paying anything.</summary>
    public const int FreeAutomationLimit = 1;

    public static async Task<int> GetEffectiveLimitAsync(ApplicationDbContext db, string userId)
    {
        var activeLimit = await db.Subscriptions
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .Select(s => (int?)s.AutomationLimit)
            .OrderByDescending(limit => limit)
            .FirstOrDefaultAsync();

        return activeLimit ?? FreeAutomationLimit;
    }
}
