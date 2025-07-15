using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Dtos;
using ArticleApi.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
// has helper methods (Ok(obj) => 200, NotFound(msg) => 404, CreatedAtAction(...) => 201, and NoContent() => 202) with default http status

namespace ArticleApi.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly AppDbContext _ctxt;

    public PostController (AppDbContext context) => _ctxt = context;

    [HttpPost("create")]
    // EF Cor I/O (SaveChangesAsync, ToListAsync(), etc) uses an async to return a Task
    // so Task<ActionResult<T>> returns a an http result whose body on success is a T data
    public async Task<IActionResult> CreatePost(PostQueryDto dto)
    {
        var user = await _ctxt.Users.FindAsync(dto.UserId);
        if(user == null)
            return ApiResponse.NotFound($"User does not exist");

        var post = new Post
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Content = dto.Content,
            PostedAt = dto.CreatedAt
        };

        _ctxt.Posts.Add(post);
        await _ctxt.SaveChangesAsync();

        var createdPost = new {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
        };

        return ApiResponse.Created(nameof(GetOnePost), createdPost, "Post Successfully Created");
    }

    [HttpGet]
    public async Task<IActionResult> FetchPosts(int page = 1, int pageSize = 5)
    {
        var totalItems = await _ctxt.Posts.CountAsync();
        var posts = await _ctxt.Posts
            .Include(post => post.User)
            .OrderBy(post => post.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(post => new
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Views = post.Views,
                Likes = post.PostLikes.Count(),
                UserId = post.UserId,
                Username = post.User.Username,
                Email = post.User.Email
            })
            .ToListAsync();
        
        return ApiResponse.Paginated(posts, totalItems, page, pageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOnePost(int id)
    {
        var post = await _ctxt.Posts
            .Where(post => post.Id == id)
            .FirstOrDefaultAsync();

        return post is null 
            ? ApiResponse.NotFound($"Post Not Found")
            : ApiResponse.Success(post, "Successfully Fetched Post");
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetUserPosts(int userId)
    {
        var user = await _ctxt.Users.FindAsync(userId);
        if(user == null)
            return ApiResponse.NotFound("User Not Found");

        var posts = await _ctxt.Posts
            .Where(post => post.UserId == userId)
            .ToListAsync();

        return posts is null
            ? ApiResponse.Error($"No post created by: {user.Username} yet")
            : ApiResponse.Success(posts, $"Successfully Fetched Posts by {user.Username}");
    }

    [HttpGet("{id}/view")]
    public async Task<IActionResult> ViewPost (int id)
    {
        var post = await _ctxt.Posts.FindAsync(id);
        if(post == null)
            return ApiResponse.NotFound("Post Not Found");

        post.Views += 1;
        await _ctxt.SaveChangesAsync();

        return ApiResponse.Success(new {post.Id, post.Views}, "Views Updated");
    }

    [Authorize]
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikePost (int id)
    {
        var userIdClaims = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaims == null)
            return ApiResponse.NotFound("Unauthorized User");
        
        int userId = int.Parse(userIdClaims.Value);

        var post = await _ctxt.Posts.FindAsync(id);
        if(post == null)
            return ApiResponse.NotFound("Post Not Found");

        int updatedLikes;
        var alreadyLiked = await _ctxt.PostLikes.FirstOrDefaultAsync(like => like.PostId == id && like.UserId == userId);
        if(alreadyLiked != null)
        {
            _ctxt.PostLikes.Remove(alreadyLiked);
            await _ctxt.SaveChangesAsync();

            updatedLikes = await _ctxt.PostLikes.CountAsync(like => like.PostId == id);
            return ApiResponse.Success(new {post.Id, likes = updatedLikes}, "Post Unliked");
        }

        _ctxt.PostLikes.Add(new PostLike
        {
            PostId = id,
            UserId = userId
        });
        await _ctxt.SaveChangesAsync();
        updatedLikes = await _ctxt.PostLikes.CountAsync(like => like.PostId == id);
        return ApiResponse.Success(new {post.Id, likes = updatedLikes}, "Post Liked");
    }

    [Authorize]
    [HttpPost("{id}/comment")]
    public async Task<IActionResult> CommentPost (int postId, string content)
    {
        var userIdClaims = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaims == null)
            return ApiResponse.NotFound("Unauthorized User");
        // create appropriate apiresponse for unauthroized users

        int userId = int.Parse(userIdClaims.Value);

        var post = await _ctxt.Posts.FindAsync(postId);
        if(post == null)
            return ApiResponse.NotFound("Post Not Found");

        var comment = new Comment 
        {
            UserId = userId,
            PostId = post.Id,
            Content = content,
            CommentedAt = DateTime.UtcNow
        };

        _ctxt.Comments.Add(comment);
        await _ctxt.SaveChangesAsync();

        return ApiResponse.Success(comment, "Comment Added to Post");
    }
}