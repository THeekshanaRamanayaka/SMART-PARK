using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPark.Data;

namespace SmartPark.Services;

public class DatabaseInitializer
{
    private readonly SmartParkDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(SmartParkDbContext context, ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Ensuring SmartPark database schema exists");

        // Create schema directly for MySQL on first run.
        // Use EF Core migrations in production; migrate the database to the latest
        // version on startup so schema changes are applied predictably.
        await _context.Database.MigrateAsync();

        _logger.LogInformation("Database schema is ready");
    }
}
