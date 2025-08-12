using Infrastructure.Context;
using Infrastructure.Context.Interceptors;
using Infrastructure.Queries;
using Infrastructure.Repositories;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration)
                .AddQueries()
                .AddRepositories()
                .AddMapper();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditTimeInterceptor>();

        services.AddDbContext<ApplicationDbContext>((provider, builder) =>
        {
            builder.AddInterceptors(provider.GetServices<ISaveChangesInterceptor>());
            //builder.UseSqlite(configuration.GetConnectionString("SQLite"));
            builder.UseNpgsql(configuration.GetConnectionString("PostgreSQL"));
        });

        services.AddScoped<DbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.Scan(x => x
            .FromAssemblies(typeof(ProductQuery).Assembly)
            .AddClasses(x => x.Where(x => x.Name.EndsWith("Query")), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.Scan(x => x
            .FromAssemblies(typeof(EntityRepository).Assembly)
            .AddClasses(x => x.Where(x => x.Name.EndsWith("Repository")), publicOnly: false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddMapper(this IServiceCollection services)
    {
        services.AddMapster();

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
