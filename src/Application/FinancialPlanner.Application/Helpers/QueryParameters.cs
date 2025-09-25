using Microsoft.AspNetCore.Mvc;

namespace FinancialPlanner.Application.Helpers;

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
    public string? SortBy { get; set; } = null; // e.g., "Date", "Amount"

    [FromQuery(Name = "sortOrder")]
    public string SortOrder { get; set; } = "asc"; // "asc" or "desc"

    [FromQuery(Name = "globalSearch")]
    public string? GlobalSearch { get; set; } // A single term to search across multiple fields

    [FromQuery(Name = "filters")]
    public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();
}