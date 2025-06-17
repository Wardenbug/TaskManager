using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Presentation.Middlewares;
using System.Text;
using Serilog;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Presentation.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

    builder.Services.AddRateLimitingPolicies();
    builder.Services.AddOpenApi();
    builder.Services.AddControllers();
    builder.Services.ConfigureModelValidationResponse();
    builder.Services.AddAuthenticationWithJwt(builder.Configuration);
    builder.Services.AddSerilog();
    builder.Services.RegisterApplicationServices();
    builder.Services.AddHealthCheckWithUI(builder.Configuration);
    builder.Services.AddAutoMapper(typeof(Presentation.AssemblyReference).Assembly);
    builder.Services.AddApplicationDbContext(builder.Configuration);
    builder.Services.AddIdentityCore<ApplicationUser>(options => { })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
    builder.Services.AddStackExchangeRedisCache((redisOptions) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis");
        redisOptions.Configuration = connectionString;
    });

    var app = builder.Build();

    app.UseRateLimiter();
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseCors();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapHealthChecks(
    "/health",
        new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    app.MapHealthChecksUI();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}