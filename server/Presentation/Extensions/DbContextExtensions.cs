using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Presentation.Extensions;

public static class DbContextExtensions
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db";
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        return services;
    }
}
