using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.GitHub;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZapWatch.Web.Data;
using ZapWatch.Web.Infrastructure;
using ZapWatch.Web.Hubs;
using ZapWatch.Web.Jobs;
using ZapWatch.Web.Models;
using ZapWatch.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Persist Data Protection keys (used for antiforgery tokens, auth cookies) in SQL Server
// instead of the default local filesystem/in-memory store. On shared IIS hosting, worker
// process recycles can otherwise regenerate keys mid-session and invalidate tokens issued
// moments earlier - manifests as a bare 400 on any [ValidateAntiForgeryToken] POST.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("ZapWatch");

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

// Empty ClientId/ClientSecret is fine at startup - OAuthOptions only validates them lazily,
// the first time someone actually hits /signin-google. Real values come from GitHub Actions
// secrets at deploy time (see build-test-deploy.yml), same pattern as Resend/Twilio/Razorpay.
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-microsoft";
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-github";
        options.Scope.Add("user:email");

        // GitHub only includes an email in the base profile response if the user made one
        // public - most accounts don't. Fall back to the /user/emails endpoint (needs the
        // user:email scope above) and use the primary verified address, so real GitHub
        // sign-ins don't hit ExternalLoginCallback's "didn't share an email" rejection by default.
        options.Events.OnCreatingTicket = async context =>
        {
            if (context.Identity!.FindFirst(ClaimTypes.Email) is not null)
            {
                return;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            request.Headers.UserAgent.ParseAdd("ZapWatch");

            using var response = await context.Backchannel.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            using var payload = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
            var primaryEmail = payload.RootElement.EnumerateArray()
                .FirstOrDefault(e => e.GetProperty("primary").GetBoolean() && e.GetProperty("verified").GetBoolean());

            if (primaryEmail.ValueKind == JsonValueKind.Object)
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.Email, primaryEmail.GetProperty("email").GetString()!));
            }
        };
    });

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));
builder.Services.Configure<TwilioOptions>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddHttpClient<IEmailSender, ResendEmailSender>();
builder.Services.AddHttpClient<ISmsSender, TwilioSmsSender>();
builder.Services.AddScoped<IAlertNotifier, CompositeAlertNotifier>();

builder.Services.Configure<RazorpayOptions>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddHttpClient<RazorpayClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var adminEmail = builder.Configuration["AdminEmail"]
    ?? throw new InvalidOperationException("AdminEmail not configured.");
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthFilter(adminEmail)]
});

app.MapStaticAssets();

app.MapHub<AutomationStatusHub>("/hubs/automation-status");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Watchdog: scans for overdue automations every 2 minutes
RecurringJob.AddOrUpdate<WatchdogJob>(
    "watchdog-scan",
    job => job.ScanForOverdueAutomations(),
    "*/2 * * * *");

app.Run();
