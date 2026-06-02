using CineScope.Data;
using CineScope.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Microsoft Identity Web authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Add authorization policies for role-based access
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("Member", policy =>
        policy.RequireRole("Member", "Admin"));
    
    options.AddPolicy("Guest", policy =>
        policy.RequireAuthenticatedUser()); 
});

// Add services to the container
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<MovieContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MovieContext")));

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddHttpClient<MovieService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
