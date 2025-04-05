using Microsoft.AspNetCore.Diagnostics;

namespace WebApi.Configuration.ExceptionHandlers;

public class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new
        {
            Status = StatusCodes.Status500InternalServerError,
            exception.Message,
            Type = exception.GetType().Name,
            Detail = exception.StackTrace
        };

        httpContext.Response.StatusCode = 500;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
