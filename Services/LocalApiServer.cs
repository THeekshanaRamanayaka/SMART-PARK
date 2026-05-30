using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartPark.Models;

namespace SmartPark.Services;

public sealed class LocalApiServer : IAsyncDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LocalApiServer> _logger;
    private readonly HttpListener _listener = new();
    private readonly string _prefix;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _listenerTask;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public LocalApiServer(IServiceScopeFactory scopeFactory, ILogger<LocalApiServer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _prefix = Environment.GetEnvironmentVariable("SMARTPARK_LOCAL_API_URL") ?? "http://127.0.0.1:5099/";
    }

    public string BaseUrl => _prefix;

    public Task StartAsync()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("SMARTPARK_ENABLE_LOCAL_API"), "false", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Local API server is disabled by SMARTPARK_ENABLE_LOCAL_API=false");
            return Task.CompletedTask;
        }

        if (_listenerTask != null)
        {
            return Task.CompletedTask;
        }

        try
        {
            _listener.Prefixes.Add(_prefix);
            _listener.Start();
            _cancellationTokenSource = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenAsync(_cancellationTokenSource.Token));
            _logger.LogInformation("Local API server started at {BaseUrl}", _prefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to start local API server at {BaseUrl}", _prefix);
        }

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _listener.Close();

            if (_listenerTask != null)
            {
                await _listenerTask.ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Local API server shutdown encountered a non-fatal issue");
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext? context = null;

            try
            {
                context = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local API listener error");
                continue;
            }

            if (context != null)
            {
                _ = Task.Run(() => HandleRequestAsync(context), CancellationToken.None);
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var method = request.HttpMethod.ToUpperInvariant();
        var path = request.Url?.AbsolutePath.TrimEnd('/') ?? "/";

        _logger.LogInformation("API {Method} {Path}", method, path);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var slotService = scope.ServiceProvider.GetRequiredService<ParkingSlotService>();
            var recordService = scope.ServiceProvider.GetRequiredService<ParkingRecordService>();
            var authService = scope.ServiceProvider.GetRequiredService<AuthService>();

            if (method == "GET" && path == "/health")
            {
                await WriteJsonAsync(response, new
                {
                    status = "ok",
                    service = "SmartPark",
                    localApi = true,
                    baseUrl = _prefix
                }).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path == "/api/auth/login")
            {
                var login = await ReadJsonBodyAsync<LoginRequest>(request).ConfigureAwait(false);
                if (login == null || string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, "username and password are required").ConfigureAwait(false);
                    return;
                }

                var (user, accessToken, refreshToken) = await authService.AuthenticateAsync(login.Username.Trim(), login.Password).ConfigureAwait(false);
                if (user == null || accessToken == null)
                {
                    await WriteErrorAsync(response, HttpStatusCode.Unauthorized, "Invalid credentials").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, new
                {
                    accessToken,
                    refreshToken,
                    user = new { id = user.Id, username = user.Username, fullName = user.FullName, email = user.Email, role = user.Role }
                }, HttpStatusCode.OK).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path == "/api/auth/refresh")
            {
                var body = await ReadJsonBodyAsync<RefreshRequest>(request).ConfigureAwait(false);
                if (body == null || string.IsNullOrWhiteSpace(body.RefreshToken))
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, "refreshToken is required").ConfigureAwait(false);
                    return;
                }

                var (user, accessToken, refreshToken) = await authService.RefreshAsync(body.RefreshToken).ConfigureAwait(false);
                if (user == null || accessToken == null)
                {
                    await WriteErrorAsync(response, HttpStatusCode.Unauthorized, "Invalid refresh token").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, new { accessToken, refreshToken }).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path == "/api/auth/logout")
            {
                var body = await ReadJsonBodyAsync<RefreshRequest>(request).ConfigureAwait(false);
                if (body != null && !string.IsNullOrWhiteSpace(body.RefreshToken))
                {
                    await authService.RevokeRefreshTokenAsync(body.RefreshToken).ConfigureAwait(false);
                }

                await WriteJsonAsync(response, new { success = true }).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/slots")
            {
                await WriteJsonAsync(response, await slotService.GetAllSlotsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/slots/available-count")
            {
                await WriteJsonAsync(response, new { available = await slotService.GetAvailableSlotsCountAsync().ConfigureAwait(false) }).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/slots/occupied-count")
            {
                await WriteJsonAsync(response, new { occupied = await slotService.GetOccupiedSlotsCountAsync().ConfigureAwait(false) }).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path == "/api/slots")
            {
                if (await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false) == null) return;

                var slotRequest = await ReadJsonBodyAsync<SlotRequest>(request).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(slotRequest?.SlotNumber))
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, "slotNumber is required").ConfigureAwait(false);
                    return;
                }

                var slot = new ParkingSlot
                {
                    SlotNumber = slotRequest.SlotNumber.Trim().ToUpperInvariant(),
                    IsOccupied = slotRequest.IsOccupied ?? false
                };

                var success = await slotService.AddSlotAsync(slot).ConfigureAwait(false);
                await WriteJsonAsync(response, new { success, slot }, success ? HttpStatusCode.Created : HttpStatusCode.InternalServerError).ConfigureAwait(false);
                return;
            }

            if (method == "PUT" && TryGetTrailingId(path, "/api/slots", out var slotId))
            {
                if (await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false) == null) return;

                var slotRequest = await ReadJsonBodyAsync<SlotRequest>(request).ConfigureAwait(false);
                var slot = await slotService.GetSlotByIdAsync(slotId).ConfigureAwait(false);

                if (slot == null)
                {
                    await WriteErrorAsync(response, HttpStatusCode.NotFound, $"Slot {slotId} was not found").ConfigureAwait(false);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(slotRequest?.SlotNumber))
                {
                    slot.SlotNumber = slotRequest.SlotNumber.Trim().ToUpperInvariant();
                }

                if (slotRequest?.IsOccupied is not null)
                {
                    slot.IsOccupied = slotRequest.IsOccupied.Value;
                }

                var success = await slotService.UpdateSlotAsync(slot).ConfigureAwait(false);
                await WriteJsonAsync(response, new { success, slot }, success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError).ConfigureAwait(false);
                return;
            }

            if (method == "DELETE" && TryGetTrailingId(path, "/api/slots", out slotId))
            {
                if (await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false) == null) return;

                var success = await slotService.DeleteSlotAsync(slotId).ConfigureAwait(false);
                if (!success)
                {
                    await WriteErrorAsync(response, HttpStatusCode.NotFound, $"Slot {slotId} was not found").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, new { success = true }).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/records")
            {
                await WriteJsonAsync(response, await recordService.GetAllRecordsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/records/active")
            {
                await WriteJsonAsync(response, await recordService.GetActiveRecordsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path == "/api/records/completed")
            {
                await WriteJsonAsync(response, await recordService.GetCompletedRecordsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && path.StartsWith("/api/records/search", StringComparison.OrdinalIgnoreCase))
            {
                var term = GetQueryParameter(request.Url, "term");
                if (string.IsNullOrWhiteSpace(term))
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, "term query parameter is required").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, await recordService.SearchRecordsAsync(term).ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && TryGetTrailingText(path, "/api/records/history", out var vehicleNumber))
            {
                await WriteJsonAsync(response, await recordService.GetVehicleHistoryAsync(vehicleNumber).ConfigureAwait(false)).ConfigureAwait(false);
                return;
            }

            if (method == "GET" && TryGetTrailingId(path, "/api/records", out var recordId))
            {
                var record = await recordService.GetRecordByIdAsync(recordId).ConfigureAwait(false);
                if (record == null)
                {
                    await WriteErrorAsync(response, HttpStatusCode.NotFound, $"Record {recordId} was not found").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, record).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path == "/api/records/entry")
            {
                if (await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false) == null) return;

                var entryRequest = await ReadJsonBodyAsync<EntryRequest>(request).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(entryRequest?.VehicleNumber) || string.IsNullOrWhiteSpace(entryRequest.OwnerName))
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, "vehicleNumber and ownerName are required").ConfigureAwait(false);
                    return;
                }

                var success = await recordService.RecordVehicleEntryAsync(entryRequest.VehicleNumber, entryRequest.OwnerName).ConfigureAwait(false);
                await WriteJsonAsync(response, new { success }, success ? HttpStatusCode.Created : HttpStatusCode.BadRequest).ConfigureAwait(false);
                return;
            }

            if (method == "POST" && path.EndsWith("/exit", StringComparison.OrdinalIgnoreCase) && TryGetTrailingId(path, "/api/records", out recordId))
            {
                if (await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false) == null) return;

                var success = await recordService.RecordVehicleExitAsync(recordId).ConfigureAwait(false);
                if (!success)
                {
                    await WriteErrorAsync(response, HttpStatusCode.BadRequest, $"Unable to record exit for record {recordId}").ConfigureAwait(false);
                    return;
                }

                await WriteJsonAsync(response, new { success = true }).ConfigureAwait(false);
                return;
            }

            await WriteErrorAsync(response, HttpStatusCode.NotFound, $"No API endpoint matched {method} {path}").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while processing {Method} {Path}", method, path);
            await WriteErrorAsync(response, HttpStatusCode.InternalServerError, "Unexpected server error").ConfigureAwait(false);
        }
        finally
        {
            response.Close();
        }
    }

    private static async Task<T?> ReadJsonBodyAsync<T>(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return default;
        }

        return await JsonSerializer.DeserializeAsync<T>(request.InputStream, JsonOptions).ConfigureAwait(false);
    }

    private static async Task WriteJsonAsync(HttpListenerResponse response, object payload, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var buffer = Encoding.UTF8.GetBytes(json);

        response.StatusCode = (int)statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer).ConfigureAwait(false);
    }

    private static Task WriteErrorAsync(HttpListenerResponse response, HttpStatusCode statusCode, string message)
    {
        return WriteJsonAsync(response, new { error = message }, statusCode);
    }

    private static bool TryGetTrailingId(string path, string prefix, out int id)
    {
        id = 0;

        if (!path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = path[(prefix.Length + 1)..];
        var slashIndex = remainder.IndexOf('/');
        var idSegment = slashIndex >= 0 ? remainder[..slashIndex] : remainder;

        return int.TryParse(idSegment, out id);
    }

    private static bool TryGetTrailingText(string path, string prefix, out string value)
    {
        value = string.Empty;

        if (!path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = path[(prefix.Length + 1)..];
        if (string.IsNullOrWhiteSpace(remainder))
        {
            return false;
        }

        value = Uri.UnescapeDataString(remainder);
        return true;
    }

    private static string? GetQueryParameter(Uri? uri, string key)
    {
        if (uri == null || string.IsNullOrWhiteSpace(uri.Query))
        {
            return null;
        }

        var query = uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var pair in query)
        {
            var parts = pair.Split('=', 2);
            if (!string.Equals(Uri.UnescapeDataString(parts[0]), key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        return null;
    }

    // Authorization helpers
    private static string ExtractBearerToken(HttpListenerRequest request)
    {
        var authHeader = request.Headers["Authorization"] ?? string.Empty;
        var parts = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? parts[1] : string.Empty;
    }

    private (System.Security.Claims.ClaimsPrincipal? principal, string? error) ValidateTokenFromRequest(HttpListenerRequest request, AuthService authService)
    {
        var token = ExtractBearerToken(request);
        return authService.ValidateJwtToken(token);
    }

    private async Task<System.Security.Claims.ClaimsPrincipal?> RequireAuthenticationAsync(HttpListenerRequest request, HttpListenerResponse response, AuthService authService)
    {
        var (principal, err) = ValidateTokenFromRequest(request, authService);
        if (principal == null)
        {
            await WriteErrorAsync(response, HttpStatusCode.Unauthorized, "Authorization required").ConfigureAwait(false);
            return null;
        }

        return principal;
    }

    private async Task<System.Security.Claims.ClaimsPrincipal?> RequireRoleAsync(HttpListenerRequest request, HttpListenerResponse response, AuthService authService, string role)
    {
        var principal = await RequireAuthenticationAsync(request, response, authService).ConfigureAwait(false);
        if (principal == null) return null;

        if (!principal.IsInRole(role))
        {
            await WriteErrorAsync(response, HttpStatusCode.Forbidden, $"{role} role required").ConfigureAwait(false);
            return null;
        }

        return principal;
    }

    private sealed class SlotRequest
    {
        public string? SlotNumber { get; set; }
        public bool? IsOccupied { get; set; }
    }

    private sealed class EntryRequest
    {
        public string? VehicleNumber { get; set; }
        public string? OwnerName { get; set; }
    }

    private sealed class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    private sealed class RefreshRequest
    {
        public string? RefreshToken { get; set; }
    }
}