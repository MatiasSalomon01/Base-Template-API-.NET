using Application;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
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

        services.Configure<JsonOptions>(x => x.JsonSerializerOptions.Converters.Add(new UtcToLocalDateTimeConverter()));

        return services;
    }
}
