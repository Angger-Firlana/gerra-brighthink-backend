using System.ComponentModel.DataAnnotations.Schema;
using backend.Enum;

namespace backend.Models;

public class SubTask
{
    public int Id {get; set;}
    public int TaskId {get; set;}
    public Task? Task {get; set;}
    public string Title {get; set;} = string.Empty;
    public SubTaskStatus Status {get; set;} = SubTaskStatus.NotStarted;

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
