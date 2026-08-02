using System.ComponentModel.DataAnnotations;

namespace ZapWatch.Web.Models.Account;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
}
