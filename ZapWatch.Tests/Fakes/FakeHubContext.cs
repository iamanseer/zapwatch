using Microsoft.AspNetCore.SignalR;
using ZapWatch.Web.Hubs;

namespace ZapWatch.Tests.Fakes;

public record SentMessage(string GroupName, string Method, object?[] Args);

public class FakeClientProxy(string groupName, List<SentMessage> sink) : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        sink.Add(new SentMessage(groupName, method, args));
        return Task.CompletedTask;
    }
}

public class FakeHubClients : IHubClients
{
    public List<SentMessage> Sent { get; } = [];

    public IClientProxy All => throw new NotSupportedException();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
    public IClientProxy Client(string connectionId) => throw new NotSupportedException();
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => throw new NotSupportedException();
    public IClientProxy Group(string groupName) => new FakeClientProxy(groupName, Sent);
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => throw new NotSupportedException();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
    public IClientProxy OthersInGroup(string groupName) => throw new NotSupportedException();
    public IClientProxy User(string userId) => throw new NotSupportedException();
    public IClientProxy Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
}

public class FakeGroupManager : IGroupManager
{
    public List<(string ConnectionId, string GroupName)> Added { get; } = [];

    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        Added.Add((connectionId, groupName));
        return Task.CompletedTask;
    }

    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        Added.RemoveAll(a => a.ConnectionId == connectionId && a.GroupName == groupName);
        return Task.CompletedTask;
    }
}

public class FakeHubContext : IHubContext<AutomationStatusHub>
{
    public FakeHubClients FakeClients { get; } = new();
    public IHubClients Clients => FakeClients;
    public IGroupManager Groups { get; } = new FakeGroupManager();
}
