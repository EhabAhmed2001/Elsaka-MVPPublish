using ElsakaHerbs.DataAccess;
using ElsakaHerbs.DataAccess.Repositories;
using ElsakaHerbs.interfaces;
using ElsakaHerbs.Services;
using ElsakaHerbsMVC.Middlewares;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Configure UTF-8 Arabic HTML Encoding
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register AppDbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Generic Repository & Unit of Work
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Application Services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ISiteContentService, SiteContentService>();
builder.Services.AddScoped<IGovernorateShippingService, GovernorateShippingService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Maintenance Mode middleware: protects public routes & checkout when website is disabled
app.UseMiddleware<MaintenanceModeMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Run database initializer: apply migrations, seed SiteContent and Governorates
await ElsakaHerbsMVC.DatabaseInitializer.InitializeAsync(app.Services, 
                                                         app.Services.GetRequiredService<IWebHostEnvironment>());

app.Run();
