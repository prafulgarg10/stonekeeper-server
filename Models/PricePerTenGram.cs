using System;
using System.Collections.Generic;

namespace MyFirstServer.Models;

public partial class PricePerTenGram
{
    public int Id { get; set; }

    public int Price { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Material IdNavigation { get; set; } = null!;
}
