using Microsoft.AspNetCore.Mvc;

namespace Presentation.Extensions;

public static class ModelValidationExtensions
{
    public static IServiceCollection ConfigureModelValidationResponse(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
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

        return services;
    }
}
