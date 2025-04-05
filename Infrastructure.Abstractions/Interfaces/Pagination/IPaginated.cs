namespace Intrastructure.Abstractions.Interfaces.Pagination;

public interface IPaginated
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}