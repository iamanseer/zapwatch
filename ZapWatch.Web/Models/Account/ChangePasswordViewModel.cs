using System.ComponentModel.DataAnnotations;

namespace ZapWatch.Web.Models.Account;

public class ChangePasswordViewModel
{
    // Not [Required] here - only required when the account already has a password, which
    // AccountController checks server-side via UserManager.HasPasswordAsync. A Google-only
    // account setting its first password has no current one to enter.
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string? CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
