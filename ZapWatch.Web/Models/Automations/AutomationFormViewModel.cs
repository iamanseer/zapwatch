using System.ComponentModel.DataAnnotations;

namespace ZapWatch.Web.Models.Automations;

public class AutomationFormViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Automation name")]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, 10080, ErrorMessage = "Enter a value between 1 minute and 7 days (10080 minutes).")]
    [Display(Name = "Expected frequency (minutes)")]
    public int ExpectedFrequencyMinutes { get; set; } = 60;

    [Required]
    [Range(0, 10080, ErrorMessage = "Enter a value between 0 and 7 days (10080 minutes).")]
    [Display(Name = "Grace period (minutes)")]
    public int GracePeriodMinutes { get; set; } = 15;
}
