namespace SmartPark.Services;

// Preferences (NSUserDefaults) is used instead of SecureStorage (Keychain) because
// Mac Catalyst dev builds lack the keychain-access-groups entitlement needed for SecureStorage.
public class AuthenticationService
{
    private const string AccessTokenKey = "smartpark_access_token";
    private const string RefreshTokenKey = "smartpark_refresh_token";

    private readonly AuthService _authService;

    public event Action? AuthenticationStateChanged;

    public AuthenticationService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<(bool ok, string? error)> LoginAsync(string username, string password)
    {
        try
        {
            var (user, accessToken, refreshToken) = await _authService.AuthenticateAsync(username, password).ConfigureAwait(false);
            if (user == null || accessToken == null)
                return (false, "Invalid credentials. Please try again.");

            if (!string.IsNullOrWhiteSpace(accessToken))
                Preferences.Default.Set(AccessTokenKey, accessToken);

            if (!string.IsNullOrWhiteSpace(refreshToken))
                Preferences.Default.Set(RefreshTokenKey, refreshToken);

            AuthenticationStateChanged?.Invoke();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Login failed: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refresh = Preferences.Default.Get(RefreshTokenKey, (string?)null);
            if (!string.IsNullOrWhiteSpace(refresh))
                await _authService.RevokeRefreshTokenAsync(refresh).ConfigureAwait(false);
        }
        catch { }

        Preferences.Default.Remove(AccessTokenKey);
        Preferences.Default.Remove(RefreshTokenKey);

        AuthenticationStateChanged?.Invoke();
    }

    public Task<string?> GetAccessTokenAsync()
    {
        return Task.FromResult(Preferences.Default.Get(AccessTokenKey, (string?)null));
    }

    public async Task<bool> TryRefreshAsync()
    {
        try
        {
            var refresh = Preferences.Default.Get(RefreshTokenKey, (string?)null);
            if (string.IsNullOrWhiteSpace(refresh)) return false;

            var (user, accessToken, refreshToken) = await _authService.RefreshAsync(refresh).ConfigureAwait(false);
            if (user == null || accessToken == null) return false;

            if (!string.IsNullOrWhiteSpace(accessToken))
                Preferences.Default.Set(AccessTokenKey, accessToken);

            if (!string.IsNullOrWhiteSpace(refreshToken))
                Preferences.Default.Set(RefreshTokenKey, refreshToken);

            AuthenticationStateChanged?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
