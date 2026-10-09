using QualiTrack.DTOs;

namespace QualiTrack.Services;

public interface IKpiService
{
    Task<KpiDto> GetUserKpiAsync(Guid userId);
}
