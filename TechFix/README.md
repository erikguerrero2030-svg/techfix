# TechFix

Prototipo universitario de mantenimiento, diagnóstico y reparación de computadoras y laptops. Desarrollado con ASP.NET Core MVC (.NET 8), Entity Framework Core, SQL Server e Identity.

## Requisitos

- Visual Studio Community con la carga de trabajo «Desarrollo de ASP.NET y web» y el SDK de .NET 8.
- Runtime ASP.NET Core 8 de 64 bits para ejecutar la aplicación.
- SQL Server Express o LocalDB.
- Acceso a NuGet la primera vez que se restaura la solución.

## Abrir y ejecutar

1. Extrae el ZIP completo y abre `TechFix.slnx` en Visual Studio.
2. Espera a que Visual Studio restaure NuGet. El proyecto web es `TechFix`.
3. La conexión predeterminada en `TechFix/appsettings.json` usa `(localdb)\MSSQLLocalDB` y crea `TechFixDB`.
4. Para SQL Server Express, configura la cadena `ConnectionStrings:TechFixDB` con `Server=.\SQLEXPRESS;Database=TechFixDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`. Puedes hacerlo con secretos de usuario de Visual Studio o con una variable de entorno `ConnectionStrings__TechFixDB`.
5. La aplicación aplica las migraciones al arrancar y carga el catálogo inicial. La cuenta del primer administrador se crea por separado, como se indica abajo.
6. Establece `TechFix` como proyecto de inicio y ejecuta con F5. Usa la URL HTTPS que muestra Visual Studio.

Si LocalDB no se inicia, comprueba en tu equipo que esté instalado y funcionando. No copies archivos internos `.mdf`, `.frm` ni `.ibd` para sustituir una migración.

## Primer administrador

Desde una terminal en la carpeta de la solución, ejecuta:

```powershell
dotnet run --project TechFix -- --create-admin
```

La consola pide correo y contraseña. La contraseña no aparece al escribir y no está fijada en el código. Este comando solo funciona si todavía no existe un administrador. Cierra la consola cuando confirme la creación y luego inicia la web normalmente. La contraseña requiere al menos 10 caracteres, una cifra y un símbolo.

## Roles y flujo de demostración

- **Cliente**: se registra públicamente, inicia sesión, reserva una cita, consulta solo sus citas y reparaciones, cancela una cita pendiente, autoriza una reparación y valora un equipo entregado.
- **Administrador**: inicia sesión con la cuenta creada desde la consola, cambia servicios y precios, confirma citas, crea técnicos, asigna reparaciones, registra pagos y consulta reportes. Puede activar o desactivar clientes y técnicos y modificar horarios.
- **Técnico**: la cuenta la crea el administrador. Ve solo las reparaciones asignadas, registra diagnóstico, componentes y avances.

Flujo recomendado: crear administrador → crear técnico → crear cliente → reservar una cita → confirmar y asignar → recibir equipo → diagnosticar → solicitar autorización → el cliente autoriza → finalizar → entregar → valorar → registrar pago.

Para preparar una instalación vacía con 20 clientes inventados, 16 servicios terminados y 4 citas pendientes, ejecuta `dotnet run --project TechFix -- --seed-sample-data` después de crear el administrador. El comando evita duplicados y muestra una credencial de cliente al terminar. Estos registros llevan una marca interna y se excluyen de las cifras de reparaciones reales e ingresos. La carga de datos no viaja en el ZIP: cada base de datos se prepara por separado.

Los códigos de seguimiento se generan con números aleatorios criptográficos. La consulta de reparaciones requiere iniciar sesión y comprueba que la reparación pertenece al cliente.

## Base de datos y seguridad

Las entidades están en `Models/Domain.cs`; el contexto está en `Data/TechFixDbContext.cs`; la migración está en `Migrations/`. Identity almacena contraseñas como hash. Las páginas y operaciones sensibles tienen autorización por rol, comprobación de propiedad y protección antifalsificación CSRF. Las cuentas públicas solo reciben el rol Cliente. Después de cinco intentos fallidos, Identity aplica un bloqueo temporal de 15 minutos. La sesión de una cuenta desactivada se rechaza al validar su cookie.

La reserva comprueba el horario de atención y la disponibilidad; un índice único evita dos citas activas a la misma hora. La hora de la cita se interpreta en la hora local del servidor, por lo que el servidor debe configurarse para la zona del negocio. Para un despliegue público, configura HTTPS con un certificado válido y cambia la cadena de conexión de desarrollo.

Los precios iniciales son los solicitados: $3, $5, $5, $5, $8 y $12. Se almacenan en la tabla `Services` y se modifican desde el panel administrativo.

## Pruebas

```powershell
dotnet test TechFix.slnx -m:1
```

`TechFix.Tests` comprueba la portada, registro e inicio de sesión, reserva y rechazo de horarios ocupados, filtros de propiedad, autorización por rol, protección CSRF, creación de técnicos y un ciclo completo desde la cita hasta entrega, pago y valoración. Las pruebas usan SQLite en memoria para comprobar la lógica de forma aislada; la aplicación principal usa SQL Server. La conexión real con SQL Server y el diseño en distintos dispositivos deben verificarse en el equipo donde se presente el prototipo. La migración genera además `Data/TechFixDB.sql` para crear las tablas en una base SQL Server existente.

## Datos de contacto

Edita `TechFix/appsettings.json` en la sección `TechFix` para cambiar correo, teléfonos y ciudad. Los datos actuales fueron proporcionados por el solicitante. No se han inventado estadísticas ambientales: el indicador de equipos reparados proviene de los registros reales del sistema.

## Metodología del prototipo

Consulta `docs/Prototipo.md` para la hipótesis del problema, el alcance del MVP y las métricas que pueden validarse con usuarios reales.
