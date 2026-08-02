using ZapWatch.Tests.Fakes;
using ZapWatch.Web.Hubs;
using ZapWatch.Web.Models;

namespace ZapWatch.Tests;

public class AutomationStatusBroadcastTests
{
    [Fact]
    public async Task SendAsync_TargetsTheOwningUsersGroup_NotEveryone()
    {
        var hubContext = new FakeHubContext();
        var automation = new Automation
        {
            Id = 7,
            UserId = "owner-user-id",
            Name = "Test",
            PingToken = "token",
            ExpectedFrequencyMinutes = 60,
            GracePeriodMinutes = 15,
            Status = AutomationStatus.Overdue,
            LastSeenAt = DateTime.UtcNow
        };

        await AutomationStatusBroadcast.SendAsync(hubContext, automation);

        var message = Assert.Single(hubContext.FakeClients.Sent);
        Assert.Equal("owner-user-id", message.GroupName);
        Assert.Equal("AutomationStatusChanged", message.Method);
        Assert.Equal(7, message.Args[0]);
        Assert.Equal("Overdue", message.Args[1]);
    }
}
