namespace QualiTrack.Constants;

public static class IndonesiaTimeZones
{
    public const string WIB = "Asia/Jakarta";
    public const string WITA = "Asia/Makassar";
    public const string WIT = "Asia/Jayapura";

    public static readonly Dictionary<string, string> Zones = new()
    {
        {"WIB", WIB},
        {"WITA", WITA},
        {"WIT", WIT}
    };

    public static readonly Dictionary<string, int> UtcOffsets = new()
    {
        {"WIB", 7},
        {"WITA", 8},
        {"WIT", 9}
    };

    public static bool IsValid(string timezone)
    {
        return Zones.ContainsKey(timezone) || Zones.ContainsValue(timezone);
    }

    public static string GetSystemTimeZoneId(string zone)
    {
        return Zones.TryGetValue(zone, out var systemId) ? systemId : zone;
    }
}