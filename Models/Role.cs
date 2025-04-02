using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Role
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Appuser> Appusers { get; set; } = new List<Appuser>();
}
