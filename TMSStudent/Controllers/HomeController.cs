using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Models;
using TMSStudent.Services;

namespace TMSStudent.Controllers;

[Authorize(Roles = "Student")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserAPIService _userAPIService;

    public HomeController(IUserAPIService userAPIService, ILogger<HomeController> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userAPIService = userAPIService;
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            await EnsureUserExists();
            return RedirectToAction("Index", "Toy");
        }
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Dashboard");
        }
        return View();
    }

    [AllowAnonymous]
    public IActionResult SignIn()
    {
        var redirectUrl = Url.Action("Index", "Home");
        return Challenge(
            new AuthenticationProperties { RedirectUri = redirectUrl },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Authorize(Roles = "Student")]
    public IActionResult Dashboard()
    {
        return View("Index");
    }

    [Authorize(Roles = "Student")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(new List<Notification>());

        var notifications = await _notificationService.GetUserNotificationsAsync(oidClaim.Value);
        return Json(notifications);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationCount()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(0);

        var count = await _notificationService.GetUnreadCountAsync(oidClaim.Value);
        return Json(count);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteNotification([FromBody] int notificationId)
    {
        var result = await _notificationService.DeleteNotificationAsync(notificationId);
        return Json(new { success = result });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var oidClaim = User.FindFirst("oid") ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
        if (oidClaim == null) return Json(new { success = false });

        var result = await _notificationService.DeleteAllNotificationsAsync(oidClaim.Value);
        return Json(new { success = result });
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
    private async Task EnsureUserExists()
    {
        try
        {
            var oid = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                   ?? User.FindFirst("oid")?.Value;

            if (string.IsNullOrEmpty(oid))
            {
                _logger.LogWarning("No OID claim found for user");
                return;
            }

            var user = await _userAPIService.GetUserByOidAsync(oid);

            if (user == null)
            {
                var newUser = new User
                {
                    Oid = oid,
                    UserType = "Student"
                };

                await _userAPIService.CreateUserAsync(newUser);
                _logger.LogInformation($"Created new user with OID: {oid}");
            }
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogInformation("User needs to consent to API access. Redirecting to consent page.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring user exists");

        }
    }
}