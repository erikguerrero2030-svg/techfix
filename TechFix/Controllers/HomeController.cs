using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFix.Data;
using TechFix.Models;

namespace TechFix.Controllers;
public class HomeController(TechFixDbContext db, IConfiguration configuration) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.ContactEmail = configuration["TechFix:ContactEmail"];
        ViewBag.ContactPhones = configuration.GetSection("TechFix:ContactPhones").Get<string[]>() ?? Array.Empty<string>();
        ViewBag.ContactCity = configuration["TechFix:ContactCity"];
        ViewBag.Repaired = await db.Repairs.CountAsync(x => !x.Appointment.IsSynthetic && (x.State == RepairState.RepairCompleted || x.State == RepairState.DeviceDelivered));
        var services = await db.Services.Where(x => x.IsActive).ToListAsync();
        return View(services.OrderBy(x => x.Price).ToList());
    }
    public IActionResult Error() => View();
}
