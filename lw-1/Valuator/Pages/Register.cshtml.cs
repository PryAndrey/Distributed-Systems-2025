using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class RegisterModel : PageModel
{
    private readonly IDBService _dbService;

    public RegisterModel(IDBService dbService)
    {
        _dbService = dbService;
    }

    [BindProperty]
    public string Login { get; set; }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty]
    public string ConfirmPassword { get; set; }

    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            Error = "Пароли не совпадают.";
            return Page();
        }

        var ok = await _dbService.RegisterUser(Login, Password);
        if (!ok)
        {
            Error = "Пользователь уже существует.";
            return Page();
        }

        return RedirectToPage("/Login");
    }
}