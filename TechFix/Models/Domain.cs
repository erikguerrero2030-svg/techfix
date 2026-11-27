using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TechFix.Models;
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public ClientProfile? ClientProfile { get; set; }
    public TechnicianProfile? TechnicianProfile { get; set; }
}
public class ClientProfile
{
    [Key] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class TechnicianProfile
{
    [Key] public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    [StringLength(120)] public string Specialty { get; set; } = "";
}
public class ServiceItem
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [StringLength(500)] public string Description { get; set; } = "";
    [Range(0, 100000)] public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    [StringLength(60)] public string Icon { get; set; } = "tools";
}
public class Device
{
    public int Id { get; set; }
    [Required] public string ClientId { get; set; } = "";
    public ApplicationUser Client { get; set; } = null!;
    [Required, StringLength(60)] public string Type { get; set; } = "";
    [Required, StringLength(60)] public string Brand { get; set; } = "";
    [Required, StringLength(80)] public string Model { get; set; } = "";
}
public enum AppointmentStatus { Pending, Confirmed, Cancelled }
public class Appointment
{
    public int Id { get; set; }
    [Required] public string ClientId { get; set; } = "";
    public ApplicationUser Client { get; set; } = null!;
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public int ServiceItemId { get; set; }
    public ServiceItem ServiceItem { get; set; } = null!;
    [Required, StringLength(20)] public string Phone { get; set; } = "";
    [Required, StringLength(1000)] public string Problem { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public bool IsSynthetic { get; set; }
    [Required, StringLength(20)] public string TrackingCode { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Repair? Repair { get; set; }
}
public enum RepairState { AppointmentPending, AppointmentConfirmed, DeviceReceived, Diagnosing, AwaitingClientApproval, Repairing, RepairCompleted, DeviceDelivered, Cancelled }
public class Repair
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string? TechnicianId { get; set; }
    public ApplicationUser? Technician { get; set; }
    public RepairState State { get; set; } = RepairState.AppointmentPending;
    public bool ClientApproved { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<RepairStateHistory> History { get; set; } = new List<RepairStateHistory>();
    public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    public ICollection<ComponentUsage> Components { get; set; } = new List<ComponentUsage>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public Review? Review { get; set; }
}
public class Diagnosis
{
    public int Id { get; set; }
    public int RepairId { get; set; }
    public Repair Repair { get; set; } = null!;
    [Required] public string TechnicianId { get; set; } = "";
    public ApplicationUser Technician { get; set; } = null!;
    [Required, StringLength(2000)] public string Findings { get; set; } = "";
    [StringLength(2000)] public string ProposedAction { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class RepairStateHistory
{
    public int Id { get; set; }
    public int RepairId { get; set; }
    public Repair Repair { get; set; } = null!;
    public RepairState State { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    [Required] public string ChangedById { get; set; } = "";
    public ApplicationUser ChangedBy { get; set; } = null!;
    [StringLength(2000)] public string Notes { get; set; } = "";
}
public class ComponentUsage
{
    public int Id { get; set; }
    public int RepairId { get; set; }
    public Repair Repair { get; set; } = null!;
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Range(1, 10000)] public int Quantity { get; set; } = 1;
    [Range(0, 100000)] public decimal UnitCost { get; set; }
}
public class Payment
{
    public int Id { get; set; }
    public int RepairId { get; set; }
    public Repair Repair { get; set; } = null!;
    [Range(0.01, 100000)] public decimal Amount { get; set; }
    [Required, StringLength(40)] public string Method { get; set; } = "";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    [Required] public string RegisteredById { get; set; } = "";
    public ApplicationUser RegisteredBy { get; set; } = null!;
}
public class Review
{
    public int Id { get; set; }
    public int RepairId { get; set; }
    public Repair Repair { get; set; } = null!;
    [Required] public string ClientId { get; set; } = "";
    public ApplicationUser Client { get; set; } = null!;
    [Range(1,5)] public int Rating { get; set; }
    [StringLength(1000)] public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class BusinessHour
{
    public int Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan OpensAt { get; set; }
    public TimeSpan ClosesAt { get; set; }
    public bool IsOpen { get; set; }
}
