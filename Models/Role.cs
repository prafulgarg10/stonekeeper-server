using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
