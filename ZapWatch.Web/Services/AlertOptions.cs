namespace ZapWatch.Web.Services;

public class ResendOptions
{
    public string ApiKey { get; set; } = "";
    public string FromAddress { get; set; } = "ZapWatch Alerts <onboarding@resend.dev>";
}

public class TwilioOptions
{
    public string AccountSid { get; set; } = "";
    public string AuthToken { get; set; } = "";
    public string FromPhoneNumber { get; set; } = "";
}
