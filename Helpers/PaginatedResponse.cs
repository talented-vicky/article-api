namespace ArticleApi.Helpers;

public class PaginatedResponse<T>
{
    public bool Status { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T>? Data { get; set; }

    public PaginatedResponse(){}

    public PaginatedResponse(bool status, int total, int page, int pageSize, IEnumerable<T> data = default)
    {
        Status = status;
        TotalItems = total;
        Data = data;
        Page = page;
        PageSize = pageSize;
    }
}