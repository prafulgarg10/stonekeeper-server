using System;
using System.Collections.Generic;

namespace Stonekeeper.Models;

public partial class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Pricepertengram> Pricepertengrams { get; set; } = new List<Pricepertengram>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
