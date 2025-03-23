using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFirstServer.Data;
using MyFirstServer.Models;

namespace MyFirstServer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _db;
    private List<PricePerTenGramResponse> latestMaterialPrice;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _db = dbContext;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        _logger.LogInformation("Testing logs");
        return View();
    }

    [HttpGet("categories")]
    public ObjectResult GetCategories(){
        var categories = _db.Categories.OrderBy(c => c.Name).ThenBy(c => c.Purity).ToList();
        return new ObjectResult(categories);
    }

    [HttpGet("materials")]
    public ObjectResult GetMaterials(){
        var materials = _db.Materials.OrderBy(m => m.Name).ToList();
        return new ObjectResult(materials);
    }

    [HttpGet("products")]
    public ObjectResult GetProducts(){
        var products = _db.Products.OrderBy(p => p.Name).Where(p => p.IsActive==true).Select(p => new {
            id = p.Id,
            name = p.Name,
            weight = p.Weight,
            quantity = p.Quantity,
            category = p.CategoryId,
            material = p.MaterialId,
            categoryName = p.Category.Name,
            materialName = p.Material.Name,
            productImage = p.ProductImage!=null ? new FileDTO{
                name = p.ImageName,
                base64 = Convert.ToBase64String(p.ProductImage)
            } : null
        }).ToList();

        return new ObjectResult(products);
    }

    //[Authorize(Roles = "Admin")]

    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct([FromBody] ProductResponse p){
        if(p==null){
            return BadRequest("Kindly provide some value to add");
        }
        Product product = new Product(){
            Name = p.name,
            Weight = p.weight,
            MaterialId = p.material,
            CategoryId = p.category,
            Quantity = p.quantity,
            ProductImage = p.productImage!=null ? Convert.FromBase64String(p.productImage.base64) : null,
            ImageName = p.productImage!=null ? p.productImage.name : null,
            IsActive = true
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return Ok(new
                {
                    id = product.Id
                });
    }

    [HttpPost("update-product")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductResponse p){
        if(p==null){
            return BadRequest("Kindly provide some value to update");
        }
        if(p.id<=0){
            return NotFound();
        }
        var product = _db.Products.Where(pd => pd.Id==p.id).FirstOrDefault();
        if(product!=null){
            product.Name = p.name;
            product.Weight = p.weight;
            product.MaterialId = p.material;
            product.CategoryId = p.category;
            product.Quantity = p.quantity;
            await _db.SaveChangesAsync();
            return Ok(new
                {
                    id = product.Id
                });
        }
        return NotFound();
    }

    [HttpPost("delete-product")]
    public async Task<IActionResult> DeleteProduct([FromBody] ProductResponse p){
        if(p==null){
            return BadRequest("Kindly provide some value to delete");
        }
        if(p.id<=0){
            return NotFound();
        }
        var product = _db.Products.Where(pd => pd.Id==p.id).FirstOrDefault();
        if(product!=null){
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new
                {
                    id = product.Id
                });
        }
        return NotFound();
    }

    //[Authorize(Roles = "Admin")]

    [HttpPost("add-category")]
    public async Task<IActionResult> AddCategory([FromBody] Category category){
        if(category==null){
            return BadRequest("Kindly provide some value to add");
        }
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(new
                {
                    id = category.Id
                });
    }

    [HttpGet("latest-price")]
    public ObjectResult GetLatestPrice(){
        if(latestMaterialPrice!=null){
            return new ObjectResult(latestMaterialPrice);
        }
        latestMaterialPrice = LatestMaterialPrice();
        return new ObjectResult(latestMaterialPrice);
    }

    private List<PricePerTenGramResponse> LatestMaterialPrice(){
        string query = "SELECT mt.Name As materialName, mt.Id As materialId, pr.Price As price, pr.LastUpdated As lastUpdated FROM PricePerTenGrams pr INNER JOIN Material mt ON pr.Id = mt.Id WHERE (pr.Id, LastUpdated) IN (SELECT Id, MAX(LastUpdated) LastUpdated FROM PricePerTenGrams GROUP BY Id)";
        FormattableString qury = FormattableStringFactory.Create(query);
        var result = _db.Database.SqlQuery<PricePerTenGramResponse>(qury).ToList();
        return result; 
    }

    [HttpPost("add-pricing")]
    public async Task<IActionResult> AddNewPricing([FromBody] PricePerTenGramResponse pricePerTenGram){
        if(pricePerTenGram==null){
            return BadRequest("Kindly provide some value to add");
        }
        PricePerTenGram lPrice = new PricePerTenGram(){
            Price = pricePerTenGram.price,
            Id = pricePerTenGram.materialId,
            LastUpdated = DateTime.Now
        };
        _db.PricePerTenGrams.Add(lPrice);
        await _db.SaveChangesAsync();
        latestMaterialPrice = LatestMaterialPrice();
        return Ok(new
                {
                    lastUpdated = lPrice.LastUpdated                    
                });
    }

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderResponse[] order){
        if(order==null || (order!=null&&order.Length==0)){
            return BadRequest(new {message = "Kindly add some products.", orderId=0});
        }
        decimal total = 0;
        foreach (var item in order)
        {
            item.price = CalculatePrice(item.productId, item.weight); 
            if(item.price==null || item.price==0){
                return BadRequest(new {message = "Please remove a product having amount zero and try again.", orderId=0});
            }
            total += item.price.Value;  
        };
        Order or = new Order(){
            Total = total,
            CreatedAt = DateTime.Now
        };
        _db.Orders.Add(or);
        await _db.SaveChangesAsync();
        foreach (var item in order)
        {
            Product? product = _db.Products.Where(p => p.Id==item.productId).FirstOrDefault();
            if(product!=null){
                if(product.Weight>item.weight && product.Quantity>item.quantity){
                    OrderSummary os = new OrderSummary(){
                    Id = or.Id,
                    ProductId = item.productId,
                    ProductQuantity = item.quantity,
                    ProductWeight = item.weight,
                    ProductTotal = item.price.Value
                    };
                    product.Weight = product.Weight-item.weight;
                    product.Quantity = product.Quantity-item.quantity;
                    product.LastUpdated = DateTime.Now;
                    _db.OrderSummaries.Add(os);
                }
                else if(product.Weight==item.weight && product.Quantity==item.quantity){
                    OrderSummary os = new OrderSummary(){
                    Id = or.Id,
                    ProductId = item.productId,
                    ProductQuantity = item.quantity,
                    ProductWeight = item.weight,
                    ProductTotal = item.price.Value
                    };
                    product.Weight = 0;
                    product.Quantity = 0;
                    product.IsActive=false;
                    product.LastUpdated = DateTime.Now;
                    _db.OrderSummaries.Add(os);
                }
            }
        };
        await _db.SaveChangesAsync();
        return Ok(new
                {
                    messgae = "Order created successfully.",
                    orderId = or.Id
                });
    }

    private decimal CalculatePrice(int Id, decimal weight){
       Product? product = _db.Products.Where(p => p.Id==Id).FirstOrDefault();
       decimal total = 0;
       if(product!=null){
        decimal? purity = _db.Categories.Where(c => c.Id==product.CategoryId).Select(c => c.Purity).FirstOrDefault();
        Nullable<int> materialPrice = null;
        if(latestMaterialPrice==null){
            latestMaterialPrice = LatestMaterialPrice();
            
        }
        materialPrice = latestMaterialPrice.Where(p => p.materialId==product.MaterialId).Select(p => p.price).FirstOrDefault();
        if(purity!=null && materialPrice!=null){
            total = purity.Value*materialPrice.Value*weight/1000;
        }
       }
       return total;
    }

    [HttpGet("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
