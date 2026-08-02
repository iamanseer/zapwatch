using System.ComponentModel.DataAnnotations;

namespace ZapWatch.Web.Models.Account;

public class ProfileViewModel
{
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [Display(Name = "Phone number (for SMS alerts)")]
    public string PhoneNumber { get; set; } = null!;
}
