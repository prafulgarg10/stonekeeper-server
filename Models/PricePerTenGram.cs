using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class PricePerTenGram
{
    public int Id { get; set; }

    public int MaterialId { get; set; }

    public int Price { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual ICollection<OrderSummary> OrderSummaries { get; set; } = new List<OrderSummary>();
}
