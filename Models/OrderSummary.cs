using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class OrderSummary
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int ProductQuantity { get; set; }

    public decimal ProductWeight { get; set; }

    public decimal ProductTotal { get; set; }

    public virtual Order IdNavigation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
