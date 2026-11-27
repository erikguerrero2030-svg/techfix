using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechFix.Data;
using TechFix.Models;
using TechFix.ViewModels;

namespace TechFix.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, TechFixDbContext db) : Controller
{
    [HttpGet] public IActionResult Register() => View(new RegisterVm());
    [HttpPost] public async Task<IActionResult> Register(RegisterVm model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName=model.Email.Trim(), Email=model.Email.Trim(), FullName=model.FullName.Trim(), EmailConfirmed=true };
        await using var tx = await db.Database.BeginTransactionAsync();
        var result = await users.CreateAsync(user, model.Password);
        if (!result.Succeeded) { foreach (var error in result.Errors) ModelState.AddModelError("",error.Description); return View(model); }
        result = await users.AddToRoleAsync(user,"Cliente");
        if (!result.Succeeded) { foreach (var error in result.Errors) ModelState.AddModelError("",error.Description); return View(model); }
        db.Clients.Add(new ClientProfile { UserId=user.Id });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        await signIn.SignInAsync(user,false);
        return RedirectToAction("Index","Appointments");
    }
    [HttpGet] public IActionResult Login(string? returnUrl=null) { ViewBag.ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:null; return View(new LoginVm()); }
    [HttpPost] public async Task<IActionResult> Login(LoginVm model, string? returnUrl=null)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.FindByEmailAsync(model.Email.Trim());
        if (user is null || !user.IsActive) { ModelState.AddModelError("","Correo o contraseña incorrectos."); return View(model); }
        var result = await signIn.PasswordSignInAsync(user,model.Password,model.RememberMe,lockoutOnFailure:true);
        if (result.IsLockedOut) ModelState.AddModelError("","Cuenta bloqueada temporalmente. Intenta más tarde.");
        else if (!result.Succeeded) ModelState.AddModelError("","Correo o contraseña incorrectos.");
        else if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl);
        else if (await users.IsInRoleAsync(user,"Administrador")) return RedirectToAction("Index","Admin",new { area="Admin" });
        else if (await users.IsInRoleAsync(user,"Tecnico")) return RedirectToAction("Index","Technician");
        else return RedirectToAction("Index","Appointments");
        return View(model);
    }
    [HttpPost] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index","Home"); }
    public IActionResult Denied() => View();
}
