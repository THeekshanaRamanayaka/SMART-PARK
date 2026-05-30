using System.Net;
using System.Security.Claims;
using SmartPark.Services;

namespace SmartPark.Services;

public static class LocalApiAuthHelper
{
    public static string ExtractBearerToken(HttpListenerRequest request)
    {
        var authHeader = request.Headers["Authorization"] ?? string.Empty;
        var parts = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? parts[1] : string.Empty;
    }

    public static (ClaimsPrincipal? principal, string? error) ValidateTokenFromRequest(HttpListenerRequest request, AuthService authService)
    {
        var token = ExtractBearerToken(request);
        return authService.ValidateJwtToken(token);
    }

    public static bool TryRequireAuthentication(HttpListenerRequest request, AuthService authService, out ClaimsPrincipal? principal, out string? error)
    {
        (principal, error) = ValidateTokenFromRequest(request, authService);
        return principal != null;
    }

    public static bool TryRequireRole(HttpListenerRequest request, AuthService authService, string role, out ClaimsPrincipal? principal, out string? error)
    {
        if (!TryRequireAuthentication(request, authService, out principal, out error))
        {
            return false;
        }

        if (!principal!.IsInRole(role))
        {
            error = $"{role} role required";
            principal = null;
            return false;
        }

        return true;
    }
}
