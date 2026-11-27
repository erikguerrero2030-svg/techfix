using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFix.Data;
using TechFix.Models;
using TechFix.ViewModels;

namespace TechFix.Controllers;
[Authorize(Roles="Tecnico")]
public class TechnicianController(TechFixDbContext db) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() => View(await db.Repairs.Include(x=>x.Appointment).ThenInclude(x=>x.Device)
        .Include(x=>x.Appointment.ServiceItem).Where(x=>x.TechnicianId==UserId).OrderByDescending(x=>x.Appointment.ScheduledAt).ToListAsync());
    public async Task<IActionResult> Details(int id)
    {
        var repair=await db.Repairs.Include(x=>x.Appointment).ThenInclude(x=>x.Device)
            .Include(x=>x.Appointment.ServiceItem).Include(x=>x.History).Include(x=>x.Diagnoses).Include(x=>x.Components)
            .SingleOrDefaultAsync(x=>x.Id==id && x.TechnicianId==UserId);
        return repair is null ? NotFound() : View(repair);
    }
    [HttpPost] public async Task<IActionResult> Update(RepairUpdateVm model)
    {
        var repair=await db.Repairs.SingleOrDefaultAsync(x=>x.Id==model.RepairId && x.TechnicianId==UserId);
        if (repair is null) return NotFound();
        var allowed=repair.State switch {
            RepairState.AppointmentConfirmed => model.State==RepairState.DeviceReceived,
            RepairState.DeviceReceived => model.State==RepairState.Diagnosing,
            RepairState.Diagnosing => model.State==RepairState.AwaitingClientApproval,
            RepairState.Repairing => model.State==RepairState.RepairCompleted,
            RepairState.RepairCompleted => model.State==RepairState.DeviceDelivered,
            _ => false
        };
        if (!allowed || (model.State==RepairState.RepairCompleted && !repair.ClientApproved)) return BadRequest("Cambio de estado no permitido.");
        if (model.State==RepairState.AwaitingClientApproval && string.IsNullOrWhiteSpace(model.Findings)) return BadRequest("Registra primero el diagnóstico.");
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (model.State==RepairState.AwaitingClientApproval)
            db.Diagnoses.Add(new Diagnosis { RepairId=repair.Id, TechnicianId=UserId, Findings=model.Findings!.Trim(), ProposedAction=(model.ProposedAction??"").Trim() });
        if (model.State==RepairState.RepairCompleted) repair.CompletedAt=DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(model.ComponentName))
            db.Components.Add(new ComponentUsage { RepairId=repair.Id, Name=model.ComponentName!.Trim(), Quantity=model.Quantity, UnitCost=model.UnitCost });
        repair.State=model.State;
        db.History.Add(new RepairStateHistory { RepairId=repair.Id, State=model.State, ChangedById=UserId, Notes=(model.Notes??"").Trim() });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details),new { id=repair.Id });
    }
}
