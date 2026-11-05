namespace QualiTrack.Helpers;

public static class CacheKeys
{
    public static string DashboardSummary(int year) => $"dashboard_summary_{year}";
    public static string ComplianceScore(int year) => $"compliance_score_{year}";
    public static string MonthlyReport(int month, int year) => $"monthly_report_{month}_{year}";
    public static string QualityTrends(int year) => $"quality_trends_{year}";
    public static string DepartmentTrend(string timeframe) => $"dept_trend_{timeframe}";
}