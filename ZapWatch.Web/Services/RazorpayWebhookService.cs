using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZapWatch.Web.Models;

namespace ZapWatch.Web.Services;

/// <summary>
/// Pure logic for the Razorpay webhook - the TRD-flagged payment path where a bug either bills
/// someone wrongly or fails to activate a paying account. Kept free of DB/HTTP so it's directly
/// unit-testable, same pattern as WatchdogService for the other TRD-flagged path.
/// </summary>
public static class RazorpayWebhookService
{
    /// <summary>Verifies the X-Razorpay-Signature header: HMAC-SHA256 of the raw request body
    /// using the webhook secret, hex-encoded. This is the only thing standing between "a real
    /// Razorpay event" and "anyone on the internet POSTing a fake activation."</summary>
    public static bool VerifySignature(string rawBody, string? signatureHeader, string webhookSecret)
    {
        if (string.IsNullOrEmpty(signatureHeader) || string.IsNullOrEmpty(webhookSecret))
        {
            return false;
        }

        var keyBytes = Encoding.UTF8.GetBytes(webhookSecret);
        var bodyBytes = Encoding.UTF8.GetBytes(rawBody);
        var computedHash = HMACSHA256.HashData(keyBytes, bodyBytes);
        var computedSignature = Convert.ToHexStringLower(computedHash);

        var computedBytes = Encoding.UTF8.GetBytes(computedSignature);
        var providedBytes = Encoding.UTF8.GetBytes(signatureHeader.Trim().ToLowerInvariant());

        return computedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(computedBytes, providedBytes);
    }

    /// <summary>Maps a Razorpay event name to our subscription status. Returns null for event
    /// types we don't act on (e.g. subscription.authenticated on its own) - the caller should
    /// leave the subscription's status untouched in that case, not clear it.</summary>
    public static SubscriptionStatus? MapEventToStatus(string eventType) => eventType switch
    {
        "subscription.activated" => SubscriptionStatus.Active,
        "subscription.charged" => SubscriptionStatus.Active,
        "subscription.halted" => SubscriptionStatus.Halted,
        "subscription.pending" => SubscriptionStatus.Halted,
        "subscription.cancelled" => SubscriptionStatus.Cancelled,
        "subscription.completed" => SubscriptionStatus.Cancelled,
        "subscription.expired" => SubscriptionStatus.Cancelled,
        _ => null
    };

    /// <summary>Pulls the Razorpay subscription id out of a webhook payload's
    /// payload.subscription.entity.id, if present.</summary>
    public static string? TryExtractSubscriptionId(JsonElement root)
    {
        if (root.TryGetProperty("payload", out var payload) &&
            payload.TryGetProperty("subscription", out var subscription) &&
            subscription.TryGetProperty("entity", out var entity) &&
            entity.TryGetProperty("id", out var idProp))
        {
            return idProp.GetString();
        }

        return null;
    }
}
