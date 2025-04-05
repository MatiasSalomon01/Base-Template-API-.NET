using Intrastructure.Abstractions.Interfaces.Pagination;

namespace Intrastructure.Abstractions.Models.Pagination;

public class PaginatedResult<T> : IPaginated
{
    public int TotalCount { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<T> Items { get; set; } = [];

    private PaginatedResult() { }

    public PaginatedResult(int totalCount, bool hasNext, bool hasPrevious, int pageNumber, int pageSize, List<T> items)
    {
        TotalCount = totalCount;
        HasNext = hasNext;
        HasPrevious = hasPrevious;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Items = items;
    }
}