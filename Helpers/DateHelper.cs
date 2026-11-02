namespace QualiTrack.Helpers;

public static class DateHelper
{
    private static readonly TimeZoneInfo WibZone = ResolveWibZone();

    public static DateOnly TodayWib() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WibZone));

    private static TimeZoneInfo ResolveWibZone()
    {
        foreach (var id in new[] { "Asia/Jakarta", "SE Asia Standard Time " })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
        }

        // Fallback WIB tetap UTC+7 dan Indonesia tidak memakai DST
        return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
    }
}