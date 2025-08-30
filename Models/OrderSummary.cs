using System;
using System.Collections.Generic;

namespace Stonekeeper.Models;

public partial class Ordersummary
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int ProductQuantity { get; set; }

    public decimal ProductWeight { get; set; }

    public decimal ProductTotal { get; set; }

    public int ProductCategoryId { get; set; }

    public int MaterialPriceId { get; set; }

    public virtual Order IdNavigation { get; set; } = null!;

    public virtual Pricepertengram MaterialPrice { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
