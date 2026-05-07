using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Models;
using TMSTeacher.Services;

namespace TMSTeacher.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly INotificationService _notificationService;
    private readonly IUserAPIService _userAPIService;

    public HomeController(
        ILogger<HomeController> logger,
        INotificationService notificationService,
        IUserAPIService userAPIService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _userAPIService = userAPIService;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        await EnsureUserExists();
        return RedirectToAction("Index", "Toy");
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> TeacherDashboard()
    {
        await EnsureUserExists();
        return RedirectToAction("Index", "Toy");
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            await EnsureUserExists();

            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminDashboard");
            else if (User.IsInRole("Teacher"))
                return RedirectToAction("TeacherDashboard");
            return RedirectToAction("AccessDenied");
        }
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminDashboard");
            else if (User.IsInRole("Teacher"))
                return RedirectToAction("TeacherDashboard");
            return RedirectToAction("AccessDenied");
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
                string userType = "Student"; 
                if (User.IsInRole("Admin") || User.IsInRole("Teacher"))
                {
                    userType = "Teacher";
                }

                var newUser = new User
                {
                    Oid = oid,
                    UserType = userType
                };

                await _userAPIService.CreateUserAsync(newUser);
                _logger.LogInformation($"Created new {userType} user with OID: {oid}");
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