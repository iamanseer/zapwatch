using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Models;
using ZapWatch.Web.Models.Automations;

namespace ZapWatch.Web.Controllers;

[Authorize]
public class AutomationsController(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager) : Controller
{
    private string CurrentUserId => userManager.GetUserId(User)!;

    public async Task<IActionResult> Index()
    {
        var automations = await db.Automations
            .Where(a => a.UserId == CurrentUserId)
            .OrderBy(a => a.Name)
            .ToListAsync();

        return View(automations);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new AutomationFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AutomationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var automation = new Automation
        {
            UserId = CurrentUserId,
            Name = model.Name,
            ExpectedFrequencyMinutes = model.ExpectedFrequencyMinutes,
            GracePeriodMinutes = model.GracePeriodMinutes,
            PingToken = Guid.NewGuid().ToString("N"),
            Status = AutomationStatus.Ok
        };

        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var automation = await db.Automations
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (automation is null)
        {
            return NotFound();
        }

        return View(new AutomationFormViewModel
        {
            Id = automation.Id,
            Name = automation.Name,
            ExpectedFrequencyMinutes = automation.ExpectedFrequencyMinutes,
            GracePeriodMinutes = automation.GracePeriodMinutes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AutomationFormViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var automation = await db.Automations
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (automation is null)
        {
            return NotFound();
        }

        automation.Name = model.Name;
        automation.ExpectedFrequencyMinutes = model.ExpectedFrequencyMinutes;
        automation.GracePeriodMinutes = model.GracePeriodMinutes;

        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var automation = await db.Automations
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (automation is null)
        {
            return NotFound();
        }

        return View(automation);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var automation = await db.Automations
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == CurrentUserId);

        if (automation is null)
        {
            return NotFound();
        }

        db.Automations.Remove(automation);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
