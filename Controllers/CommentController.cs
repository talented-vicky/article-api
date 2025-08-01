using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Controllers;

[ApiController]
[Route("/api/comments")]
public class CommentController : ControllerBase
{
    private readonly AppDbContext _ctxt;
    public CommentController(AppDbContext context) => _ctxt = context;


    [HttpGet("{postId}", Name="FetchPostCommentsRoute")]
    public async Task<IActionResult> FetchPostComments(int postId, [FromQuery] int page, [FromQuery] int pageSize)
    {
        var totalComments = await _ctxt.Comments
            .Where(comment => comment.PostId == postId)
            .CountAsync();

        var comments = await _ctxt.Comments
            .Where(comment => comment.PostId == postId)
            .Select(c => new 
            {
                c.Id,
                c.Content,
                c.User.Username,
                c.CommentedAt
            })
            .ToListAsync();

        if(comments == null)
            return ApiResponse.NotFound("Comment Not Found");

        return ApiResponse.Paginated(comments, totalComments, page, pageSize);
    }
}