using System;

namespace Stonekeeper.Models;

public class PricePerTenGramResponse
{
    public int id {get;set;}
    public string? materialName {get; set;}
    public int materialId {get; set;}
    public int price {get; set;}
    public DateTime? lastUpdated {get; set;}
}
