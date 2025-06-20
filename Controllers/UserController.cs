using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Dtos;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _ctxt;

    public UserController(AppDbContext context) => _ctxt = context;

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<UserDto>>> FetchAllUsers()
    {
        var totalItems = await _ctxt.Users.CountAsync();
        
        var users = await _ctxt.Users
            .Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            })
            .ToListAsync();

        return Ok(new {
            total = totalItems,
            data = users
        });
    }

    [HttpGet("{id:int}")] // maps to api/users/4
    public async Task<ActionResult<UserDto>> GetOneUser(int id)
    {
        var user = await _ctxt.Users
            .Where(user => user.Id == id)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username
            })
            .FirstOrDefaultAsync();
        
        return user is null ? NotFound($"User {id} not found") : Ok(user);
    }
}