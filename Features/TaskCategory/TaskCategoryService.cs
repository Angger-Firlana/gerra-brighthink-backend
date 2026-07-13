using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.TaskCategory;

public class TaskCategoryService : ITaskCategoryService
{
    private readonly AppDbContext dbContext;

    public TaskCategoryService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<Models.TaskCategory>> Index(int userId)
    {
        return await dbContext.TaskCategories
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Models.TaskCategory?> GetById(int id, int userId)
    {
        return await dbContext.TaskCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<Models.TaskCategory> Create(DTOs.TaskCategory.CreateTaskCategoryRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name cannot be null or empty.");

        var category = new Models.TaskCategory
        {
            UserId = userId,
            Name = request.Name
        };

        dbContext.TaskCategories.Add(category);
        await dbContext.SaveChangesAsync();

        return category;
    }

    public async Task<Models.TaskCategory?> Update(int id, DTOs.TaskCategory.UpdateTaskCategoryRequest request, int userId)
    {
        var category = await dbContext.TaskCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
            throw new ArgumentException($"TaskCategory with id {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name;

        await dbContext.SaveChangesAsync();
        return category;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var category = await dbContext.TaskCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (category is null)
            throw new ArgumentException($"TaskCategory with id {id} not found.");

        dbContext.TaskCategories.Remove(category);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
