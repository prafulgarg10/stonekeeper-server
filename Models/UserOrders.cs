using System;

namespace MyFirstServer.Models;

public class UserOrders
{
    public int orderId {get;set;}
    public decimal totalAmount {get;set;}
    public string? createBy {get;set;}
    public List<ProductPerOrder>? products {get; set;}
    public DateTime? createdOn {get;set;}
}

public class ProductPerOrder{
    public FileDTO? productImage { get; set; }
    public string? name { get; set; }
    public decimal weight { get; set; }
    public string? category {get; set;}
    public decimal? purity { get; set; }
    public int quantity { get; set; }
    public int materialPrice {get;set;}
    public decimal productTotal {get;set;}
}
