using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ZapWatch.Web.Models.Account;

public class ProfileViewModel
{
    // Never actually editable - the Profile form's Email <input> is always disabled, so it
    // never submits, and the controller always overwrites this from the real user record before
    // saving. Without [ValidateNever], ASP.NET Core's implicit-required check for non-nullable
    // reference types treats an empty submitted value as a validation failure ("The Email field
    // is required") even though this property was never meant to be user input in the first
    // place - and since that check runs during model binding, before the action body's own
    // `model.Email = user.Email!` line, that reassignment can't undo it.
    [ValidateNever]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [Display(Name = "Phone number (for SMS alerts)")]
    public string PhoneNumber { get; set; } = null!;
}
