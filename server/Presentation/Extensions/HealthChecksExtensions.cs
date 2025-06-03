namespace Presentation.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddHealthCheckWithUI(this IServiceCollection services, string connectionString)
    {
        services.AddHealthChecks()
        .AddSqlite(connectionString, name: "SqlLite");

        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(10);
            options.AddHealthCheckEndpoint("SQLite", "/health");
        })
        .AddSqliteStorage(connectionString);

        return services;
    }
}
