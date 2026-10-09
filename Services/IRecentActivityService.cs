using QualiTrack.DTOs;

namespace QualiTrack.Services;

public interface IRecentActivityService
{
    Task<List<RecentActivityDto>> GetUserRecentActivityAsync(Guid userId, int limit = 10);
}
