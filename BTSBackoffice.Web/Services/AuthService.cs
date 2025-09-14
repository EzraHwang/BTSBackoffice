using BTSBackoffice.Web.Models.Configuration;
using BTSBackoffice.Web.Models.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BTSBackoffice.Web.Services;

public interface IAuthService
{
    Task<bool> ValidateCredentialsAsync(LoginViewModel model);
    bool IsAccountLockedOut(string username);
    void RecordFailedAttempt(string username);
    void ClearFailedAttempts(string username);
    int GetRemainingAttempts(string username);
}

public class AuthService : IAuthService
{
    private readonly AuthenticationSettings _authSettings;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IOptions<AuthenticationSettings> authSettings, IMemoryCache cache, ILogger<AuthService> logger)
    {
        _authSettings = authSettings.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> ValidateCredentialsAsync(LoginViewModel model)
    {
        if (IsAccountLockedOut(model.Username))
        {
            _logger.LogWarning("Login attempt for locked account: {Username}", model.Username);
            return false;
        }

        // Simple credential validation (in production, use proper password hashing)
        var isValid = model.Username.Equals(_authSettings.AdminUsername, StringComparison.OrdinalIgnoreCase) &&
                     model.Password == _authSettings.AdminPassword;

        if (isValid)
        {
            ClearFailedAttempts(model.Username);
            _logger.LogInformation("Successful login for user: {Username}", model.Username);
        }
        else
        {
            RecordFailedAttempt(model.Username);
            _logger.LogWarning("Failed login attempt for user: {Username}", model.Username);
        }

        return await Task.FromResult(isValid);
    }

    public bool IsAccountLockedOut(string username)
    {
        var key = $"lockout_{username}";
        return _cache.TryGetValue(key, out _);
    }

    public void RecordFailedAttempt(string username)
    {
        var key = $"attempts_{username}";
        var attempts = _cache.TryGetValue(key, out int currentAttempts) ? currentAttempts : 0;
        attempts++;

        if (attempts >= _authSettings.MaxLoginAttempts)
        {
            // Lock account
            var lockoutKey = $"lockout_{username}";
            _cache.Set(lockoutKey, DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(_authSettings.LockoutDurationMinutes));
            _cache.Remove(key);
            _logger.LogWarning("Account locked for user: {Username} after {Attempts} failed attempts", username, attempts);
        }
        else
        {
            _cache.Set(key, attempts, TimeSpan.FromMinutes(_authSettings.LockoutDurationMinutes));
        }
    }

    public void ClearFailedAttempts(string username)
    {
        var key = $"attempts_{username}";
        _cache.Remove(key);
    }

    public int GetRemainingAttempts(string username)
    {
        var key = $"attempts_{username}";
        var attempts = _cache.TryGetValue(key, out int currentAttempts) ? currentAttempts : 0;
        return Math.Max(0, _authSettings.MaxLoginAttempts - attempts);
    }
}