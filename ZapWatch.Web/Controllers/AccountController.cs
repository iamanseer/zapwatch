using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using ZapWatch.Web.Models;
using ZapWatch.Web.Models.Account;
using ZapWatch.Web.Services;

namespace ZapWatch.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender emailSender,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };
        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await SendConfirmationEmailAsync(user);
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToLocal(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked out. Try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is not null)
        {
            try
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetUrl = Url.Action(nameof(ResetPassword), "Account",
                    new { userId = user.Id, token = encodedToken }, Request.Scheme)!;

                await emailSender.SendAsync(
                    user.Email!,
                    "Reset your ZapWatch password",
                    $"<p>Click below to set a new password for {user.Email}:</p>"
                    + $"<p><a href=\"{resetUrl}\">Reset my password</a></p>"
                    + "<p>If you didn't request this, you can ignore this email.</p>");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send password reset email to {Email}", user.Email);
            }
        }

        // Same response whether or not the account exists - don't leak which emails are registered.
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? userId, string? token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel { UserId = userId, Token = token });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            // Don't reveal whether the account exists.
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "That reset link is invalid or has expired.");
            return View(model);
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        return RedirectToAction(nameof(ResetPasswordConfirmation));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        TempData[result.Succeeded ? "EmailConfirmed" : "ExternalLoginError"] = result.Succeeded
            ? "true"
            : "That verification link is invalid or has expired. Request a new one from your account page.";

        return RedirectToAction(User.Identity?.IsAuthenticated == true ? nameof(Profile) : nameof(Login));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is not null && !user.EmailConfirmed)
        {
            await SendConfirmationEmailAsync(user);
        }

        TempData["ConfirmationResent"] = "true";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl, provider });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(
        string? returnUrl = null, string? remoteError = null, string? provider = null)
    {
        // Scheme names ("Google"/"Microsoft"/"GitHub") already read as display names - no
        // separate display-name map needed. Passed through as a route value from ExternalLogin
        // since it isn't otherwise known until after GetExternalLoginInfoAsync() succeeds below.
        var providerName = string.IsNullOrEmpty(provider) ? "that provider" : provider;

        if (remoteError != null)
        {
            TempData["ExternalLoginError"] = $"Something went wrong signing in with {providerName}. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["ExternalLoginError"] = $"Something went wrong signing in with {providerName}. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        // Already linked to an account from a previous sign-in - just sign in.
        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (signInResult.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }
        if (signInResult.IsLockedOut)
        {
            TempData["ExternalLoginError"] = "This account has been locked out. Try again later.";
            return RedirectToAction(nameof(Login));
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            TempData["ExternalLoginError"] = $"{info.LoginProvider} didn't share an email address, so we can't sign you in.";
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByEmailAsync(email);
        var isNewUser = user is null;
        if (user is null)
        {
            // The external provider has already verified this email, unlike a local signup's address.
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ExternalLoginError"] = "Couldn't create your account. Please try again.";
                return RedirectToAction(nameof(Login));
            }
        }

        // Link this external identity to the (new or pre-existing local) account by email.
        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            TempData["ExternalLoginError"] = $"Couldn't link your {info.LoginProvider} account. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

        // SMS alerts need a phone number, which none of the external sign-in providers supply -
        // nudge new accounts to add one instead of silently leaving that half of the alert path dark.
        if (isNewUser && string.IsNullOrEmpty(user.PhoneNumber))
        {
            TempData["WelcomeNeedsPhone"] = "true";
            return RedirectToAction(nameof(Profile));
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var logins = await userManager.GetLoginsAsync(user);
        ViewBag.LinkedProviders = logins.Select(l => l.LoginProvider).ToList();
        ViewBag.HasPassword = await userManager.HasPasswordAsync(user);

        return View(new ProfileViewModel
        {
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber ?? ""
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        model.Email = user.Email!;

        if (!ModelState.IsValid)
        {
            var logins = await userManager.GetLoginsAsync(user);
            ViewBag.LinkedProviders = logins.Select(l => l.LoginProvider).ToList();
            ViewBag.HasPassword = await userManager.HasPasswordAsync(user);
            return View(model);
        }

        user.PhoneNumber = model.PhoneNumber;
        await userManager.UpdateAsync(user);

        TempData["ProfileSaved"] = "true";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var hasPassword = await userManager.HasPasswordAsync(user);

        if (hasPassword && string.IsNullOrEmpty(model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required.");
        }

        if (!ModelState.IsValid)
        {
            var logins = await userManager.GetLoginsAsync(user);
            ViewBag.LinkedProviders = logins.Select(l => l.LoginProvider).ToList();
            ViewBag.HasPassword = hasPassword;
            ViewBag.ChangePasswordModel = model;
            return View(nameof(Profile), new ProfileViewModel
            {
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber ?? ""
            });
        }

        // A Google-only account (no password yet) uses AddPasswordAsync - there's no current
        // password to verify. Everyone else goes through ChangePasswordAsync, which does.
        var result = hasPassword
            ? await userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword)
            : await userManager.AddPasswordAsync(user, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            var logins = await userManager.GetLoginsAsync(user);
            ViewBag.LinkedProviders = logins.Select(l => l.LoginProvider).ToList();
            ViewBag.HasPassword = hasPassword;
            ViewBag.ChangePasswordModel = model;
            return View(nameof(Profile), new ProfileViewModel
            {
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber ?? ""
            });
        }

        // ChangePasswordAsync/AddPasswordAsync bump the security stamp - refresh the current
        // cookie now so this session isn't silently signed out on the next stamp check.
        await signInManager.RefreshSignInAsync(user);

        TempData["PasswordChanged"] = hasPassword ? "changed" : "set";
        return RedirectToAction(nameof(Profile));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Automations");
    }

    // A failed send here shouldn't block signup/login itself - the account still works, the
    // owner just won't be verified yet. They can hit "Resend" from their account page. Logged
    // so a run of these is actually visible somewhere, per TRD's fail-visibly-not-silently rule.
    private async Task SendConfirmationEmailAsync(ApplicationUser user)
    {
        try
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmUrl = Url.Action(nameof(ConfirmEmail), "Account",
                new { userId = user.Id, token = encodedToken }, Request.Scheme)!;

            await emailSender.SendAsync(
                user.Email!,
                "Confirm your ZapWatch email",
                $"<p>One click to confirm <strong>{user.Email}</strong> for ZapWatch alerts:</p>"
                + $"<p><a href=\"{confirmUrl}\">Confirm my email</a></p>"
                + "<p>If you didn't sign up for ZapWatch, you can ignore this email.</p>");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send confirmation email to {Email}", user.Email);
        }
    }
}
