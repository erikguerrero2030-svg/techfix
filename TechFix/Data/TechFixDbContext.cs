using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechFix.Models;

namespace TechFix.Data;
public class TechFixDbContext(DbContextOptions<TechFixDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ClientProfile> Clients => Set<ClientProfile>();
    public DbSet<TechnicianProfile> Technicians => Set<TechnicianProfile>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Repair> Repairs => Set<Repair>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<RepairStateHistory> History => Set<RepairStateHistory>();
    public DbSet<ComponentUsage> Components => Set<ComponentUsage>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ApplicationUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<ClientProfile>().HasOne(x => x.User).WithOne(x => x.ClientProfile).HasForeignKey<ClientProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<TechnicianProfile>().HasOne(x => x.User).WithOne(x => x.TechnicianProfile).HasForeignKey<TechnicianProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Device>().HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.Device).WithMany().HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasOne(x => x.ServiceItem).WithMany().HasForeignKey(x => x.ServiceItemId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Appointment>().HasIndex(x => x.TrackingCode).IsUnique();
        b.Entity<Appointment>().HasIndex(x => x.ScheduledAt).IsUnique().HasFilter(Database.IsNpgsql() ? "\"Status\" <> 2" : "[Status] <> 2");
        b.Entity<Repair>().HasOne(x => x.Appointment).WithOne(x => x.Repair).HasForeignKey<Repair>(x => x.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Repair>().HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Diagnosis>().HasOne(x => x.Technician).WithMany().HasForeignKey(x => x.TechnicianId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RepairStateHistory>().HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedById).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Payment>().HasOne(x => x.RegisteredBy).WithMany().HasForeignKey(x => x.RegisteredById).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Review>().HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Review>().HasOne(x => x.Repair).WithOne(x => x.Review).HasForeignKey<Review>(x => x.RepairId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<BusinessHour>().HasIndex(x => x.Day).IsUnique();
        b.Entity<ServiceItem>().Property(x => x.Price).HasPrecision(10,2);
        b.Entity<ComponentUsage>().Property(x => x.UnitCost).HasPrecision(10,2);
        b.Entity<Payment>().Property(x => x.Amount).HasPrecision(10,2);
        b.Entity<Review>().ToTable(t => t.HasCheckConstraint("CK_Review_Rating", Database.IsNpgsql() ? "\"Rating\" BETWEEN 1 AND 5" : "[Rating] BETWEEN 1 AND 5"));
    }
}
