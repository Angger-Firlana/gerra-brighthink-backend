using System;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using System.Threading.Tasks;

namespace backend.Models;

public class User
{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string Username {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public bool isActive {get; set;}
    public int RoleId {get; set;}
    public Role? Role {get; set;}
    public DateTime? created_at {get; set;}
    public DateTime? updated_at {get; set;}
    public DateTime? deleted_at {get; set;}
}