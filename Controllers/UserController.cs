using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Dtos;

using ArticleApi.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ArticleApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _ctxt;

    public UserController(AppDbContext context) => _ctxt = context;

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if(userIdClaim == null)
            return ApiResponse.NotFound("Invalid User Token");

        int userId = int.Parse(userIdClaim.Value);

        var user = await _ctxt.Users
            .Where(user => user.Id == userId)
            .Select(u => new 
            {
                u.Id,
                u.Username,
                u.Email,

                PostCount = u.Posts.Count,
                RecentPosts = u.Posts
                    .OrderByDescending(post => post.PostedAt)
                    .Take(4)
                    .Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.Content,
                        p.PostLikes.Count,
                        p.Views,
                        p.PostedAt
                    }),
                Analytics = new 
                {
                    TotalViews = u.Posts.Sum(post => post.Views),
                    TotalLikes = u.Posts.Sum(post => post.PostLikes.Count)
                },
                TotalComments = u.Comments.Count(comment => comment.UserId == u.Id)
            })
            .FirstOrDefaultAsync();
        
        if(user == null)
            return ApiResponse.NotFound("User Not Found");

        return ApiResponse.Success(user, "Successfully Fetched Dashboard Info");
    }

    [HttpGet]
    public async Task<IActionResult> FetchAllUsers(int page = 1, int pageSize = 2)
    {
        var totalItems = await _ctxt.Users.CountAsync();
        var users = await _ctxt.Users
            .OrderBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new 
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            })
            .ToListAsync();        

        return ApiResponse.Paginated(users, totalItems, page, pageSize);
    }

    [HttpGet("{id:int}")] // maps to api/users/4
    public async Task<IActionResult> GetOneUser(int id)
    {
        var user = await _ctxt.Users
            .Where(user => user.Id == id)
            .Select(user => new UserDataDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            })
            .FirstOrDefaultAsync();
        
        return user is null ? 
            ApiResponse.NotFound("User Not Found") : 
            ApiResponse.Success(user, "Successfully Fetched User");
    }
}