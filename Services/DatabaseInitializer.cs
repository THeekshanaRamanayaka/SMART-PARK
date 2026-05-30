using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartPark.Data;

namespace SmartPark.Services;

public class DatabaseInitializer
{
    private readonly SmartParkDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly AuthService _authService;

    public DatabaseInitializer(SmartParkDbContext context, ILogger<DatabaseInitializer> logger, AuthService authService)
    {
        _context = context;
        _logger = logger;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Ensuring SmartPark database schema exists");

        // No EF migrations exist — schema is managed via raw SQL.
        // Create any missing tables using IF NOT EXISTS so this is safe to run repeatedly.
        await _context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS AppUsers (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(50) NOT NULL,
                FullName VARCHAR(100) NOT NULL,
                Email VARCHAR(100) NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role VARCHAR(20) NOT NULL DEFAULT 'User',
                IsActive TINYINT(1) NOT NULL DEFAULT 1,
                CreatedAt DATETIME(6) NOT NULL,
                UNIQUE INDEX IX_AppUsers_Username (Username),
                UNIQUE INDEX IX_AppUsers_Email (Email)
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
        ");

        await _context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS RefreshTokens (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                AppUserId INT NOT NULL,
                Token VARCHAR(200) NOT NULL,
                ExpiresAt DATETIME(6) NOT NULL,
                CreatedAt DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
                Revoked TINYINT(1) NOT NULL DEFAULT 0,
                CONSTRAINT FK_RefreshTokens_AppUsers FOREIGN KEY (AppUserId)
                    REFERENCES AppUsers (Id) ON DELETE CASCADE
            ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
        ");

        // Seed default admin user for initial setup if no users exist.
        if (!_context.AppUsers.Any())
        {
            _logger.LogInformation("Seeding default admin user");
            var adminUsername = Environment.GetEnvironmentVariable("SMARTPARK_ADMIN_USERNAME") ?? "admin";
            var adminEmail = Environment.GetEnvironmentVariable("SMARTPARK_ADMIN_EMAIL") ?? "admin@localhost";
            var adminPassword = Environment.GetEnvironmentVariable("SMARTPARK_ADMIN_PASSWORD") ?? "smartpark123";

            var admin = new Models.AppUser
            {
                Username = adminUsername,
                FullName = "Administrator",
                Email = adminEmail,
                PasswordHash = _authService.HashPassword(adminPassword),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(admin);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Default admin created: {Username}", adminUsername);
        }

        _logger.LogInformation("Database schema is ready");
    }
}
