using Application;
using Infrastructure;
using WebApi.Configuration.ExceptionHandlers;
using WebApi.Configuration.Swagger;
using WebApi.Filters;

namespace WebApi.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication(configuration)
            .AddInfrastructure(configuration)
            .AddEndpointsApiExplorer()
            .AddSwagger()
            .AddExceptionHandler<ExceptionHandler>()
            .AddProblemDetails()
            .AddControllers(x => x.Filters.Add(typeof(PaginatedOptionsActionFilter)));

        return services;
    }
}
