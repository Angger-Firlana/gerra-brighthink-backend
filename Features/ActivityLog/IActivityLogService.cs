using backend.DTOs.ActivityLog;
using backend.Enum;
using backend.Wrapper;

namespace backend.Features.ActivityLog;

public interface IActivityLogService
{
    System.Threading.Tasks.Task<PagedResult<Models.ActivityLog>> Index(ActivityLogFilteringRequest filter, int userId);
    System.Threading.Tasks.Task Log(int userId, int entityId, EntityType entityType, string action);
}
