using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ZapWatch.Web.Controllers;
using ZapWatch.Web.Data;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

namespace ZapWatch.Tests;

public class RazorpayWebhookControllerTests
{
    private const string WebhookSecret = "whsec_test";

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string ComputeHmac(string body, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hash = HMACSHA256.HashData(keyBytes, bodyBytes);
        return Convert.ToHexStringLower(hash);
    }

    private static string BuildPayload(string eventType, string subscriptionId) =>
        $$"""
        {
          "event": "{{eventType}}",
          "payload": { "subscription": { "entity": { "id": "{{subscriptionId}}", "status": "active" } } }
        }
        """;

    private static RazorpayWebhookController CreateController(ApplicationDbContext db, string body, string? signatureOverride = null)
    {
        var options = Options.Create(new RazorpayOptions { WebhookSecret = WebhookSecret });
        var controller = new RazorpayWebhookController(db, options, NullLogger<RazorpayWebhookController>.Instance);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        httpContext.Request.Headers["X-Razorpay-Signature"] = signatureOverride ?? ComputeHmac(body, WebhookSecret);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return controller;
    }

    [Fact]
    public async Task Receive_ActivatedEvent_SetsSubscriptionActive()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Created
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var body = BuildPayload("subscription.activated", "sub_123");
        var controller = CreateController(db, body);

        var result = await controller.Receive();

        Assert.IsType<OkResult>(result);
        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Active, updated!.Status);
    }

    [Fact]
    public async Task Receive_ChargedEvent_KeepsSubscriptionActive()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Active
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var body = BuildPayload("subscription.charged", "sub_123");
        var controller = CreateController(db, body);

        await controller.Receive();

        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Active, updated!.Status);
    }

    [Fact]
    public async Task Receive_HaltedEvent_SetsSubscriptionHalted()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Active
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var body = BuildPayload("subscription.halted", "sub_123");
        var controller = CreateController(db, body);

        await controller.Receive();

        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Halted, updated!.Status);
    }

    [Fact]
    public async Task Receive_CancelledEvent_SetsSubscriptionCancelled()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Active
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var body = BuildPayload("subscription.cancelled", "sub_123");
        var controller = CreateController(db, body);

        await controller.Receive();

        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Cancelled, updated!.Status);
    }

    [Fact]
    public async Task Receive_InvalidSignature_ReturnsUnauthorized_AndDoesNotChangeStatus()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Created
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        var body = BuildPayload("subscription.activated", "sub_123");
        // Simulates a forged/tampered request: signature doesn't match the body+secret.
        var controller = CreateController(db, body, signatureOverride: "0000not-a-real-signature0000");

        var result = await controller.Receive();

        Assert.IsType<UnauthorizedResult>(result);
        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Created, updated!.Status);
    }

    [Fact]
    public async Task Receive_UnknownSubscriptionId_ReturnsOkAndDoesNotCrash()
    {
        await using var db = CreateContext();
        var body = BuildPayload("subscription.activated", "sub_does_not_exist");
        var controller = CreateController(db, body);

        var result = await controller.Receive();

        Assert.IsType<OkResult>(result);
        Assert.Empty(await db.Subscriptions.ToListAsync());
    }

    [Fact]
    public async Task Receive_UnhandledEventType_LeavesExistingStatusUntouched()
    {
        await using var db = CreateContext();
        var sub = new Subscription
        {
            UserId = "u1", Plan = "Starter", AutomationLimit = 5,
            RazorpaySubscriptionId = "sub_123", Status = SubscriptionStatus.Active
        };
        db.Subscriptions.Add(sub);
        await db.SaveChangesAsync();

        // subscription.authenticated fires before activation on some flows - must not
        // prematurely flip status away from whatever it currently is.
        var body = BuildPayload("subscription.authenticated", "sub_123");
        var controller = CreateController(db, body);

        var result = await controller.Receive();

        Assert.IsType<OkResult>(result);
        var updated = await db.Subscriptions.FindAsync(sub.Id);
        Assert.Equal(SubscriptionStatus.Active, updated!.Status);
    }
}
