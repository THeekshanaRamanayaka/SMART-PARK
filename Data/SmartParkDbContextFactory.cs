using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmartPark.Data;

public class SmartParkDbContextFactory : IDesignTimeDbContextFactory<SmartParkDbContext>
{
    public SmartParkDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SmartParkDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("SMARTPARK_MYSQL_CONNECTION")
            ?? "server=127.0.0.1;port=3307;database=smartpark;user=smartpark;password=smartpark;";

        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new SmartParkDbContext(optionsBuilder.Options);
    }
}
