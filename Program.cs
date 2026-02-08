using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentWisePro.Web.Data;
using RentWisePro.Web.Models.Identity;
using RentWisePro.Web.Services;
using RentWisePro.Web.Services.Etl;
using RentWisePro.Web.Services.Identity;
using RentWisePro.Web.Services.MarketData;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("RentWiseProDb");
builder.Services.AddDbContext<RentWiseProDbContext>(options =>
    options.UseSqlServer(cs));
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(cs));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddScoped<ForecastCalculationService>();
builder.Services.AddScoped<ClosingDisclosureCalculationService>();
builder.Services.AddScoped<InvestmentProfileResolver>();
builder.Services.AddScoped<IEtlControlService, LocalEtlControlService>();
builder.Services.AddScoped<EtlOpsMetricsService>();
builder.Services.AddScoped<CompositeScoreCalculator>();
builder.Services.AddScoped<IGeoMarketDataLookup, NullGeoMarketDataLookup>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PurchaseSheetCalculationService>();

var app = builder.Build();

await RentWisePro.Web.Services.AdminBootstrapper.RunAsync(
    app.Services,
    app.Configuration,
    app.Environment,
    app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
