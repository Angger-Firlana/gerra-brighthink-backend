using System.ComponentModel.DataAnnotations.Schema;
using backend.Enum;

namespace backend.Models;

public class Goal
{
    public int Id {get; set;}
    public string TypeGoal {get; set;} = string.Empty;
    public int UserId {get; set;}
    public User? User {get; set;}
    public string Title {get; set;} = string.Empty;
    public GoalStatus Status {get; set;}
    public List<Task>? Tasks {get; set;}

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
