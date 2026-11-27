using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TechFix.Controllers;
using TechFix.Areas.Admin.Controllers;
using TechFix.Data;
using TechFix.Models;
using TechFix.ViewModels;
using Xunit;

namespace TechFix.Tests;
public class WorkflowTests
{
    private static (SqliteConnection, TechFixDbContext) Database()
    {
        var connection=new SqliteConnection("DataSource=:memory:"); connection.Open();
        var db=new TechFixDbContext(new DbContextOptionsBuilder<TechFixDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        return (connection,db);
    }
    private static ClaimsPrincipal Principal(string id) => new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier,id) },"Test"));
    private sealed class TempStore : ITempDataProvider
    {
        public IDictionary<string,object> LoadTempData(HttpContext context) => new Dictionary<string,object>();
        public void SaveTempData(HttpContext context, IDictionary<string,object> values) { }
    }
    private static AppointmentsController Appointments(TechFixDbContext db,string userId)
    {
        var context=new DefaultHttpContext { User=Principal(userId) };
        return new AppointmentsController(db) { ControllerContext=new ControllerContext { HttpContext=context }, TempData=new TempDataDictionary(context,new TempStore()) };
    }
    [Fact] public async Task Client_can_book_and_duplicate_slot_is_rejected()
    {
        var (connection,db)=Database(); using(connection) using(db)
        {
            db.Users.AddRange(new ApplicationUser { Id="client-a",UserName="a",FullName="A" },new ApplicationUser { Id="client-b",UserName="b",FullName="B" });
            db.Services.Add(new ServiceItem { Id=1, Name="Diagnóstico", Price=3 });
            var when=DateTime.Today.AddDays(2).AddHours(10);
            db.BusinessHours.Add(new BusinessHour { Day=when.DayOfWeek, OpensAt=new TimeSpan(9,0,0), ClosesAt=new TimeSpan(18,0,0), IsOpen=true });
            await db.SaveChangesAsync();
            var form=new AppointmentVm { DeviceType="Laptop",Brand="Lenovo",Model="X",Phone="0994665796",Problem="No enciende",ServiceItemId=1,ScheduledAt=when };
            var first=await Appointments(db,"client-a").Create(form);
            Assert.IsType<RedirectToActionResult>(first);
            Assert.Equal(1,await db.Appointments.CountAsync());
            Assert.Equal(1,await db.History.CountAsync());
            Assert.Equal(16,(await db.Appointments.SingleAsync()).TrackingCode.Length);
            var second=await Appointments(db,"client-b").Create(form);
            Assert.IsType<ViewResult>(second);
            Assert.Equal(1,await db.Appointments.CountAsync());
        }
    }
    [Fact] public async Task Client_cannot_open_another_clients_repair()
    {
        var (connection,db)=Database(); using(connection) using(db)
        {
            db.Users.AddRange(new ApplicationUser { Id="owner",UserName="o",FullName="Owner" },new ApplicationUser { Id="other",UserName="x",FullName="Other" });
            db.Services.Add(new ServiceItem { Id=1,Name="Servicio",Price=5 });
            var device=new Device { ClientId="owner",Type="Laptop",Brand="A",Model="B" };
            var appointment=new Appointment { ClientId="owner",Device=device,ServiceItemId=1,Phone="099",Problem="Test",TrackingCode="AABBCCDDEEFF0011",ScheduledAt=DateTime.Today.AddDays(1) };
            db.Repairs.Add(new Repair { Id=10,Appointment=appointment });
            await db.SaveChangesAsync();
            var controller=new RepairsController(db) { ControllerContext=new ControllerContext { HttpContext=new DefaultHttpContext { User=Principal("other") } } };
            Assert.IsType<NotFoundResult>(await controller.Details(10));
        }
    }
    [Fact] public async Task Technician_cannot_open_unassigned_repair()
    {
        var (connection,db)=Database(); using(connection) using(db)
        {
            db.Users.AddRange(new ApplicationUser { Id="owner",UserName="o",FullName="Owner" },new ApplicationUser { Id="tech-a",UserName="a",FullName="Tech A" },new ApplicationUser { Id="tech-b",UserName="b",FullName="Tech B" });
            db.Services.Add(new ServiceItem { Id=1,Name="Servicio",Price=5 });
            var appointment=new Appointment { ClientId="owner",Device=new Device { ClientId="owner",Type="Laptop",Brand="A",Model="B" },ServiceItemId=1,Phone="099",Problem="Test",TrackingCode="AABBCCDDEEFF0011",ScheduledAt=DateTime.Today.AddDays(1) };
            db.Repairs.Add(new Repair { Id=10,Appointment=appointment,TechnicianId="tech-a" });
            await db.SaveChangesAsync();
            var controller=new TechnicianController(db) { ControllerContext=new ControllerContext { HttpContext=new DefaultHttpContext { User=Principal("tech-b") } } };
            Assert.IsType<NotFoundResult>(await controller.Details(10));
        }
    }
    [Fact] public void Sensitive_controllers_require_the_correct_role()
    {
        Assert.Equal("Cliente",Attribute.GetCustomAttribute(typeof(AppointmentsController),typeof(AuthorizeAttribute)) is AuthorizeAttribute a?a.Roles:null);
        Assert.Equal("Cliente",Attribute.GetCustomAttribute(typeof(RepairsController),typeof(AuthorizeAttribute)) is AuthorizeAttribute b?b.Roles:null);
        Assert.Equal("Tecnico",Attribute.GetCustomAttribute(typeof(TechnicianController),typeof(AuthorizeAttribute)) is AuthorizeAttribute c?c.Roles:null);
        Assert.Equal("Administrador",Attribute.GetCustomAttribute(typeof(AdminController),typeof(AuthorizeAttribute)) is AuthorizeAttribute d?d.Roles:null);
    }
}
