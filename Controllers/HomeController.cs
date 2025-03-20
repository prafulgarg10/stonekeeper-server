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

    [Authorize]
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
        var products = _db.Products.OrderBy(p => p.Name).Select(p => new {
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
            ImageName = p.productImage!=null ? p.productImage.name : null
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
        
        string query = "SELECT mt.Name As materialName, mt.Id As materialId, pr.Price As price, pr.LastUpdated As lastUpdated FROM PricePerTenGrams pr INNER JOIN Material mt ON pr.Id = mt.Id WHERE (pr.Id, LastUpdated) IN (SELECT Id, MAX(LastUpdated) LastUpdated FROM PricePerTenGrams GROUP BY Id)";
        FormattableString qury = FormattableStringFactory.Create(query);
        var result = _db.Database.SqlQuery<PricePerTenGramResponse>(qury).ToList();
        return new ObjectResult(result);
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
        return Ok(new
                {
                    lastUpdated = lPrice.LastUpdated                    
                });
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
