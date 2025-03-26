using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Order
{
    public int Id { get; set; }

    public decimal Total { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual AppUser CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<OrderSummary> OrderSummaries { get; set; } = new List<OrderSummary>();
}
