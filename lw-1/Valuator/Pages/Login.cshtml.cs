using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class LoginModel : PageModel
{
    private readonly IDBService _dbService;

    public LoginModel(IDBService dbService)
    {
        _dbService = dbService;
    }

    [BindProperty] public string Login { get; set; }
    [BindProperty] public string Password { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await _dbService.ValidateUser(Login, Password))
        {
            Error = "Неверные имя или пароль.";
            return Page();
        }

        var claims = new List<Claim> { new(ClaimTypes.Name, Login) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity)
        );

        return RedirectToPage("/Index");
    }
}