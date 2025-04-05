using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebApi.Common;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace WebApi.Configuration.Swagger;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = Constants.ProjectName,
                Description = Constants.ProjectName,
            });
            c.DescribeAllParametersInCamelCase();
            c.CustomSchemaIds(t => t.Name);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter [space] and then your token in the text input below.",
                Name = "Authorization",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
              {
                {
                  new OpenApiSecurityScheme
                  {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme,
                    }
                  },
                  Array.Empty<string>()
                }
            });

            //c.IncludeXmlComments(Path.ChangeExtension(typeof(Program).Assembly.Location, ".xml"));
        });

        return services;
    }

    public static SwaggerOptions ConfigureTemplate(this SwaggerOptions options, string prefix)
    {
        options.RouteTemplate = $"/{BuildPrefix(prefix)}/{{documentName}}/swagger.json";
        return options;
    }

    public static SwaggerUIOptions ConfigureUI(this SwaggerUIOptions options, string prefix)
    {
        options.SwaggerEndpoint($"/{BuildPrefix(prefix)}/v1/swagger.json", "");
        options.RoutePrefix = BuildPrefix(prefix);
        return options;
    }

    public static string BuildPrefix(string prefix) => $"swagger-{prefix}";

    public static void LogSwaggerLocation(this WebApplication app)
    {
        Task.Run(async () =>
        {
            await Task.Delay(500);
            
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();

            logger.LogInformation("Swagger disponible en: {address}/{swaggerName}", address, BuildPrefix(Constants.ProjectName));
        });
    }
}
