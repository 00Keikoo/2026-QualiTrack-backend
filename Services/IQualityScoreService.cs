using QualiTrack.DTOs;

namespace QualiTrack.Services;

public interface IQualityScoreService
{
    Task<List<QualityTrendDto>> GetQualityTrendsAsync(int year);
    Task<List<DepartmentTrendDto>> GetDepartmentComplianceTrendAsync(string timeframe);
}
