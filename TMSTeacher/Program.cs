using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TMSTeacher.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.HandleSameSiteCookieCompatibility();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var auth = builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.UsePkce = true;
        options.SaveTokens = false;
        options.ResponseType = "code";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Events.OnTokenValidated += async context =>
        {
            var roleClaims = context.Principal.FindAll("roles");
            var identity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;

            foreach (var roleClaim in roleClaims)
            {
                var role = roleClaim.Value;
                identity.AddClaim(new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Role, role));
            }

            var oidClaim = context.Principal.FindFirst("oid") ??
                          context.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (oidClaim != null && !context.Principal.HasClaim(c => c.Type == "oid"))
            {
                identity.AddClaim(new System.Security.Claims.Claim("oid", oidClaim.Value));
            }
        };

        options.Events.OnSignedOutCallbackRedirect = context =>
        {
            context.Response.Redirect("/Home/Login");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi(new[] {
        "User.Read",
        "User.Read.All"
    })
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApis:MicrosoftGraph"))
    .AddDistributedTokenCaches();

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = ".AspNetCore.Teacher.Cookies";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
});

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.NonceCookie.Name = ".AspNetCore.Teacher.Nonce";
    options.CorrelationCookie.Name = ".AspNetCore.Teacher.Correlation";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireTeacherRole", policy =>
        policy.RequireRole("Teacher"));

    options.AddPolicy("RequireStudentRole", policy =>
        policy.RequireRole("Student"));
});

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
})
.AddMicrosoftIdentityUI();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddScoped<ReportsPDFService>();

var apiBase = builder.Configuration["APIUrl"] ?? "https://team2app-csdevwww.cegep-heritage.qc.ca/api/";

if (string.IsNullOrEmpty(apiBase))
{
    throw new InvalidOperationException("APIUrl is not configured in appsettings.json");
}

builder.Services.AddTransient<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IUserAPIService, UserAPIService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IStudentAPIService, StudentAPIService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<ITeacherAPIService, TeacherAPIService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IToyAPIService, ToyAPIService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<INotificationService, NotificationService>(client =>
{
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient("TmsApi", client => {
    client.BaseAddress = new Uri(apiBase);
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();