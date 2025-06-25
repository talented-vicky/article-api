using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Dtos;

using ArticleApi.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _ctxt;

    public UserController(AppDbContext context) => _ctxt = context;

    [HttpGet]
    public async Task<IActionResult> FetchAllUsers(int page = 1, int pageSize = 2)
    {
        var totalItems = await _ctxt.Users.CountAsync();
        var users = await _ctxt.Users
            .OrderBy(user => user.Id)
            // .Skip()
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