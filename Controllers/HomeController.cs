using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
