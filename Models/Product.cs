using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Weight { get; set; }

    public int Quantity { get; set; }

    public int CategoryId { get; set; }

    public int MaterialId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastUpdated { get; set; }

    public byte[]? ProductImage { get; set; }

    public string? ImageName { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Material Material { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
