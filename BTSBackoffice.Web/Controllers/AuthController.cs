using BTSBackoffice.Web.Models.ViewModels;
using BTSBackoffice.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BTSBackoffice.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        // If already authenticated, redirect to dashboard
        if (HttpContext.Session.GetString("IsAuthenticated") == "true")
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel());
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (_authService.IsAccountLockedOut(model.Username))
        {
            model.ErrorMessage = "帳號已被鎖定，請稍後再試";
            model.IsLocked = true;
            return View(model);
        }

        var isValid = await _authService.ValidateCredentialsAsync(model);

        if (isValid)
        {
            // Set session
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));

            _logger.LogInformation("User {Username} logged in successfully at {Time}",
                model.Username, DateTime.UtcNow.AddHours(8));

            return RedirectToAction("Index", "Dashboard");
        }
        else
        {
            model.ErrorMessage = "使用者名稱或密碼錯誤";
            model.RemainingAttempts = _authService.GetRemainingAttempts(model.Username);

            if (model.RemainingAttempts <= 0)
            {
                model.ErrorMessage = "登入失敗次數過多，帳號已被鎖定";
                model.IsLocked = true;
            }

            return View(model);
        }
    }

    [HttpGet("/logout")]
    public IActionResult Logout()
    {
        var username = HttpContext.Session.GetString("Username");

        HttpContext.Session.Clear();

        _logger.LogInformation("User {Username} logged out at {Time}",
            username, DateTime.UtcNow.AddHours(8));

        return RedirectToAction("Login");
    }
}