using ZapWatch.Web.Services;

namespace ZapWatch.Tests;

public class WatchdogServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void NeverPinged_IsNotOverdue()
    {
        var result = WatchdogService.IsOverdue(
            lastSeenAt: null, expectedFrequencyMinutes: 60, gracePeriodMinutes: 15, now: Now);

        Assert.False(result);
    }

    [Fact]
    public void WellWithinWindow_IsNotOverdue()
    {
        var lastSeen = Now.AddMinutes(-10);

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 15, now: Now);

        Assert.False(result);
    }

    [Fact]
    public void ExactlyAtDeadline_IsNotOverdue()
    {
        // TRD: "now - last_seen_at > expected_frequency + grace_period" - strictly greater than,
        // so landing exactly on the deadline must not trigger a false alert.
        var lastSeen = Now.AddMinutes(-(60 + 15));

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 15, now: Now);

        Assert.False(result);
    }

    [Fact]
    public void OneMinutePastDeadline_IsOverdue()
    {
        var lastSeen = Now.AddMinutes(-(60 + 15 + 1));

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 15, now: Now);

        Assert.True(result);
    }

    [Fact]
    public void ZeroGracePeriod_OverdueImmediatelyAfterExpectedFrequency()
    {
        var lastSeen = Now.AddMinutes(-61);

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 0, now: Now);

        Assert.True(result);
    }

    [Fact]
    public void LongGracePeriod_NotOverdueWithinExtendedWindow()
    {
        // A quiet-hours-style long grace period should not false-positive mid-window.
        var lastSeen = Now.AddMinutes(-(60 + 500));

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 720, now: Now);

        Assert.False(result);
    }

    [Fact]
    public void FutureLastSeenAt_IsNotOverdue()
    {
        // Defensive: clock skew or a stray future timestamp shouldn't ever compute as overdue.
        var lastSeen = Now.AddMinutes(5);

        var result = WatchdogService.IsOverdue(
            lastSeenAt: lastSeen, expectedFrequencyMinutes: 60, gracePeriodMinutes: 15, now: Now);

        Assert.False(result);
    }
}
