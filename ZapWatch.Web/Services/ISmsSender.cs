namespace ZapWatch.Web.Services;

public interface ISmsSender
{
    Task SendAsync(string toPhoneNumber, string body, CancellationToken ct = default);
}
