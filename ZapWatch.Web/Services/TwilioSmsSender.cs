using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace ZapWatch.Web.Services;

public class TwilioSmsSender(HttpClient httpClient, IOptions<TwilioOptions> options) : ISmsSender
{
    public async Task SendAsync(string toPhoneNumber, string body, CancellationToken ct = default)
    {
        var opts = options.Value;
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{opts.AccountSid}/Messages.json";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        var authBytes = Encoding.ASCII.GetBytes($"{opts.AccountSid}:{opts.AuthToken}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = toPhoneNumber,
            ["From"] = opts.FromPhoneNumber,
            ["Body"] = body
        });

        using var response = await httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"Twilio send failed ({response.StatusCode}): {responseBody}");
        }
    }
}
