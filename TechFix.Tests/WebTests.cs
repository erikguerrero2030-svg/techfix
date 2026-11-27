using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using TechFix.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechFix.Data;
using Xunit;

namespace TechFix.Tests;
public class WebTests
{
    private sealed class SiteFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        public SiteFactory() => _connection.Open();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseContentRoot(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..","..","TechFix")));
            builder.ConfigureServices(services => {
                services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.GetTempPath()));
                services.RemoveAll<DbContextOptions<TechFixDbContext>>();
                services.AddDbContext<TechFixDbContext>(options => options.UseSqlite(_connection));
            });
        }
        protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) _connection.Dispose(); }
    }
    private static string Token(string html) => Regex.Match(html,"name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
    private static async Task Login(HttpClient client,string email)
    {
        var page=await client.GetStringAsync("/Account/Login");
        var response=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["Email"]=email,["Password"]="Testing#12345",["__RequestVerificationToken"]=Token(page)
        }));
        Assert.Equal(HttpStatusCode.Redirect,response.StatusCode);
    }
    [Fact] public async Task Repair_moves_from_booking_to_delivery_and_review()
    {
        using var factory=new SiteFactory();
        var opts=new WebApplicationFactoryClientOptions { AllowAutoRedirect=false, BaseAddress=new Uri("https://localhost") };
        using var client=factory.CreateClient(opts);
        using var adminClient=factory.CreateClient(opts);
        using var technicianClient=factory.CreateClient(opts);
        await client.GetAsync("/");
        string technicianId;
        using(var scope=factory.Services.CreateScope())
        {
            var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            foreach(var (email,role) in new[] { ("client@techfix.local","Cliente"),("admin2@techfix.local","Administrador"),("worker@techfix.local","Tecnico") })
            {
                var user=new ApplicationUser { UserName=email,Email=email,FullName=email,EmailConfirmed=true };
                Assert.True((await users.CreateAsync(user,"Testing#12345")).Succeeded);
                Assert.True((await users.AddToRoleAsync(user,role)).Succeeded);
                if(role=="Tecnico") technicianId=user.Id;
            }
            technicianId=(await users.FindByEmailAsync("worker@techfix.local"))!.Id;
            var db=scope.ServiceProvider.GetRequiredService<TechFixDbContext>();
            db.Technicians.Add(new TechnicianProfile { UserId=technicianId });
            await db.SaveChangesAsync();
        }
        await Login(client,"client@techfix.local");
        await Login(adminClient,"admin2@techfix.local");
        await Login(technicianClient,"worker@techfix.local");
        var create=await client.GetStringAsync("/Appointments/Create");
        var when=DateTime.Today.AddDays(7 + ((8-(int)DateTime.Today.DayOfWeek)%7)).AddHours(11);
        var booked=await client.PostAsync("/Appointments/Create",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["DeviceType"]="Laptop",["Brand"]="Dell",["Model"]="Latitude",["Phone"]="0990000000",["Problem"]="Pantalla negra",["ServiceItemId"]="1",["ScheduledAt"]=when.ToString("yyyy-MM-ddTHH:mm"),["__RequestVerificationToken"]=Token(create)
        }));
        Assert.Equal(HttpStatusCode.Redirect,booked.StatusCode);
        int appointmentId, repairId;
        using(var scope=factory.Services.CreateScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<TechFixDbContext>();
            var appointment=await db.Appointments.Include(x=>x.Repair).SingleAsync();
            appointmentId=appointment.Id; repairId=appointment.Repair!.Id;
        }
        var adminPage=await adminClient.GetStringAsync("/Admin/Appointments");
        var confirmed=await adminClient.PostAsync($"/Admin/Confirm/{appointmentId}",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(adminPage) }));
        Assert.Equal(HttpStatusCode.Redirect,confirmed.StatusCode);
        adminPage=await adminClient.GetStringAsync("/Admin/Appointments");
        var assigned=await adminClient.PostAsync($"/Admin/Assign/{repairId}",new FormUrlEncodedContent(new Dictionary<string,string> { ["technicianId"]=technicianId,["__RequestVerificationToken"]=Token(adminPage) }));
        Assert.Equal(HttpStatusCode.Redirect,assigned.StatusCode);
        foreach(var state in new[] { RepairState.DeviceReceived,RepairState.Diagnosing,RepairState.AwaitingClientApproval })
        {
            var detail=await technicianClient.GetStringAsync($"/Technician/Details/{repairId}");
            var changed=await technicianClient.PostAsync("/Technician/Update",new FormUrlEncodedContent(new Dictionary<string,string> { ["RepairId"]=repairId.ToString(),["State"]=((int)state).ToString(),["Notes"]="Avance de prueba",["Findings"]=state==RepairState.AwaitingClientApproval?"Falla de pantalla":"",["ProposedAction"]="Revisar pantalla",["Quantity"]="1",["UnitCost"]="0",["__RequestVerificationToken"]=Token(detail) }));
            Assert.Equal(HttpStatusCode.Redirect,changed.StatusCode);
        }
        var clientDetail=await client.GetStringAsync($"/Repairs/Details/{repairId}");
        var approved=await client.PostAsync($"/Repairs/Approve/{repairId}",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(clientDetail) }));
        Assert.Equal(HttpStatusCode.Redirect,approved.StatusCode);
        foreach(var state in new[] { RepairState.RepairCompleted,RepairState.DeviceDelivered })
        {
            var detail=await technicianClient.GetStringAsync($"/Technician/Details/{repairId}");
            var changed=await technicianClient.PostAsync("/Technician/Update",new FormUrlEncodedContent(new Dictionary<string,string> { ["RepairId"]=repairId.ToString(),["State"]=((int)state).ToString(),["Notes"]="Avance de prueba",["Quantity"]="1",["UnitCost"]="0",["__RequestVerificationToken"]=Token(detail) }));
            Assert.Equal(HttpStatusCode.Redirect,changed.StatusCode);
        }
        var payments=await adminClient.GetStringAsync("/Admin/Payments");
        Assert.Contains($"value=\"{repairId}\"",payments);
        var invalidPayment=await adminClient.PostAsync("/Admin/Payment",new FormUrlEncodedContent(new Dictionary<string,string> { ["repairId"]="99999",["amount"]="3",["method"]="Efectivo",["__RequestVerificationToken"]=Token(payments) }));
        Assert.Equal(HttpStatusCode.Redirect,invalidPayment.StatusCode);
        payments=await adminClient.GetStringAsync("/Admin/Payments");
        Assert.Contains("Selecciona una reparación terminada",payments);
        var paid=await adminClient.PostAsync("/Admin/Payment",new FormUrlEncodedContent(new Dictionary<string,string> { ["repairId"]=repairId.ToString(),["amount"]="3",["method"]="Efectivo",["__RequestVerificationToken"]=Token(payments) }));
        Assert.Equal(HttpStatusCode.Redirect,paid.StatusCode);
        clientDetail=await client.GetStringAsync($"/Repairs/Details/{repairId}");
        var reviewed=await client.PostAsync($"/Repairs/Review/{repairId}",new FormUrlEncodedContent(new Dictionary<string,string> { ["rating"]="5",["comment"]="Buen servicio",["__RequestVerificationToken"]=Token(clientDetail) }));
        Assert.Equal(HttpStatusCode.Redirect,reviewed.StatusCode);
        using(var scope=factory.Services.CreateScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<TechFixDbContext>();
            var repair=await db.Repairs.Include(x=>x.History).Include(x=>x.Payments).Include(x=>x.Review).SingleAsync();
            Assert.Equal(RepairState.DeviceDelivered,repair.State);
            Assert.True(repair.ClientApproved);
            Assert.Equal(8,repair.History.Count);
            Assert.Single(repair.Payments);
            Assert.Equal(5,repair.Review?.Rating);
        }
    }
    [Fact] public async Task Administrator_can_open_management_pages()
    {
        using var factory=new SiteFactory();
        using var client=factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect=false, BaseAddress=new Uri("https://localhost") });
        await client.GetAsync("/");
        using(var scope=factory.Services.CreateScope())
        {
            var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin=new ApplicationUser { UserName="admin@techfix.local",Email="admin@techfix.local",FullName="Admin de prueba",EmailConfirmed=true };
            Assert.True((await users.CreateAsync(admin,"Testing#12345")).Succeeded);
            Assert.True((await users.AddToRoleAsync(admin,"Administrador")).Succeeded);
        }
        var loginPage=await client.GetStringAsync("/Account/Login");
        var login=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["Email"]="admin@techfix.local",["Password"]="Testing#12345",["__RequestVerificationToken"]=Token(loginPage)
        }));
        Assert.Equal(HttpStatusCode.Redirect,login.StatusCode);
        foreach(var route in new[] { "/Admin", "/Admin/Appointments", "/Admin/Services", "/Admin/Technicians", "/Admin/Clients", "/Admin/Hours", "/Admin/Payments", "/Admin/Reports" })
        {
            var response=await client.GetAsync(route);
            Assert.True(response.StatusCode==HttpStatusCode.OK,$"{route}: {response.StatusCode}");
        }
        var techForm=await client.GetStringAsync("/Admin/NewTechnician");
        var created=await client.PostAsync("/Admin/NewTechnician",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["FullName"]="Técnico de prueba",["Email"]="tech@techfix.local",["Password"]="Testing#12345",["Specialty"]="Laptops",["__RequestVerificationToken"]=Token(techForm)
        }));
        Assert.Equal(HttpStatusCode.Redirect,created.StatusCode);
        var dashboard=await client.GetStringAsync("/Admin");
        await client.PostAsync("/Account/Logout",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(dashboard) }));
        var techLoginPage=await client.GetStringAsync("/Account/Login");
        var techLogin=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["Email"]="tech@techfix.local",["Password"]="Testing#12345",["__RequestVerificationToken"]=Token(techLoginPage)
        }));
        Assert.Equal(HttpStatusCode.Redirect,techLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/Technician")).StatusCode);
        var forbidden=await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Redirect,forbidden.StatusCode);
        Assert.Contains("/Account/Denied",forbidden.Headers.Location?.ToString());
    }
    [Fact] public async Task Public_home_registration_booking_and_admin_denial_work()
    {
        using var factory=new SiteFactory();
        using var client=factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect=false, BaseAddress=new Uri("https://localhost") });
        var home=await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK,home.StatusCode);
        var homeHtml=await home.Content.ReadAsStringAsync();
        Assert.Contains("Diagnóstico básico",WebUtility.HtmlDecode(homeHtml));
        if (Environment.GetEnvironmentVariable("TECHFIX_PREVIEW_OUTPUT") is string previewPath) File.WriteAllText(previewPath,homeHtml);
        var admin=await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Redirect,admin.StatusCode);
        Assert.Contains("/Account/Login",admin.Headers.Location?.ToString());
        var register=await client.GetStringAsync("/Account/Register");
        var token=Token(register); Assert.NotEmpty(token);
        var result=await client.PostAsync("/Account/Register",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["FullName"]="Estudiante de prueba",["Email"]="test@techfix.local",["Password"]="Testing#12345",["ConfirmPassword"]="Testing#12345",["__RequestVerificationToken"]=token
        }));
        Assert.Equal(HttpStatusCode.Redirect,result.StatusCode);
        var denied=await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.Redirect,denied.StatusCode);
        Assert.Contains("/Account/Denied",denied.Headers.Location?.ToString());
        var create=await client.GetStringAsync("/Appointments/Create");
        var bookingToken=Token(create); Assert.NotEmpty(bookingToken);
        var when=DateTime.Today.AddDays(7 + ((8-(int)DateTime.Today.DayOfWeek)%7)).AddHours(10);
        var booking=await client.PostAsync("/Appointments/Create",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["DeviceType"]="Laptop",["Brand"]="Lenovo",["Model"]="IdeaPad",["Phone"]="0990000000",["Problem"]="No enciende",["ServiceItemId"]="1",["ScheduledAt"]=when.ToString("yyyy-MM-ddTHH:mm"),["__RequestVerificationToken"]=bookingToken
        }));
        Assert.Equal(HttpStatusCode.Redirect,booking.StatusCode);
        var appointments=await client.GetStringAsync("/Appointments");
        Assert.Contains("IdeaPad",appointments);
        var withoutToken=await client.PostAsync("/Appointments/Create",new FormUrlEncodedContent(new Dictionary<string,string> { ["DeviceType"]="Laptop" }));
        Assert.Equal(HttpStatusCode.BadRequest,withoutToken.StatusCode);
        var logout=await client.PostAsync("/Account/Logout",new FormUrlEncodedContent(new Dictionary<string,string> { ["__RequestVerificationToken"]=Token(appointments) }));
        Assert.Equal(HttpStatusCode.Redirect,logout.StatusCode);
        var loginPage=await client.GetStringAsync("/Account/Login");
        var login=await client.PostAsync("/Account/Login",new FormUrlEncodedContent(new Dictionary<string,string> {
            ["Email"]="test@techfix.local",["Password"]="Testing#12345",["__RequestVerificationToken"]=Token(loginPage)
        }));
        Assert.Equal(HttpStatusCode.Redirect,login.StatusCode);
        Assert.Contains("IdeaPad",await client.GetStringAsync("/Appointments"));
    }
}
