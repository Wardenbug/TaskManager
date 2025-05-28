using Application.Interfaces;
using Application.Services;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Entities;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presentation.Middlewares;
using System.Text;
using Serilog;
using System.Threading.RateLimiting;
using Presentation.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity.Name ?? "anonymous",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    QueueLimit = 2,
                    Window = TimeSpan.FromMinutes(1)
                });
            }

            return RateLimitPartition.GetFixedWindowLimiter(
               partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
               factory: partition => new FixedWindowRateLimiterOptions
               {
                   AutoReplenishment = true,
                   PermitLimit = 10,
                   QueueLimit = 0,
                   Window = TimeSpan.FromMinutes(1)
               });
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/json";

            var response = new ErrorResponseDto(
                "Too many requests. Please try again later.",
                "RATE_LIMIT_EXCEEDED"
            );

            await context.HttpContext.Response.WriteAsJsonAsync(response, token);
        };
    });

    builder.Services.AddOpenApi();
    builder.Services.AddControllers();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
                );

            return new BadRequestObjectResult(new
            {
                Message = "Validation Failed",
                Errors = errors
            });
        };
    });

    builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        };
    });
    builder.Services.AddSerilog();

    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<TaskService>();
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProdiver>();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAutoMapper(typeof(Presentation.AssemblyReference).Assembly);

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=tasks.db"));

    builder.Services.AddIdentityCore<ApplicationUser>(options => { })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

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

    app.MapControllers();

    app.MapGet("/", () => "Hello World!");

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