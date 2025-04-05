using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using WebApi.Common;
using WebApi.Configuration;
using WebApi.Configuration.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var config = builder.Configuration;

builder.Services.AddWebApi(config);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(x => x.ConfigureTemplate(Constants.ProjectName));
    app.UseSwaggerUI(x => x.ConfigureUI(Constants.ProjectName));
    app.LogSwaggerLocation();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
