using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TechFix.Data;
using TechFix.Models;

var builder = WebApplication.CreateBuilder(args);
var databaseUrl = builder.Configuration["DATABASE_URL"];
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // El modelo original usa DateTime para horas locales de citas y marcas UTC.
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    var url = new Uri(databaseUrl);
    var credentials = url.UserInfo.Split(':', 2);
    if (credentials.Length != 2) throw new InvalidOperationException("DATABASE_URL no contiene credenciales válidas.");
    var pg = new NpgsqlConnectionStringBuilder {
        Host = url.Host, Port = url.Port, Database = Uri.UnescapeDataString(url.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(credentials[0]), Password = Uri.UnescapeDataString(credentials[1]),
        SslMode = SslMode.Prefer
    };
    builder.Services.AddDbContext<TechFixDbContext>(options => options.UseNpgsql(pg.ConnectionString));
}
else
{
    var connection = builder.Configuration.GetConnectionString("TechFixDB")
        ?? throw new InvalidOperationException("Configura ConnectionStrings:TechFixDB.");
    builder.Services.AddDbContext<TechFixDbContext>(options => options.UseSqlServer(connection));
}
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<TechFixDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = true;
    options.Events.OnValidatePrincipal = async context => {
        var manager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var account = await manager.GetUserAsync(context.Principal!);
        if (account is null || !account.IsActive) { context.RejectPrincipal(); await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); }
    };
});
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
// Render termina TLS antes de entregar el tráfico HTTP al contenedor.
if (string.IsNullOrWhiteSpace(databaseUrl)) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
await SeedData.InitializeAsync(app.Services);
if (args.Contains("--create-admin")) { await SeedData.CreateAdminAsync(app.Services); return; }
if (args.Contains("--seed-sample-data")) { await SampleDataSeeder.RunAsync(app.Services); return; }
app.MapAreaControllerRoute(name:"admin",areaName:"Admin",pattern:"Admin/{action=Index}/{id?}",defaults:new { controller="Admin" });
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
