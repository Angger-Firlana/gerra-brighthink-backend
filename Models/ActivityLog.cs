using System.ComponentModel.DataAnnotations.Schema;
using backend.Enum;

namespace backend.Models;

public class ActivityLog
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public User? Actor {get; set;}
    public int EntityId {get; set;}
    public EntityType EntityType {get; set;}
    public string Action {get; set;} = string.Empty;

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
