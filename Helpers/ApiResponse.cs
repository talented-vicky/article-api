using Microsoft.AspNetCore.Mvc;
// has helper methods; such as IActionResult (interface for all action results), OkObjectResult (http 200 OK with body), ObjectResult (custom http code with body), CreatedResult (http 201 with location header), NotFoundObjectResult (http 404 with body), NoContentResult (http 204 with no content)

namespace ArticleApi.Helpers;

public static class ApiResponse
{
    public static IActionResult Completed (bool status, string? msg = null) 
    {
        var resp = new BaseResponse<string>(status, msg, null);
        return new OkObjectResult(resp);
    }

    public static IActionResult Success<T> (T data, string? msg = null) 
    {
        var resp = new BaseResponse<T>(true, msg, data);
        return new OkObjectResult(resp);
    }

    public static IActionResult Paginated<T>(IEnumerable<T> items, int total, int page, int pageSize)
    {
        var resp = new PaginatedResponse<T>(true, total, page, pageSize, items);
        return new OkObjectResult(resp);
    }

    public static IActionResult Created<T> (
        ControllerBase controller, 
        string actionName, object routeValues, 
        T data, string? msg = null)
    {
        var resp = new BaseResponse<T>(true, msg, data);
        return controller.CreatedAtAction(actionName, routeValues, resp);
    }

    public static IActionResult Error(string msg, int code = 404)
    {
        var resp = new BaseResponse<string>(false, msg, null);
        return new ObjectResult(resp){ StatusCode = code };
    }

    public static IActionResult NotFound(string msg)
    {
        var resp = new BaseResponse<string>(false, msg, null);
        return new NotFoundObjectResult(resp);
    }
}