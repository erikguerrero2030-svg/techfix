using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechFix.Models;

namespace TechFix.Data;
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechFixDbContext>();
        if (scope.ServiceProvider.GetRequiredService<IHostEnvironment>().IsEnvironment("Testing") || db.Database.IsNpgsql()) await db.Database.EnsureCreatedAsync();
        else await db.Database.MigrateAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Cliente", "Tecnico", "Administrador" })
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(new[] {
                new ServiceItem { Name="Diagnóstico básico", Description="Revisión inicial y evaluación del equipo.", Price=3, Icon="scan" },
                new ServiceItem { Name="Limpieza preventiva", Description="Limpieza interna y externa para cuidar el equipo.", Price=5, Icon="spark" },
                new ServiceItem { Name="Optimización del sistema", Description="Ajustes para mejorar el rendimiento.", Price=5, Icon="speed" },
                new ServiceItem { Name="Respaldo de archivos", Description="Copia de seguridad de tus datos importantes.", Price=5, Icon="backup" },
                new ServiceItem { Name="Instalación de software", Description="Instalación y configuración de programas.", Price=8, Icon="install" },
                new ServiceItem { Name="Mantenimiento completo", Description="Diagnóstico, limpieza y optimización integral.", Price=12, Icon="tools" }
            });
        }
        if (!await db.BusinessHours.AnyAsync())
            for (var day = DayOfWeek.Sunday; day <= DayOfWeek.Saturday; day++)
                db.BusinessHours.Add(new BusinessHour { Day=day, OpensAt=new TimeSpan(9,0,0), ClosesAt=new TimeSpan(18,0,0), IsOpen=day!=DayOfWeek.Sunday });
        await db.SaveChangesAsync();
        var adminEmail = Environment.GetEnvironmentVariable("TECHFIX_ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("TECHFIX_ADMIN_PASSWORD");
        if (db.Database.IsNpgsql() && !string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.GetUsersInRoleAsync("Administrador") is { Count: 0 })
            {
                var admin = new ApplicationUser { UserName=adminEmail.Trim(), Email=adminEmail.Trim(), FullName="Administrador TechFix", EmailConfirmed=true };
                var created = await users.CreateAsync(admin, adminPassword);
                if (!created.Succeeded) throw new InvalidOperationException("No se pudo crear el administrador: " + string.Join("; ",created.Errors.Select(x=>x.Description)));
                var assigned = await users.AddToRoleAsync(admin,"Administrador");
                if (!assigned.Succeeded) throw new InvalidOperationException("No se pudo asignar el rol de administrador.");
            }
        }
    }
    public static async Task CreateAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await users.GetUsersInRoleAsync("Administrador") is { Count: > 0 })
            throw new InvalidOperationException("Ya existe un administrador. No se crearon credenciales adicionales.");
        Console.Write("Correo del primer administrador: ");
        var email = Console.ReadLine()?.Trim();
        Console.Write("Contraseña inicial (oculta): ");
        var password = ReadPassword();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Debes ingresar correo y contraseña.");
        var user = new ApplicationUser { UserName=email, Email=email, FullName="Administrador TechFix", EmailConfirmed=true };
        var result = await users.CreateAsync(user,password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));
        result = await users.AddToRoleAsync(user,"Administrador");
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ",result.Errors.Select(x=>x.Description)));
        Console.WriteLine("Administrador creado correctamente.");
    }
    private static string ReadPassword()
    {
        var chars = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept:true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && chars.Length>0) chars.Length--;
            else if (!char.IsControl(key.KeyChar)) chars.Append(key.KeyChar);
        }
        Console.WriteLine();
        return chars.ToString();
    }
}
