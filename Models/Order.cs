using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class Order
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int ProductQuantity { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
