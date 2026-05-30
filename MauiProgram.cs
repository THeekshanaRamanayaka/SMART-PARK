using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartPark.Data;
using SmartPark.Services;
using MudBlazor.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace SmartPark;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Register MainPage
        builder.Services.AddTransient<MainPage>();

        // Database
        var connectionString = Environment.GetEnvironmentVariable("SMARTPARK_MYSQL_CONNECTION")
            ?? "server=127.0.0.1;port=3307;database=smartpark;user=smartpark;password=smartpark;";
        builder.Services.AddDbContext<SmartParkDbContext>(options =>
            options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));

        // Services
        builder.Services.AddScoped<ParkingSlotService>();
        builder.Services.AddScoped<ParkingRecordService>();
        builder.Services.AddScoped<AuthService>();
        // Client-side authentication services
        builder.Services.AddScoped<AuthenticationService>();
        builder.Services.AddScoped<AuthenticationStateProvider, ClientAuthStateProvider>();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddTransient<DatabaseInitializer>();
        builder.Services.AddSingleton<LocalApiServer>();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

        var app = builder.Build();

        // Start the local API server immediately so the login endpoint is reachable
        // even if the database initializer hasn't finished yet.
        var localApiServer = app.Services.GetRequiredService<LocalApiServer>();
        _ = localApiServer.StartAsync();

        // Initialize the database in the background; failures are logged but do not
        // prevent the app or the API server from starting.
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                await dbInitializer.InitializeAsync();
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
                logger.LogError(ex, "Database initialization failed");
            }
        });

        var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        startupLogger.LogInformation("SmartPark application created successfully");

        return app;
    }
}
