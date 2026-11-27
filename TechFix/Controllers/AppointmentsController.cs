using System.Security.Claims;
using TechFix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechFix.Data;
using TechFix.Models;
using TechFix.ViewModels;

namespace TechFix.Controllers;
[Authorize(Roles="Cliente")]
public class AppointmentsController(TechFixDbContext db) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index() => View(await db.Appointments.Include(x=>x.Device).Include(x=>x.ServiceItem).Include(x=>x.Repair)
        .Where(x=>x.ClientId==UserId).OrderByDescending(x=>x.CreatedAt).ToListAsync());
    [HttpGet] public async Task<IActionResult> Create()
    {
        await Services();
        var hours=await db.BusinessHours.ToDictionaryAsync(x=>x.Day);
        var booked=await db.Appointments.Where(x=>x.ScheduledAt>DateTime.Now && x.Status!=AppointmentStatus.Cancelled)
            .Select(x=>x.ScheduledAt).ToListAsync();
        var occupied=booked.ToHashSet();
        var date=DateTime.Now.Date;
        for(var day=0; day<30; day++,date=date.AddDays(1))
        {
            if(!hours.TryGetValue(date.DayOfWeek,out var businessHour) || !businessHour.IsOpen) continue;
            for(var slot=date.Add(businessHour.OpensAt); slot<date.Add(businessHour.ClosesAt); slot=slot.AddHours(1))
                if(slot>DateTime.Now.AddHours(1) && !occupied.Contains(slot))
                    return View(new AppointmentVm { ScheduledAt=slot });
        }
        return View(new AppointmentVm());
    }
    private async Task Services()
    {
        ViewBag.Services = new SelectList(await db.Services.Where(x=>x.IsActive).OrderBy(x=>x.Name).ToListAsync(),"Id","Name");
        var client = await db.Users.SingleAsync(x=>x.Id==UserId);
        ViewBag.ClientName = client.FullName;
        ViewBag.ClientEmail = client.Email;
    }
    [HttpPost] public async Task<IActionResult> Create(AppointmentVm form)
    {
        if (form.ScheduledAt <= DateTime.Now) ModelState.AddModelError(nameof(form.ScheduledAt),"Selecciona una fecha futura.");
        var hour = await db.BusinessHours.SingleOrDefaultAsync(x=>x.Day==form.ScheduledAt.DayOfWeek);
        if (hour is null || !hour.IsOpen || form.ScheduledAt.TimeOfDay < hour.OpensAt || form.ScheduledAt.TimeOfDay >= hour.ClosesAt || form.ScheduledAt.Minute!=0)
            ModelState.AddModelError(nameof(form.ScheduledAt),"Selecciona una hora en punto dentro del horario de atención.");
        if (!await db.Services.AnyAsync(x=>x.Id==form.ServiceItemId && x.IsActive)) ModelState.AddModelError(nameof(form.ServiceItemId),"Servicio no disponible.");
        if (await db.Appointments.AnyAsync(x=>x.ScheduledAt==form.ScheduledAt && x.Status!=AppointmentStatus.Cancelled))
            ModelState.AddModelError(nameof(form.ScheduledAt),"Ese horario ya está reservado.");
        if (!ModelState.IsValid) { await Services(); return View(form); }
        var device = new Device { ClientId=UserId, Type=form.DeviceType.Trim(), Brand=form.Brand.Trim(), Model=form.Model.Trim() };
        var appointment = new Appointment { ClientId=UserId, Device=device, ServiceItemId=form.ServiceItemId, Phone=form.Phone.Trim(), Problem=form.Problem.Trim(), ScheduledAt=form.ScheduledAt,
            TrackingCode=TrackingCodeGenerator.Create() };
        var repair = new Repair { Appointment=appointment };
        db.Repairs.Add(repair);
        db.History.Add(new RepairStateHistory { Repair=repair, State=RepairState.AppointmentPending, ChangedById=UserId, Notes="Cita solicitada" });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { ModelState.AddModelError(nameof(form.ScheduledAt),"No se pudo reservar: el horario ya fue ocupado. Prueba otro."); await Services(); return View(form); }
        TempData["Message"]="Cita creada. Código de seguimiento: "+appointment.TrackingCode;
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await db.Appointments.Include(x=>x.Repair).SingleOrDefaultAsync(x=>x.Id==id && x.ClientId==UserId);
        if (appointment is null) return NotFound();
        if (appointment.Status!=AppointmentStatus.Pending) { TempData["Error"]="Solo puedes cancelar citas pendientes."; return RedirectToAction(nameof(Index)); }
        appointment.Status=AppointmentStatus.Cancelled;
        appointment.Repair!.State=RepairState.Cancelled;
        db.History.Add(new RepairStateHistory { RepairId=appointment.Repair.Id, State=RepairState.Cancelled, ChangedById=UserId, Notes="Cancelada por el cliente" });
        await db.SaveChangesAsync();
        TempData["Message"]="Cita cancelada.";
        return RedirectToAction(nameof(Index));
    }
}
