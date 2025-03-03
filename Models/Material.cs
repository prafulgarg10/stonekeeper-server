using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PricePerTenGram> PricePerTenGrams { get; set; } = new List<PricePerTenGram>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
