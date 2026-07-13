using System.ComponentModel.DataAnnotations.Schema;
using backend.Enum;

namespace backend.Models;

public class Task
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public User? User {get; set;}
    public int? GoalId {get; set;}
    public Goal? Goal {get; set;}
    public int? TaskCategoryId {get; set;}
    public TaskCategory? TaskCategory {get; set;}
    public string Title {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public TodoStatus Status {get; set;} = TodoStatus.NotStarted;
    public string Priority {get; set;} = string.Empty;
    public string TypeHabbit {get; set;} = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
