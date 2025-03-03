using System;

namespace MyFirstServer.Models;

public class PricePerTenGramResponse
{
    public string? materialName {get; set;}
    public int materialId {get; set;}
    public int price {get; set;}
    public DateTime? lastUpdated {get; set;}
}
