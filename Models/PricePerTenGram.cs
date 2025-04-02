using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Pricepertengram
{
    public int Id { get; set; }

    public int MaterialId { get; set; }

    public int Price { get; set; }

    public DateTime? Lastupdated { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual ICollection<Ordersummary> Ordersummaries { get; set; } = new List<Ordersummary>();
}
