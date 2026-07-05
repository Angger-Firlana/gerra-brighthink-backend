using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Goal> Goals { get; set; }
    public DbSet<TaskCategory> TaskCategories { get; set; }
    public DbSet<Models.Task> Tasks { get; set; }
    public DbSet<SubTask> SubTasks { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Habit> Habits { get; set; }
}