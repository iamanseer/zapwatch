using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ZapWatch.Web.Services;

public class RazorpayClient(HttpClient httpClient, IOptions<RazorpayOptions> options)
{
    private void AddAuth(HttpRequestMessage request)
    {
        var opts = options.Value;
        var authBytes = Encoding.ASCII.GetBytes($"{opts.KeyId}:{opts.KeySecret}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    /// <summary>Creates a Razorpay subscription for the given plan. total_count of 120 monthly
    /// cycles (10 years) emulates "until cancelled" - Razorpay subscriptions require a finite
    /// count, there's no literal "forever" option.</summary>
    public async Task<string> CreateSubscriptionAsync(string planId, string customerEmail, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.razorpay.com/v1/subscriptions");
        AddAuth(request);
        request.Content = JsonContent.Create(new
        {
            plan_id = planId,
            customer_notify = 1,
            total_count = 120,
            notes = new { email = customerEmail }
        });

        using var response = await httpClient.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Razorpay subscription creation failed ({response.StatusCode}): {body}");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetString()!;
    }
}
