namespace Presentation.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddHealthCheckWithUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
        .AddNpgSql(configuration.GetConnectionString("DefaultConnection"), name: "sql");

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(10);
            options.AddHealthCheckEndpoint("sql", "http://presentation:5000/health");
        })
        .AddInMemoryStorage();

        return services;
    }
}
