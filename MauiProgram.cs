using Microsoft.Extensions.Logging;
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

		// Initialize the database after startup so the first scene can render immediately.
		_ = Task.Run(async () =>
		{
			try
			{
				using var scope = app.Services.CreateScope();
				var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
				await dbInitializer.InitializeAsync();

				var localApiServer = scope.ServiceProvider.GetRequiredService<LocalApiServer>();
				await localApiServer.StartAsync();
			}
			catch (Exception ex)
			{
				var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
				logger.LogError(ex, "Application startup failed");
			}
		});

		var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
		startupLogger.LogInformation("SmartPark application created successfully");

		return app;
	}
}
