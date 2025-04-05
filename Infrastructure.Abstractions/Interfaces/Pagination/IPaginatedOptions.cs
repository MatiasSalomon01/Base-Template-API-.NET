using Intrastructure.Abstractions.Models.Search;

namespace Intrastructure.Abstractions.Interfaces.Pagination;
public interface IPaginatedOptions : IPaginated
{
    public Dictionary<string, string>? Filters { get; set; }
    public GeneralSearchOptions? Search { get; set; }
    public string? SortBy { get; set; }
    public string? Direction { get; set; }
}
