namespace ZapWatch.Web.Services;

public static class WatchdogService
{
    /// <summary>
    /// An automation with no ping yet has no baseline to measure against, so it is never
    /// considered overdue until its first ping arrives.
    /// </summary>
    public static bool IsOverdue(DateTime? lastSeenAt, int expectedFrequencyMinutes, int gracePeriodMinutes, DateTime now)
    {
        if (lastSeenAt is null)
        {
            return false;
        }

        var deadline = lastSeenAt.Value.AddMinutes(expectedFrequencyMinutes + gracePeriodMinutes);
        return now > deadline;
    }
}
