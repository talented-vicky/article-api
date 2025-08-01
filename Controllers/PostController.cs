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

    [Authorize]
    [HttpPost("create")]
    // EF Cor I/O (SaveChangesAsync, ToListAsync(), etc) uses an async to return a Task
    // so Task<ActionResult<T>> returns a an http result whose body on success is a T data
    public async Task<IActionResult> CreatePost(PostQueryDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaim == null)
            return ApiResponse.NotFound("Unauthorized user");
        
        var userId = int.Parse(userIdClaim.Value);

        var isVisibilityValid = Enum.TryParse<Visibility>(dto.Visibility, true, out var visibility);
        if(!isVisibilityValid)
            return ApiResponse.NotFound("Invalid visibility value");

        var post = new Post
        {
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            Visibility = visibility,
            // PostedAt = dto.CreatedAt
        };

        foreach (var url in dto.ImageUrls)
        {
            post.PostImages.Add(new PostImage { Url = url });
        }

        _ctxt.Posts.Add(post);
        await _ctxt.SaveChangesAsync();

        // return ApiResponse.Created(nameof(GetOnePost), post, "Post Successfully Created");
        return ApiResponse.Created(
            this,
            actionName: "GetOnePost",
            routeValues: new {id = post.Id}, 
            data: post, 
            msg: "Post Successfully Created"
        );
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
                Comments = post.Comments.Count(),
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
            return ApiResponse.Error("Post Not Found");

        var alreadyLiked = await _ctxt.PostLikes.FirstOrDefaultAsync(like => like.PostId == id && like.UserId == userId);
        if(alreadyLiked != null)
        {
            _ctxt.PostLikes.Remove(alreadyLiked);
            await _ctxt.SaveChangesAsync();
            return ApiResponse.Completed(true, "Post Unliked");
        }

        _ctxt.PostLikes.Add(new PostLike
        {
            PostId = id,
            UserId = userId
        });
        await _ctxt.SaveChangesAsync();        
        return ApiResponse.Completed(true, "Post Liked");
    }

    [Authorize]
    [HttpPost("comment")]
    public async Task<IActionResult> CommentPost(CommentDto dto) 
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaim == null)
            return ApiResponse.NotFound("Unauthenticated user");

        var userId = int.Parse(userIdClaim.Value);

        var post = await _ctxt.Posts.FindAsync(dto.PostId);
        if(post == null)
            return ApiResponse.NotFound("Post Not Found");

        var comment = new Comment {
            Content = dto.Content,
            PostId = post.Id, // Saving id gotten 4rm database
            UserId = userId,
        };

        _ctxt.Comments.Add(comment);
        await _ctxt.SaveChangesAsync();

        return ApiResponse.CreatedAtRoute(
            this,
            routeName: "FetchPostCommentsRoute", 
            routeValues: new {postId = post.Id, page = 1, pageSize = 10},
            data: comment.Id, 
            msg: "Comment Added"
        );
    }
}