using System;

namespace Stonekeeper.Models;

public class ProductResponse
{
    public int? id {get;set;}
    public int category {get;set;}
    public int material {get;set;}
    public string name {get;set;}
    public int quantity {get;set;}
    public decimal weight {get;set;}
    public FileDTO productImage {get;set;}
}

public class FileDTO{
    public string name {get; set;}
    public string fileType {get; set;}
    public int fileSize {get; set;}
    public string base64 {get; set;}
}