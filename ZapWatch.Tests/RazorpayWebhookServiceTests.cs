using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

namespace ZapWatch.Tests;

public class RazorpayWebhookServiceTests
{
    private static string ComputeHmac(string body, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hash = HMACSHA256.HashData(keyBytes, bodyBytes);
        return Convert.ToHexStringLower(hash);
    }

    [Fact]
    public void VerifySignature_ValidSignature_ReturnsTrue()
    {
        var body = """{"event":"subscription.activated"}""";
        var secret = "test_webhook_secret";
        var signature = ComputeHmac(body, secret);

        Assert.True(RazorpayWebhookService.VerifySignature(body, signature, secret));
    }

    [Fact]
    public void VerifySignature_TamperedBody_ReturnsFalse()
    {
        var originalBody = """{"event":"subscription.activated"}""";
        var secret = "test_webhook_secret";
        var signatureForOriginal = ComputeHmac(originalBody, secret);

        // Attacker (or a bug) changes the body after the signature was computed for it.
        var tamperedBody = """{"event":"subscription.cancelled"}""";

        Assert.False(RazorpayWebhookService.VerifySignature(tamperedBody, signatureForOriginal, secret));
    }

    [Fact]
    public void VerifySignature_WrongSecret_ReturnsFalse()
    {
        var body = """{"event":"subscription.activated"}""";
        var signature = ComputeHmac(body, "correct_secret");

        Assert.False(RazorpayWebhookService.VerifySignature(body, signature, "wrong_secret"));
    }

    [Fact]
    public void VerifySignature_MissingSignatureHeader_ReturnsFalse()
    {
        Assert.False(RazorpayWebhookService.VerifySignature("{}", null, "secret"));
    }

    [Fact]
    public void VerifySignature_EmptyWebhookSecret_ReturnsFalse()
    {
        // Misconfiguration (secret never set) must fail closed, not verify everything.
        Assert.False(RazorpayWebhookService.VerifySignature("{}", "anything", ""));
    }

    [Theory]
    [InlineData("subscription.activated", SubscriptionStatus.Active)]
    [InlineData("subscription.charged", SubscriptionStatus.Active)]
    [InlineData("subscription.halted", SubscriptionStatus.Halted)]
    [InlineData("subscription.pending", SubscriptionStatus.Halted)]
    [InlineData("subscription.cancelled", SubscriptionStatus.Cancelled)]
    [InlineData("subscription.completed", SubscriptionStatus.Cancelled)]
    [InlineData("subscription.expired", SubscriptionStatus.Cancelled)]
    public void MapEventToStatus_KnownEvents_MapCorrectly(string eventType, SubscriptionStatus expected)
    {
        Assert.Equal(expected, RazorpayWebhookService.MapEventToStatus(eventType));
    }

    [Theory]
    [InlineData("subscription.authenticated")]
    [InlineData("payment.failed")]
    [InlineData("payment.captured")]
    [InlineData("some.unrelated.event")]
    public void MapEventToStatus_UnhandledEvents_ReturnNull(string eventType)
    {
        Assert.Null(RazorpayWebhookService.MapEventToStatus(eventType));
    }

    [Fact]
    public void TryExtractSubscriptionId_ValidPayload_ReturnsId()
    {
        const string json = """
        {
          "event": "subscription.activated",
          "payload": { "subscription": { "entity": { "id": "sub_abc123", "status": "active" } } }
        }
        """;
        using var doc = JsonDocument.Parse(json);

        Assert.Equal("sub_abc123", RazorpayWebhookService.TryExtractSubscriptionId(doc.RootElement));
    }

    [Fact]
    public void TryExtractSubscriptionId_NoSubscriptionInPayload_ReturnsNull()
    {
        const string json = """
        { "event": "payment.captured", "payload": { "payment": { "entity": { "id": "pay_xyz" } } } }
        """;
        using var doc = JsonDocument.Parse(json);

        Assert.Null(RazorpayWebhookService.TryExtractSubscriptionId(doc.RootElement));
    }
}
