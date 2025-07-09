using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Controllers;

[ApiController]
[Route("/api/comment")]
public class CommentController : ControllerBase
{
    private readonly AppDbContext _ctxt;
    public CommentController(AppDbContext context) => _ctxt = context;

    [HttpGet]
    public async Task<IActionResult> FetchUserComments(int userId, int postId, int page, int pageSize )
    {
        var totalComments = await _ctxt.Comments
            .Where(comment => comment.UserId == userId && comment.PostId == postId)
            .CountAsync();

        var comments = await _ctxt.Comments
            .Where(comment => comment.UserId == userId && comment.PostId == postId)
            .Select(c => new 
            {
                c.Id,
                c.Content
            })
            .ToListAsync();

        if(comments == null)
            return ApiResponse.NotFound("Comment Not Found");

        return ApiResponse.Paginated(comments, totalComments, page, pageSize);
    }
}