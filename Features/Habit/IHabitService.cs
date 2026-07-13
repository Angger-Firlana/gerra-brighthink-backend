using backend.DTOs.Habit;
using backend.Wrapper;

namespace backend.Features.Habit;

public interface IHabitService
{
    Task<PagedResult<Models.Habit>> Index(HabitFilteringRequest filter, int userId);
    Task<Models.Habit?> GetById(int id, int userId);
    Task<Models.Habit> Create(CreateHabitRequest request, int userId);
    Task<Models.Habit?> Update(int id, UpdateHabitRequest request, int userId);
    Task<bool> SoftDelete(int id, int userId);
    Task<bool> Delete(int id, int userId);
}
