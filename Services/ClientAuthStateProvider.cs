using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SmartPark.Services;

public class ClientAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationService _authService;

    public ClientAuthStateProvider(AuthenticationService authService)
    {
        _authService = authService;
        _authService.AuthenticationStateChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged()
    {
        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(
            () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var access = await _authService.GetAccessTokenAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(access))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwt = null;
        try
        {
            jwt = handler.ReadJwtToken(access);
        }
        catch
        {
            // invalid token, try refresh
        }

        if (jwt == null || jwt.ValidTo <= DateTime.UtcNow)
        {
            var refreshed = await _authService.TryRefreshAsync().ConfigureAwait(false);
            if (!refreshed)
            {
                await _authService.LogoutAsync().ConfigureAwait(false);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            access = await _authService.GetAccessTokenAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(access))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            jwt = handler.ReadJwtToken(access);
        }

        var claims = jwt.Claims.Select(c => new Claim(c.Type, c.Value));
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }
}
