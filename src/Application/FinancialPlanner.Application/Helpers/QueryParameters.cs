using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Application;

public class QueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; set; } = null;

    [FromQuery(Name = "sortOrder")]
    public string SortOrder { get; set; } = "asc";

    [FromQuery(Name = "globalSearch")]
    public string? GlobalSearch { get; set; }

    [FromQuery(Name = "filters")]
    public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();
}
