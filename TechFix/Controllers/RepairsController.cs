using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFix.Data;
using TechFix.Models;

namespace TechFix.Controllers;
[Authorize(Roles="Cliente")]
public class RepairsController(TechFixDbContext db) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Details(int id)
    {
        var repair=await db.Repairs.Include(x=>x.Appointment).ThenInclude(x=>x.Device)
            .Include(x=>x.Appointment.ServiceItem).Include(x=>x.History).ThenInclude(x=>x.ChangedBy)
            .Include(x=>x.Diagnoses).Include(x=>x.Review)
            .SingleOrDefaultAsync(x=>x.Id==id && x.Appointment.ClientId==UserId);
        return repair is null ? NotFound() : View(repair);
    }
    [HttpPost] public async Task<IActionResult> Approve(int id)
    {
        var repair=await db.Repairs.Include(x=>x.Appointment).SingleOrDefaultAsync(x=>x.Id==id && x.Appointment.ClientId==UserId);
        if (repair is null) return NotFound();
        if (repair.State!=RepairState.AwaitingClientApproval) return BadRequest();
        repair.ClientApproved=true;
        repair.State=RepairState.Repairing;
        db.History.Add(new RepairStateHistory { RepairId=id, State=RepairState.Repairing, ChangedById=UserId, Notes="Cliente autorizó la reparación" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details),new { id });
    }
    [HttpPost] public async Task<IActionResult> Review(int id, int rating, string? comment)
    {
        var repair=await db.Repairs.Include(x=>x.Appointment).Include(x=>x.Review).SingleOrDefaultAsync(x=>x.Id==id && x.Appointment.ClientId==UserId);
        if (repair is null) return NotFound();
        if (repair.State!=RepairState.DeviceDelivered || repair.Review is not null || rating is <1 or >5 || (comment?.Length??0)>1000) return BadRequest();
        db.Reviews.Add(new Review { RepairId=id, ClientId=UserId, Rating=rating, Comment=comment?.Trim()??"" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details),new { id });
    }
}
