using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;

namespace QualiTrack.Services;

public class ConfigService
{
    private readonly AppDbContext _db;
    private string? _cachedTimeZone;
    private string? _cachedDateFormat;
    private DateTime _cacheExpiry;

    public ConfigService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> GetTimeZoneAsync()
    {
        if (_cachedTimeZone == null || DateTime.UtcNow > _cacheExpiry)
        {
            var config = await _db.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == "TimeZone");
            _cachedTimeZone = config?.Value ?? "Asia/Jakarta";
            _cacheExpiry = DateTime.UtcNow.AddMinutes(5); // Cache 5 Menit
        }

        return _cachedTimeZone;
    }

    public async Task<string> GetDateFormatAsync()
    {
        if (_cachedDateFormat == null || DateTime.UtcNow > _cacheExpiry)
        {
            var config = await _db.SystemConfigs
                .FirstOrDefaultAsync(c => c.Key == "DateFormat");
            _cachedDateFormat = config?.Value ?? "dd-MM-yyyy";
        }

        return _cachedDateFormat;
    }
}