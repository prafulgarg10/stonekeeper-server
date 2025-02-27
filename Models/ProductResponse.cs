using System;

namespace MyFirstServer.Models;

public class ProductResponse
{
    public int category {get;set;}
    public int material {get;set;}
    public string name {get;set;}
    public int quantity {get;set;}
    public decimal weight {get;set;}
}
