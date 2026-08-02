using ZapWatch.Tests.Fakes;
using ZapWatch.Web.Hubs;

namespace ZapWatch.Tests;

/// <summary>
/// Per TRD/MODULES.md: SignalR connections must be scoped per authenticated user via groups
/// keyed to Context.UserIdentifier. Getting this wrong is a privacy bug (one customer seeing
/// another's automation status), not just a UX bug - worth a real test, not just a visual check.
/// </summary>
public class AutomationStatusHubTests
{
    [Fact]
    public async Task OnConnected_AddsConnectionToGroupNamedAfterTheUser()
    {
        var groups = new FakeGroupManager();
        var context = new FakeHubCallerContext(connectionId: "conn-1", userIdentifier: "user-42");
        var hub = new AutomationStatusHub { Context = context, Groups = groups };

        await hub.OnConnectedAsync();

        Assert.Contains(("conn-1", "user-42"), groups.Added);
    }

    [Fact]
    public async Task DifferentUsers_AreAddedToDifferentGroups()
    {
        var groups = new FakeGroupManager();

        var hubA = new AutomationStatusHub
        {
            Context = new FakeHubCallerContext("conn-a", "user-a"),
            Groups = groups
        };
        var hubB = new AutomationStatusHub
        {
            Context = new FakeHubCallerContext("conn-b", "user-b"),
            Groups = groups
        };

        await hubA.OnConnectedAsync();
        await hubB.OnConnectedAsync();

        Assert.Contains(("conn-a", "user-a"), groups.Added);
        Assert.Contains(("conn-b", "user-b"), groups.Added);
        // The critical assertion: user A's connection must never land in user B's group.
        Assert.DoesNotContain(("conn-a", "user-b"), groups.Added);
        Assert.DoesNotContain(("conn-b", "user-a"), groups.Added);
    }
}
