using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZapWatch.Web.Data;
using ZapWatch.Web.Services;

namespace ZapWatch.Web.Controllers;

[AllowAnonymous]
[Route("webhooks/razorpay")]
public class RazorpayWebhookController(
    ApplicationDbContext db,
    IOptions<RazorpayOptions> options,
    ILogger<RazorpayWebhookController> logger) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        var signatureHeader = Request.Headers["X-Razorpay-Signature"].ToString();

        if (!RazorpayWebhookService.VerifySignature(rawBody, signatureHeader, options.Value.WebhookSecret))
        {
            logger.LogWarning("Razorpay webhook signature verification failed.");
            return Unauthorized();
        }

        using var doc = JsonDocument.Parse(rawBody);
        var eventType = doc.RootElement.GetProperty("event").GetString() ?? "";

        var newStatus = RazorpayWebhookService.MapEventToStatus(eventType);
        if (newStatus is null)
        {
            // Not an event we act on (e.g. subscription.authenticated) - ack it so Razorpay
            // doesn't retry, but leave the subscription's current status untouched.
            return Ok();
        }

        var subscriptionId = RazorpayWebhookService.TryExtractSubscriptionId(doc.RootElement);
        if (subscriptionId is null)
        {
            logger.LogWarning("Razorpay webhook {Event} had no subscription id in payload.", eventType);
            return Ok();
        }

        var subscription = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.RazorpaySubscriptionId == subscriptionId);

        if (subscription is null)
        {
            logger.LogWarning("Razorpay webhook {Event} for unknown subscription {SubscriptionId}.", eventType, subscriptionId);
            return Ok();
        }

        subscription.Status = newStatus.Value;
        await db.SaveChangesAsync();

        return Ok();
    }
}
