using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class User
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Username {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;

    [Column("isActive")]
    public bool IsActive {get; set;}

    public int RoleId {get; set;}
    public Role? Role {get; set;}

    [Column("created_at")]
    public DateTime? CreatedAt {get; set;}

    [Column("updated_at")]
    public DateTime? UpdatedAt {get; set;}

    [Column("deleted_at")]
    public DateTime? DeletedAt {get; set;}
}
