using backend.DTOs.Goal;
using backend.Wrapper;

namespace backend.Features.Goal;

public interface IGoalService
{
    Task<PagedResult<Models.Goal>> Index(GoalFilteringRequest filter, int userId);
    Task<Models.Goal?> GetById(int id, int userId);
    Task<Models.Goal> Create(CreateGoalRequest request, int userId);
    Task<Models.Goal?> Update(int id, UpdateGoalRequest request, int userId);
    Task<bool> SoftDelete(int id, int userId);
    Task<bool> Delete(int id, int userId);
}
