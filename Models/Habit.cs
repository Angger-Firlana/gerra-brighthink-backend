using System.ComponentModel.DataAnnotations.Schema;
using backend.Enum;

namespace backend.Models;

public class Habit
{
    public int Id {get; set;}

    [Column("userId")]
    public int UserId {get; set;}
    public User? User {get; set;}

    public string Title {get; set;} = string.Empty;
    public string Period {get; set;} = string.Empty;
    public int TargetMinutes {get; set;}
    public int CurrentMinutes {get; set;}
    public HabitStatus Status {get; set;}
    public DateTime? CurrentStartedAt {get; set;}

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
