using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Jobs;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

namespace ZapWatch.Tests;

public class FakeAlertNotifier : IAlertNotifier
{
    public int CallCount { get; private set; }

    public Task<string> NotifyAsync(Automation automation, CancellationToken ct = default)
    {
        CallCount++;
        return Task.FromResult("email,sms");
    }
}

public class WatchdogJobTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<string> AddUserAsync(ApplicationDbContext db, string userId = "user-1")
    {
        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@example.com",
            Email = $"{userId}@example.com",
            PhoneNumber = "+15550000000"
        });
        await db.SaveChangesAsync();
        return userId;
    }

    private static Automation MakeOverdueAutomation(string userId = "user-1") => new()
    {
        UserId = userId,
        Name = "Test Zap",
        PingToken = Guid.NewGuid().ToString("N"),
        ExpectedFrequencyMinutes = 60,
        GracePeriodMinutes = 15,
        LastSeenAt = DateTime.UtcNow.AddHours(-3),
        Status = AutomationStatus.Ok
    };

    [Fact]
    public async Task OverdueAutomation_FlipsToOverdueAndOpensExactlyOneAlert()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        await job.ScanForOverdueAutomations();

        var updated = await db.Automations.FindAsync(automation.Id);
        Assert.Equal(AutomationStatus.Overdue, updated!.Status);

        var alerts = await db.AlertEvents.Where(a => a.AutomationId == automation.Id).ToListAsync();
        Assert.Single(alerts);
        Assert.Null(alerts[0].ResolvedAt);
        Assert.Equal(1, notifier.CallCount);
    }

    [Fact]
    public async Task RepeatedScansWhileStillOverdue_DoNotDuplicateAlerts()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        // Simulate the recurring job firing three times in a row while nothing has changed.
        await job.ScanForOverdueAutomations();
        await job.ScanForOverdueAutomations();
        await job.ScanForOverdueAutomations();

        var alerts = await db.AlertEvents.Where(a => a.AutomationId == automation.Id).ToListAsync();
        Assert.Single(alerts);
        Assert.Equal(1, notifier.CallCount);
    }

    [Fact]
    public async Task NewAlertCanOpenAgain_AfterPreviousOneWasResolved()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        await job.ScanForOverdueAutomations();

        // Simulate what PingController does on a successful ping: resolve + go quiet again later.
        var alert = await db.AlertEvents.SingleAsync(a => a.AutomationId == automation.Id);
        alert.ResolvedAt = DateTime.UtcNow;
        automation.Status = AutomationStatus.Ok;
        automation.LastSeenAt = DateTime.UtcNow.AddHours(-3); // immediately goes quiet again
        await db.SaveChangesAsync();

        await job.ScanForOverdueAutomations();

        var alerts = await db.AlertEvents.Where(a => a.AutomationId == automation.Id).ToListAsync();
        Assert.Equal(2, alerts.Count);
        Assert.Equal(2, notifier.CallCount);
    }

    [Fact]
    public async Task PausedAutomation_IsNeverFlaggedOverdue()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        automation.Status = AutomationStatus.Paused;
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        await job.ScanForOverdueAutomations();

        var updated = await db.Automations.FindAsync(automation.Id);
        Assert.Equal(AutomationStatus.Paused, updated!.Status);
        Assert.Empty(await db.AlertEvents.Where(a => a.AutomationId == automation.Id).ToListAsync());
        Assert.Equal(0, notifier.CallCount);
    }

    [Fact]
    public async Task HealthyAutomation_IsUntouched()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        automation.LastSeenAt = DateTime.UtcNow.AddMinutes(-5);
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        await job.ScanForOverdueAutomations();

        var updated = await db.Automations.FindAsync(automation.Id);
        Assert.Equal(AutomationStatus.Ok, updated!.Status);
        Assert.Empty(await db.AlertEvents.Where(a => a.AutomationId == automation.Id).ToListAsync());
    }

    [Fact]
    public async Task NeverPingedAutomation_IsNotFlaggedOverdue()
    {
        await using var db = CreateContext();
        await AddUserAsync(db);
        var automation = MakeOverdueAutomation();
        automation.LastSeenAt = null;
        db.Automations.Add(automation);
        await db.SaveChangesAsync();

        var notifier = new FakeAlertNotifier();
        var job = new WatchdogJob(db, notifier);

        await job.ScanForOverdueAutomations();

        var updated = await db.Automations.FindAsync(automation.Id);
        Assert.Equal(AutomationStatus.Ok, updated!.Status);
        Assert.Equal(0, notifier.CallCount);
    }
}
