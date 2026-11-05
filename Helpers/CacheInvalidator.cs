using Microsoft.Extensions.Caching.Memory;

namespace QualiTrack.Helpers;

public class CacheInvalidator(IMemoryCache cache)
{
    public void InvalidateDashboard(int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        cache.Remove(CacheKeys.DashboardSummary(targetYear));
        cache.Remove(CacheKeys.ComplianceScore(targetYear));
    }

    public void InvalidateMonthlyReport(int? month = null, int? year = null)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;
        cache.Remove(CacheKeys.MonthlyReport(targetMonth, targetYear));
    }

    public void InvalidateQualityTrends(int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        cache.Remove(CacheKeys.QualityTrends(targetYear));
        // Invalidate all department trends
        foreach (var timeframe in new[] { "3m", "6m", "1y", "all" })
        {
            cache.Remove(CacheKeys.DepartmentTrend(timeframe));
        }
    }
}
