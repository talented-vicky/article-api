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
    public async Task<ActionResult<PostDto>> CreatePost(PostDto dto)
    {
        var user = await _ctxt.Users.FindAsync(dto.UserId);
        if(user == null)
            return NotFound($"The user with id: ${dto.UserId} does not exist");

        var post = new Post
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Content = dto.Content
        };

        _ctxt.Posts.Add(post);
        await _ctxt.SaveChangesAsync();

        dto.Id = post.Id; // just ensuring the id is DB generated
        return CreatedAtAction(
            nameof(GetOnePost),
            new { id = post.Id},
            dto
        );
    }

    [HttpGet]
    public async Task<IActionResult> FetchPosts()
    {
        var totalItems = await _ctxt.Posts.CountAsync();

        var posts = await _ctxt.Posts
            .Select(post => new PostDto
            {
                // Id = post.Id,
                UserId = post.UserId,
                Title = post.Title,
                Content = post.Content
            })
            .ToListAsync();
        
        // return Ok(new {
        //     total = totalItems,
        //     data = posts
        // });
        return ApiResponse.Paginated(posts, totalItems, 1, 2, "Fetched All Posts");
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOnePost(int id)
    {
        var post = await _ctxt.Posts
            .Where(post => post.Id == id)
            .Select(post => new PostDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Title = post.Title,
                Content = post.Content
            })
            .FirstOrDefaultAsync();

        return post is null 
            ? ApiResponse.Error($"Post with id: {id} not found")
            : ApiResponse.Success(post, "Successfully fetched post");
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<PostDto>> GetUserPost(int userId)
    {
        var user = await _ctxt.Users.FindAsync(userId);
        if(user == null)
            return NotFound(new {
                status = true,
                data = "User not found"
            });

        var posts = await _ctxt.Posts
            .Where(post => post.UserId == userId)
            .Select(post => new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = "post.Content"
            }).ToListAsync();

        return posts is null
            ? NotFound($"No post created by: {user.Username} yet")
            : Ok(new {
                status = true,
                data = posts
            });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePost(int id)
    {
        var post = await _ctxt.Posts.FindAsync(id);
        if(post == null)
            return NotFound(new {
                status = true,
                data = $"Post with id: {id} does not exist"
            });
        
        _ctxt.Posts.Remove(post);
        await _ctxt.SaveChangesAsync();

        return NoContent(); // http 204
    }
}