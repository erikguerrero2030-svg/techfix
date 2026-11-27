using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechFix.Models;
using TechFix.Services;

namespace TechFix.Data;

// Datos inventados para poblar el flujo de presentación; nunca cuentan como resultados reales.
public static class SampleDataSeeder
{
    public static async Task RunAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TechFixDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        if (await db.Appointments.AnyAsync(x => x.IsSynthetic) ||
            await db.Users.AnyAsync(x => x.Email != null && x.Email.EndsWith("@techfix.invalid")))
        {
            Console.WriteLine("Los registros de presentación ya existen; no se duplicaron.");
            return;
        }

        var admin = (await users.GetUsersInRoleAsync("Administrador")).FirstOrDefault()
            ?? throw new InvalidOperationException("Primero crea una cuenta de administrador.");
        var servicesList = await db.Services.Where(x => x.IsActive).OrderBy(x => x.Id).ToListAsync();
        if (servicesList.Count == 0) throw new InvalidOperationException("No hay servicios activos.");

        string[] names = [
            "Ana Belén Rosero", "Carlos Andrés Cárdenas", "María José Guerrero", "Luis Fernando Paredes",
            "Daniela Paola Benavides", "Juan Sebastián Mora", "Valeria Alejandra Paz", "Mateo Alejandro Enríquez",
            "Camila Fernanda Andrade", "David Esteban López", "Sofía Isabel Chicaiza", "Diego Mauricio Martínez",
            "Paula Carolina Cevallos", "Andrés Felipe Ayala", "Gabriela Nicole Villota", "Nicolás Eduardo Pozo",
            "Karla Michelle Viveros", "Jorge Iván Ortega", "Lucía Fernanda Tapia", "Cristian Javier Chamorro"
        ];
        string[] deviceTypes = ["Laptop", "Computadora de escritorio", "Laptop", "Laptop"];
        string[] brands = ["HP", "Lenovo", "Dell", "Acer", "Asus"];
        string[] models = ["Pavilion 15", "IdeaPad 3", "Inspiron 15", "Aspire 5", "VivoBook 14"];
        string[] problems = [
            "El equipo tarda demasiado en iniciar y necesita revisión.",
            "Se calienta y el ventilador hace ruido al trabajar.",
            "El sistema funciona lento y requiere optimización.",
            "Necesita respaldo de archivos antes del mantenimiento.",
            "Requiere instalación y configuración de programas de estudio.",
            "Solicita mantenimiento preventivo y revisión general."
        ];
        string[] findings = [
            "Se revisó el arranque y se verificó el funcionamiento general.",
            "Se encontró acumulación de polvo y se realizó limpieza preventiva.",
            "Se ajustaron programas de inicio y se verificó el rendimiento.",
            "Se respaldaron los archivos indicados y se comprobó la copia.",
            "Se instaló y configuró el software solicitado.",
            "Se completó la limpieza y optimización general del equipo."
        ];
        var samplePassword = "Tf#1" + Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
        await using var tx = await db.Database.BeginTransactionAsync();
        var technician = new ApplicationUser {
            UserName = "tecnico01@techfix.invalid", Email = "tecnico01@techfix.invalid",
            FullName = "Técnico TechFix", EmailConfirmed = true
        };
        Check(await users.CreateAsync(technician, samplePassword));
        Check(await users.AddToRoleAsync(technician, "Tecnico"));
        db.Technicians.Add(new TechnicianProfile { UserId = technician.Id, Specialty = "Mantenimiento de computadoras" });

        var today = DateTime.Today;
        var pastDates = Enumerable.Range(1, 40).Select(n => today.AddDays(-n))
            .Where(d => d.DayOfWeek != DayOfWeek.Sunday).Take(16).Reverse().ToArray();
        var nextOpenDay = today.AddDays(1);
        while (nextOpenDay.DayOfWeek == DayOfWeek.Sunday) nextOpenDay = nextOpenDay.AddDays(1);

        for (var i = 0; i < names.Length; i++)
        {
            var email = $"cliente{i + 1:00}@techfix.invalid";
            var client = new ApplicationUser {
                UserName = email, Email = email, FullName = names[i], EmailConfirmed = true
            };
            Check(await users.CreateAsync(client, samplePassword));
            Check(await users.AddToRoleAsync(client, "Cliente"));
            db.Clients.Add(new ClientProfile { UserId = client.Id });

            var device = new Device { ClientId = client.Id, Type = deviceTypes[i % deviceTypes.Length],
                Brand = brands[i % brands.Length], Model = models[i % models.Length] };
            var completed = i < 16;
            var scheduled = completed ? pastDates[i].AddHours(10) : nextOpenDay.AddHours(10 + i - 16);
            var appointment = new Appointment {
                ClientId = client.Id, Device = device,
                ServiceItemId = servicesList[i % servicesList.Count].Id,
                Phone = "0000000000", Problem = problems[i % problems.Length],
                ScheduledAt = scheduled, Status = completed ? AppointmentStatus.Confirmed : AppointmentStatus.Pending,
                IsSynthetic = true, TrackingCode = TrackingCodeGenerator.Create(),
                CreatedAt = scheduled.AddDays(-2)
            };
            var repair = new Repair {
                Appointment = appointment, TechnicianId = completed ? technician.Id : null,
                State = completed ? RepairState.DeviceDelivered : RepairState.AppointmentPending,
                ClientApproved = completed, CompletedAt = completed ? scheduled.AddDays(1).ToUniversalTime() : null
            };
            var firstEvent = scheduled.AddDays(-2).ToUniversalTime();
            if (completed)
            {
                repair.Diagnoses.Add(new Diagnosis {
                    TechnicianId = technician.Id, Findings = findings[i % findings.Length],
                    ProposedAction = "Trabajo realizado y equipo verificado.", CreatedAt = scheduled.ToUniversalTime()
                });
                var states = new[] { RepairState.AppointmentPending, RepairState.AppointmentConfirmed,
                    RepairState.DeviceReceived, RepairState.Diagnosing, RepairState.AwaitingClientApproval,
                    RepairState.Repairing, RepairState.RepairCompleted, RepairState.DeviceDelivered };
                for (var j = 0; j < states.Length; j++)
                    repair.History.Add(new RepairStateHistory {
                        State = states[j], ChangedById = j == 0 || j == 4 ? client.Id : j == 1 ? admin.Id : technician.Id,
                        ChangedAt = firstEvent.AddHours(j * 8), Notes = states[j].ToString()
                    });
            }
            else repair.History.Add(new RepairStateHistory {
                State = RepairState.AppointmentPending, ChangedById = client.Id,
                ChangedAt = firstEvent, Notes = "Cita solicitada"
            });
            db.Repairs.Add(repair);
        }
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        Console.WriteLine("Se crearon 20 clientes, 16 servicios terminados y 4 citas pendientes.");
        Console.WriteLine($"Acceso de cliente: cliente01@techfix.invalid / {samplePassword}");
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
