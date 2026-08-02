using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace ZapWatch.Tests.Fakes;

public class FakeHubCallerContext(string connectionId, string? userIdentifier) : HubCallerContext
{
    public override string ConnectionId { get; } = connectionId;
    public override string? UserIdentifier { get; } = userIdentifier;
    public override ClaimsPrincipal? User { get; } = null;
    public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
    public override IFeatureCollection Features { get; } = new FeatureCollection();
    public override CancellationToken ConnectionAborted { get; } = CancellationToken.None;

    public override void Abort()
    {
    }
}
