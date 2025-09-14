namespace BTSBackoffice.Web.Models.Configuration;

public class AuthenticationSettings
{
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
}