using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ZapWatch.Web.Models.Account;

namespace ZapWatch.Tests;

// Regression coverage for the "The Email field is required" bug reported 2026-08-03: the
// Profile page's Email <input> is always disabled (never submits), so every Profile POST binds
// Email to null - ASP.NET Core's implicit-required check for non-nullable reference types then
// failed validation, even though the controller always overwrites Email from the real user
// record before saving and never actually needed it as submitted input.
public class ProfileViewModelValidationTests
{
    private static IObjectModelValidator CreateValidator(out ActionContext actionContext)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllersWithViews();
        var provider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = provider };
        actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());

        return provider.GetRequiredService<IObjectModelValidator>();
    }

    [Fact]
    public void Empty_Email_from_the_always_disabled_input_does_not_fail_validation()
    {
        var validator = CreateValidator(out var actionContext);
        var model = new ProfileViewModel { Email = null!, PhoneNumber = "+15551234567" };

        validator.Validate(actionContext, validationState: null, prefix: "", model);

        Assert.True(actionContext.ModelState.IsValid);
    }

    [Fact]
    public void Missing_PhoneNumber_still_fails_validation()
    {
        // Confirms [ValidateNever] on Email didn't accidentally suppress validation for the rest
        // of the model - PhoneNumber's own [Required] must still be enforced.
        var validator = CreateValidator(out var actionContext);
        var model = new ProfileViewModel { Email = "user@example.com", PhoneNumber = "" };

        validator.Validate(actionContext, validationState: null, prefix: "", model);

        Assert.False(actionContext.ModelState.IsValid);
        Assert.True(actionContext.ModelState.ContainsKey(nameof(ProfileViewModel.PhoneNumber)));
    }
}
