using Intrastructure.Abstractions.Interfaces.Pagination;
using Intrastructure.Abstractions.Models.Search;

namespace Intrastructure.Abstractions.Models.Pagination;

public record PaginatedOptions : IPaginatedOptions
{
    public Dictionary<string, string>? Filters { get; set; } = [];
    public GeneralSearchOptions? Search { get; set; }
    public string? SortBy { get; set; }
    public string? Direction { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}