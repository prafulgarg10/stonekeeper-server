using System;

namespace MyFirstServer.Models;

public class UserOrders
{
    public int orderId {get;set;}
    public decimal totalAmount {get;set;}
    public string createBy {get;set;}
    public List<ProductPerOrder> products {get; set;}
    public DateTime? createdOn {get;set;}
}

public class ProductPerOrder{
    public byte[]? ProductImage { get; set; }
    public string? Name { get; set; }
    public decimal Weight { get; set; }
    public string Category {get; set;}
    public decimal? Purity { get; set; }
    public int Quantity { get; set; }
    public int materialPrice {get;set;}
    public decimal productTotal {get;set;}
}
