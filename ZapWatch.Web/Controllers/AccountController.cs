using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZapWatch.Web.Models;
using ZapWatch.Web.Models.Account;

namespace ZapWatch.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
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
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            TempData["ExternalLoginError"] = "Something went wrong signing in with Google. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            TempData["ExternalLoginError"] = "Something went wrong signing in with Google. Please try again.";
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
            TempData["ExternalLoginError"] = "Google didn't share an email address, so we can't sign you in.";
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByEmailAsync(email);
        var isNewUser = user is null;
        if (user is null)
        {
            // Google has already verified this email, unlike a local signup's address.
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ExternalLoginError"] = "Couldn't create your account. Please try again.";
                return RedirectToAction(nameof(Login));
            }
        }

        // Link this Google identity to the (new or pre-existing local) account by email.
        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            TempData["ExternalLoginError"] = "Couldn't link your Google account. Please try again.";
            return RedirectToAction(nameof(Login));
        }

        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

        // SMS alerts need a phone number, which Google sign-in never supplies - nudge new
        // accounts to add one instead of silently leaving that half of the alert path dark.
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
        ViewBag.IsGoogleLinked = logins.Any(l => l.LoginProvider == "Google");

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
            ViewBag.IsGoogleLinked = logins.Any(l => l.LoginProvider == "Google");
            return View(model);
        }

        user.PhoneNumber = model.PhoneNumber;
        await userManager.UpdateAsync(user);

        TempData["ProfileSaved"] = "true";
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
}
