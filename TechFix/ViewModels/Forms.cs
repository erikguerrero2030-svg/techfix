using System.ComponentModel.DataAnnotations;
using TechFix.Models;

namespace TechFix.ViewModels;
public class RegisterVm
{
    [Required, StringLength(100), Display(Name="Nombre completo")] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(10)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name="Confirmar contraseña")] public string ConfirmPassword { get; set; } = "";
}
public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
public class AppointmentVm
{
    [Required, Display(Name="Tipo de equipo")] public string DeviceType { get; set; } = "";
    [Required] public string Brand { get; set; } = "";
    [Required] public string Model { get; set; } = "";
    [Required, Phone] public string Phone { get; set; } = "";
    [Required, StringLength(1000), Display(Name="Problema")] public string Problem { get; set; } = "";
    [Range(1,int.MaxValue)] public int ServiceItemId { get; set; }
    [Required, DataType(DataType.DateTime)] public DateTime ScheduledAt { get; set; }
}
public class ServiceVm
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [StringLength(500)] public string Description { get; set; } = "";
    [Range(0,100000)] public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
public class TechnicianVm
{
    [Required] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, MinLength(10)] public string Password { get; set; } = "";
    public string Specialty { get; set; } = "";
}
public class RepairUpdateVm
{
    public int RepairId { get; set; }
    public RepairState State { get; set; }
    [StringLength(2000)] public string? Notes { get; set; }
    [StringLength(2000)] public string? Findings { get; set; }
    [StringLength(2000)] public string? ProposedAction { get; set; }
    [StringLength(100)] public string? ComponentName { get; set; }
    [Range(1,10000)] public int Quantity { get; set; } = 1;
    [Range(0,100000)] public decimal UnitCost { get; set; }
}

public record ReportMonthVm(int Year, int Month, decimal Total);
