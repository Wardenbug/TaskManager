using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ApplicationException("Database connection string 'DefaultConnection' is missing or empty in the configuration.");
        }
        services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString)
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging()
        );
        return services;
    }
}
