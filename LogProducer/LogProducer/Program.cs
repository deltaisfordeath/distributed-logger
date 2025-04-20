using LogProducer.Data;
using LogProducer.Helpers;
using LogProducer.Services;
using LogProducer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<LogProducerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<LogProducerDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHostedService<SyncService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Set the correct login path
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Optional: Set access denied path
    options.ReturnUrlParameter = "ReturnUrl"; // Ensure return URL is handled correctly
});

builder.Services.AddRazorPages();

builder.Services.AddHttpClient<DistributedLogService>();
builder.Services.AddScoped<IDistributedLogService, DistributedLogService>();

var app = builder.Build();

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

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var config = services.GetRequiredService<IConfiguration>();
    await IdentitySeeder.SeedRolesAsync(services, config);
}

app.Run();