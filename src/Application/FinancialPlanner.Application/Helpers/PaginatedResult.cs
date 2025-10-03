namespace FinancialPlanner.Application;

public class PaginatedResult<T>
{
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<T> Data { get; set; } = new List<T>();

    public PaginatedResult(List<T> items, int totalRecords, int pageNumber, int pageSize)
    {
        Data = items;
        TotalRecords = totalRecords;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
