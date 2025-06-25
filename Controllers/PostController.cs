using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Dtos;
using ArticleApi.Helpers;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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
            Content = dto.Content
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
    public async Task<IActionResult> FetchPosts(int page = 1, int pageSize = 2)
    {
        var totalItems = await _ctxt.Posts.CountAsync();
        var posts = await _ctxt.Posts
            .Include(post => post.User)
            .OrderBy(post => post.Id)
            // .Skip()
            .Take(pageSize)
            .Select(post => new PostDataDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
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
}