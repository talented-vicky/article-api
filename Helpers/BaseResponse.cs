namespace ArticleApi.Helpers;

public class BaseResponse<T>
{
    public bool Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public BaseResponse(){}

    public BaseResponse(bool status, string? msg = null, T? data = default)
    {
        Status = status;
        Message = msg;
        Data = data;
    }
}