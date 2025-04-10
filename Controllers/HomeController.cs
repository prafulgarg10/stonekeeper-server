using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyFirstServer.Data;
using MyFirstServer.Models;

namespace MyFirstServer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _logger = logger;
        _db = dbContext;
        _configuration = configuration;
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
                name = p.ImageName!=null ? p.ImageName : "",
                base64 = Convert.ToBase64String(p.ProductImage)
            } : null
        }).ToList();

        return new ObjectResult(products);
    }
 
    [Authorize(Roles = "Admin, Staff")]
    [HttpPost("add-product")]
    public async Task<IActionResult> AddProduct([FromBody] ProductResponse p){
        if(p==null){
            return BadRequest("Kindly provide some value to add");
        }
        if(p.quantity==0 || p.weight==0){
            return BadRequest("Kindly provide appropriate data.");
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

    [Authorize(Roles = "Admin")]
    [HttpPost("update-product")]
    public async Task<IActionResult> UpdateProduct([FromBody] ProductResponse p){
        if(p==null){
            return BadRequest("Kindly provide some value to update");
        }
        if(p.id<=0){
            return NotFound();
        }
        if(p.weight==0 || p.quantity==0){
            return BadRequest("Kindly provide appropriate data.");
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
  
    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
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
    public async Task<ObjectResult> GetLatestPrice(){
        return new ObjectResult(await LatestMaterialPrice());
    }

    private async Task<List<PricePerTenGramResponse>> LatestMaterialPrice(){
        string query = "SELECT mt.Name As materialName, mt.Id As materialId, pr.Price As price, pr.LastUpdated As lastUpdated, pr.Id As id FROM PricePerTenGrams pr INNER JOIN Material mt ON pr.material_id = mt.Id WHERE (pr.material_id, LastUpdated) IN (SELECT material_id , MAX(LastUpdated) LastUpdated FROM PricePerTenGrams GROUP BY material_id)";
        FormattableString qury = FormattableStringFactory.Create(query);
        var result = await _db.Database.SqlQuery<PricePerTenGramResponse>(qury).ToListAsync();
        return result; 
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPost("add-pricing")]
    public async Task<IActionResult> AddNewPricing([FromBody] PricePerTenGramResponse pricePerTenGram){
        if(pricePerTenGram==null){
            return BadRequest("Kindly provide some value to add");
        }
        Pricepertengram lPrice = new Pricepertengram(){
            Price = pricePerTenGram.price,
            MaterialId = pricePerTenGram.materialId,
            Lastupdated = DateTime.Now
        };
        _db.Pricepertengrams.Add(lPrice);
        await _db.SaveChangesAsync();
        return Ok(new
                {
                    lastUpdated = lPrice.Lastupdated                    
                });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderResponse[] order){
        if(order==null || (order!=null&&order.Length==0)){
            return BadRequest(new {message = "Kindly add some products.", orderId=0});
        }
        var loggedInUser = await GetUserInfoFromToken();
        var latestMaterialPrice = await LatestMaterialPrice();
        if(loggedInUser==null){
            return BadRequest(new {message = "Kindly login to place an order.", orderId=0});
        }
        decimal total = 0;
        foreach (var item in order)
        {
            item.price = CalculatePrice(item.productId, item.weight, latestMaterialPrice); 
            if(item.price==null || item.price==0){
                return BadRequest(new {message = "Please remove a product having amount zero and try again.", orderId=0});
            }
            total += item.price.Value;  
        };
        Order or = new Order(){
            Total = total,
            CreatedBy = loggedInUser.Id,
            CreatedAt = DateTime.Now
        };
        _db.Orders.Add(or);
        await _db.SaveChangesAsync();
        foreach (var item in order)
        {
            Product? product = _db.Products.Where(p => p.Id==item.productId).FirstOrDefault();
            if(product!=null){
                var currentPriceId = latestMaterialPrice.Where(p => p.materialId==product.MaterialId).Select(p => p.id).FirstOrDefault();
                Ordersummary os = new Ordersummary(){
                    Id = or.Id,
                    ProductId = item.productId,
                    ProductQuantity = item.quantity,
                    ProductWeight = item.weight,
                    ProductTotal = item.price!=null ? item.price.Value : 0,
                    ProductCategoryId = product.CategoryId,
                    MaterialPriceId = currentPriceId
                    };
                if(product.Weight>item.weight && product.Quantity>item.quantity){
                    product.Weight = product.Weight-item.weight;
                    product.Quantity = product.Quantity-item.quantity;
                    product.Lastupdated = DateTime.Now;
                    _db.Ordersummaries.Add(os);
                }
                else if(product.Weight==item.weight && product.Quantity==item.quantity){
                    product.Weight = 0;
                    product.Quantity = 0;
                    product.IsActive=false;
                    product.Lastupdated = DateTime.Now;
                    _db.Ordersummaries.Add(os);
                }
                else if(product.Weight>item.weight && product.Quantity==item.quantity){
                    product.Weight = product.Weight-item.weight;
                    product.Quantity = 1; //keeping it 1 by default as still the product is available
                    os.ProductQuantity = 1;
                    product.Lastupdated = DateTime.Now;
                    _db.Ordersummaries.Add(os);
                }
                else if(product.Weight==item.weight && product.Quantity>item.quantity){
                    product.Weight = product.Weight-item.weight;
                    product.Quantity = product.Quantity; //utilising full capacity as no weight is left
                    os.ProductQuantity = product.Quantity;
                    product.IsActive=false;
                    product.Lastupdated = DateTime.Now;
                    _db.Ordersummaries.Add(os);
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

    private decimal CalculatePrice(int Id, decimal weight, List<PricePerTenGramResponse> latestMaterialPrice){
       Product? product = _db.Products.Where(p => p.Id==Id).FirstOrDefault();
       decimal total = 0;
       if(product!=null){
        decimal? purity = _db.Categories.Where(c => c.Id==product.CategoryId).Select(c => c.Sellingpurity).FirstOrDefault();
        Nullable<int> materialPrice = null;
        if(latestMaterialPrice!=null){
            materialPrice = latestMaterialPrice.Where(p => p.materialId==product.MaterialId).Select(p => p.price).FirstOrDefault();
        }
        if(purity!=null && materialPrice!=null && weight>0 && weight<=product.Weight){
            total = purity.Value*materialPrice.Value*weight/1000;
        }
       }
       return total;
    }
    
    [Authorize(Roles = "Admin, Staff")]
    [HttpGet("get-orders")]
    public async Task<List<UserOrders>?> GetOrders(){
        var loggedInUser = await GetUserInfoFromToken();
        if(loggedInUser!=null){
            var orders = await _db.Orders.Where(o => o.CreatedBy==loggedInUser.Id).OrderByDescending(o => o.Id).ToListAsync();
            List<UserOrders> userOrders = new List<UserOrders>();
            foreach (var o in orders)
            {
                var createdBy = await _db.Appusers.Where(u => u.Id==o.CreatedBy).Select(u => u.Username).FirstOrDefaultAsync();
                UserOrders userOrder = new UserOrders();
                userOrder.orderId = o.Id;
                userOrder.totalAmount = o.Total;
                userOrder.createBy = createdBy!=null ? createdBy : "";
                userOrder.createdOn = o.CreatedAt;
                var orderSummary = await _db.Ordersummaries.Where(os => os.Id==o.Id).ToListAsync();
                var productsPerOrder = orderSummary.Select(os => new ProductPerOrder{
                    materialPrice = _db.Pricepertengrams.Where(p => p.Id==os.MaterialPriceId).Select(p => p.Price).FirstOrDefault(),
                    name = _db.Products.Where(p => p.Id==os.ProductId).Select(p => p.Name).FirstOrDefault(),
                    category = _db.Categories.Where(c => c.Id==os.ProductCategoryId).Select(c => c.Name).FirstOrDefault(),
                    productImage = _db.Products.Where(p => p.Id==os.ProductId).Select(p => p.ProductImage!=null ? new FileDTO{
                                    name = p.ImageName!=null ? p.ImageName : "",
                                    base64 = Convert.ToBase64String(p.ProductImage)
                                    } : null).FirstOrDefault(),
                    weight = os.ProductWeight,
                    quantity = os.ProductQuantity,
                    productTotal = os.ProductTotal,
                    materialId = _db.Pricepertengrams.Where(p => p.Id==os.MaterialPriceId).Select(p => p.MaterialId).FirstOrDefault(),
                    purity = _db.Categories.Where(c => c.Id==os.ProductCategoryId).Select(c => c.Sellingpurity).FirstOrDefault()
                }).ToList();

                userOrder.products = productsPerOrder;
                userOrders.Add(userOrder);
            }
            return userOrders;
        }
        return null;
    }

    private async Task<Appuser?> GetUserInfoFromToken(){
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
        if(string.IsNullOrEmpty(token)){
            return null;
        }
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]);
        var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,  
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        var jwtToken = validatedToken as JwtSecurityToken;
        var userName = jwtToken?.Claims.First(c => c.Type == ClaimTypes.Name)?.Value;
        if(userName!=null){
            var loggedInUser = await _db.Appusers.Where(u => u.Username==userName).FirstOrDefaultAsync();
            return loggedInUser;
        }
        return null;
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
