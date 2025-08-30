using System;

namespace Stonekeeper.Models;

public class OrderResponse
{
    public int productId {get;set;}
    public decimal weight {get;set;}
    public int quantity {get;set;}
    public decimal? price {get;set;}
}
