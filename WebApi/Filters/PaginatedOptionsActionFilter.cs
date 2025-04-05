using Intrastructure.Abstractions.Models.Pagination;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.Extensions;

namespace WebApi.Filters;

public class PaginatedOptionsActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var options = context.ActionArguments.FirstOrDefault(x => x.Value is PaginatedOptions);

        if (options.Value is not PaginatedOptions query) return;

        query.Filters = context.HttpContext.Request.GetFilters();
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
