using Microsoft.EntityFrameworkCore;
using Stonekeeper.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Stonekeeper;
using Stonekeeper.Service;

// var options = new WebApplicationOptions
// {
//     Args = args,
//     ContentRootPath = AppContext.BaseDirectory,
//     WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
// };

//var builder = WebApplication.CreateBuilder(options);

var builder = WebApplication.CreateBuilder(args);

//Directory.SetCurrentDirectory(AppContext.BaseDirectory.ToString());
var databaseCredentials = builder.Configuration.GetSection("Database");
var connectionString = $"server={databaseCredentials["Host"]};port={databaseCredentials["Port"]};database={databaseCredentials["DbName"]};Username={databaseCredentials["UserName"]};password={databaseCredentials["Password"]};";
var validAudience = builder.Configuration.GetSection("JWT")["ValidAudience"];
var secret = builder.Configuration.GetSection("JWT")["Secret"];

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
        builder.WithOrigins(new[] { "http://localhost:4200", "http://localhost:8080" }).AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = validAudience,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!=null ? secret : "Ne38sdEtvqskp1gRqC0dLTI/ZLpjxTrnZhJKfUADnQY="))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
