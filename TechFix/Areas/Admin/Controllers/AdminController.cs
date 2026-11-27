using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechFix.Data;
using TechFix.Models;
using TechFix.ViewModels;

namespace TechFix.Areas.Admin.Controllers;
[Area("Admin")]
[Authorize(Roles="Administrador")]
public class AdminController(TechFixDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    public async Task<IActionResult> Index()
    {
        ViewBag.Clients=await db.Clients.CountAsync(x=>x.User.IsActive);
        ViewBag.Pending=await db.Appointments.CountAsync(x=>x.Status==AppointmentStatus.Pending);
        ViewBag.InProcess=await db.Repairs.CountAsync(x=>x.State>=RepairState.DeviceReceived && x.State<=RepairState.Repairing);
        ViewBag.Finished=await db.Repairs.CountAsync(x=>x.State==RepairState.RepairCompleted || x.State==RepairState.DeviceDelivered);
        ViewBag.Revenue=(await db.Payments.Where(x=>!x.Repair.Appointment.IsSynthetic).Select(x=>x.Amount).ToListAsync()).Sum();
        ViewBag.Technicians=await db.Technicians.CountAsync(x=>x.User.IsActive);
        return View(await db.Appointments.Include(x=>x.Client).Include(x=>x.ServiceItem).Include(x=>x.Repair)
            .OrderByDescending(x=>x.CreatedAt).Take(12).ToListAsync());
    }
    public async Task<IActionResult> Appointments(string? search, AppointmentStatus? status)
    {
        var query=db.Appointments.Include(x=>x.Client).Include(x=>x.Device).Include(x=>x.ServiceItem)
            .Include(x=>x.Repair).ThenInclude(x=>x!.Technician).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query=query.Where(x=>x.TrackingCode.Contains(search) || x.Client.FullName.Contains(search) || x.Client.Email!.Contains(search));
        if (status is not null) query=query.Where(x=>x.Status==status);
        ViewBag.Search=search;
        ViewBag.Status=status;
        ViewBag.Technicians=await db.Technicians.Include(x=>x.User).Where(x=>x.User.IsActive).ToListAsync();
        return View(await query.OrderByDescending(x=>x.CreatedAt).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Confirm(int id)
    {
        var appointment=await db.Appointments.Include(x=>x.Repair).SingleOrDefaultAsync(x=>x.Id==id);
        if (appointment is null) return NotFound();
        if (appointment.Status!=AppointmentStatus.Pending) return BadRequest();
        appointment.Status=AppointmentStatus.Confirmed;
        appointment.Repair!.State=RepairState.AppointmentConfirmed;
        db.History.Add(new RepairStateHistory { RepairId=appointment.Repair.Id, State=RepairState.AppointmentConfirmed, ChangedById=UserId, Notes="Cita confirmada" });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Appointments));
    }
    [HttpPost] public async Task<IActionResult> Assign(int id,string technicianId)
    {
        var repair=await db.Repairs.Include(x=>x.Appointment).SingleOrDefaultAsync(x=>x.Id==id);
        if (repair is null) return NotFound();
        if (repair.Appointment.Status!=AppointmentStatus.Confirmed) return BadRequest();
        var technician=await db.Technicians.Include(x=>x.User).SingleOrDefaultAsync(x=>x.UserId==technicianId && x.User.IsActive);
        if (technician is null || !await users.IsInRoleAsync(technician.User,"Tecnico")) return BadRequest();
        repair.TechnicianId=technicianId;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Appointments));
    }
    public async Task<IActionResult> Services() => View(await db.Services.OrderBy(x=>x.Id).ToListAsync());
    [HttpGet] public async Task<IActionResult> Service(int? id)
    {
        var entity=id is null?null:await db.Services.FindAsync(id.Value);
        if (id is not null && entity is null) return NotFound();
        return View(entity is null?new ServiceVm():new ServiceVm { Id=entity.Id, Name=entity.Name, Description=entity.Description, Price=entity.Price, IsActive=entity.IsActive });
    }
    [HttpPost] public async Task<IActionResult> Service(ServiceVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity=model.Id==0?new ServiceItem():await db.Services.FindAsync(model.Id);
        if (entity is null) return NotFound();
        entity.Name=model.Name.Trim(); entity.Description=model.Description.Trim(); entity.Price=model.Price; entity.IsActive=model.IsActive;
        if (model.Id==0) db.Services.Add(entity);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Services));
    }
    public async Task<IActionResult> Technicians() => View(await db.Technicians.Include(x=>x.User).OrderBy(x=>x.User.FullName).ToListAsync());
    [HttpGet] public IActionResult NewTechnician() => View(new TechnicianVm());
    [HttpPost] public async Task<IActionResult> NewTechnician(TechnicianVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var user=new ApplicationUser { UserName=model.Email.Trim(), Email=model.Email.Trim(), FullName=model.FullName.Trim(), EmailConfirmed=true };
        await using var tx=await db.Database.BeginTransactionAsync();
        var result=await users.CreateAsync(user,model.Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("",e.Description); return View(model); }
        result=await users.AddToRoleAsync(user,"Tecnico");
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("",e.Description); return View(model); }
        db.Technicians.Add(new TechnicianProfile { UserId=user.Id, Specialty=model.Specialty.Trim() });
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return RedirectToAction(nameof(Technicians));
    }
    [HttpPost] public async Task<IActionResult> ToggleTechnician(string id)
    {
        var t=await db.Technicians.Include(x=>x.User).SingleOrDefaultAsync(x=>x.UserId==id);
        if (t is null) return NotFound();
        t.User.IsActive=!t.User.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Technicians));
    }
    public async Task<IActionResult> Clients() => View(await db.Clients.Include(x=>x.User).OrderBy(x=>x.User.FullName).ToListAsync());
    [HttpPost] public async Task<IActionResult> ToggleClient(string id)
    {
        var c=await db.Clients.Include(x=>x.User).SingleOrDefaultAsync(x=>x.UserId==id);
        if (c is null) return NotFound();
        c.User.IsActive=!c.User.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Clients));
    }
    public async Task<IActionResult> Hours() => View(await db.BusinessHours.OrderBy(x=>x.Day).ToListAsync());
    [HttpPost] public async Task<IActionResult> Hours(int id,bool isOpen,TimeSpan opensAt,TimeSpan closesAt)
    {
        var hour=await db.BusinessHours.FindAsync(id);
        if (hour is null) return NotFound();
        if (isOpen && opensAt>=closesAt) return BadRequest("La hora de cierre debe ser posterior a la apertura.");
        hour.IsOpen=isOpen; hour.OpensAt=opensAt; hour.ClosesAt=closesAt;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Hours));
    }
    public async Task<IActionResult> Payments(int? repairId)
    {
        ViewBag.SelectedRepairId=repairId;
        ViewBag.PayableRepairs=await db.Repairs.Include(x=>x.Appointment).ThenInclude(x=>x.Client)
            .Where(x=>x.State==RepairState.RepairCompleted || x.State==RepairState.DeviceDelivered)
            .OrderByDescending(x=>x.CompletedAt).ToListAsync();
        return View(await db.Payments.Include(x=>x.Repair).ThenInclude(x=>x.Appointment)
            .OrderByDescending(x=>x.RegisteredAt).ToListAsync());
    }
    [HttpPost] public async Task<IActionResult> Payment(int repairId,decimal amount,string? method)
    {
        if (amount<=0 || amount>100000 || string.IsNullOrWhiteSpace(method) || method.Length>40)
        {
            TempData["Error"]="Ingresa un monto mayor que cero y selecciona un método de pago.";
            return RedirectToAction(nameof(Payments));
        }
        var repair=await db.Repairs.FindAsync(repairId);
        if (repair is null || (repair.State!=RepairState.RepairCompleted && repair.State!=RepairState.DeviceDelivered))
        {
            TempData["Error"]="Selecciona una reparación terminada o entregada de la lista.";
            return RedirectToAction(nameof(Payments));
        }
        db.Payments.Add(new Payment { RepairId=repairId, Amount=amount, Method=method.Trim(), RegisteredById=UserId });
        await db.SaveChangesAsync();
        TempData["Message"]="Pago registrado correctamente.";
        return RedirectToAction(nameof(Payments));
    }
    public async Task<IActionResult> Reports()
    {
        ViewBag.Reviews=await db.Reviews.CountAsync(x=>!x.Repair.Appointment.IsSynthetic);
        ViewBag.AverageRating=await db.Reviews.Where(x=>!x.Repair.Appointment.IsSynthetic).AverageAsync(x=>(double?)x.Rating)??0;
        var payments=await db.Payments.Where(x=>!x.Repair.Appointment.IsSynthetic).Select(x=>new { x.RegisteredAt,x.Amount }).ToListAsync();
        var monthly=payments.GroupBy(x=>new { x.RegisteredAt.Year,x.RegisteredAt.Month })
            .Select(x=>new ReportMonthVm(x.Key.Year,x.Key.Month,x.Sum(p=>p.Amount))).OrderBy(x=>x.Year).ThenBy(x=>x.Month).ToList();
        ViewBag.MaxMonthly=monthly.Count==0?1m:monthly.Max(x=>x.Total);
        ViewBag.Completed=await db.Repairs.CountAsync(x=>!x.Appointment.IsSynthetic && (x.State==RepairState.RepairCompleted || x.State==RepairState.DeviceDelivered));
        ViewBag.Recovered=await db.Repairs.CountAsync(x=>!x.Appointment.IsSynthetic && x.State==RepairState.DeviceDelivered);
        return View(monthly);
    }
}
