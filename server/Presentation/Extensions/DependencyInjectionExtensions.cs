using Application.Interfaces;
using Application.Services;
using Core.Interfaces;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;

namespace Presentation.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<TaskRepository>();
        services.AddScoped<ITaskRepository, CachedTaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<TaskService>();
        services.AddScoped<UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProdiver>();
        services.AddHttpContextAccessor();
        return services;
    }
}
