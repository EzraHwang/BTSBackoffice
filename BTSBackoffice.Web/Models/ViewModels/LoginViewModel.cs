using System.ComponentModel.DataAnnotations;

namespace BTSBackoffice.Web.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "使用者名稱為必填")]
    [Display(Name = "使用者名稱")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼為必填")]
    [Display(Name = "密碼")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public bool IsLocked { get; set; }

    public int RemainingAttempts { get; set; }
}