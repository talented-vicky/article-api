using ArticleApi.Models;
using ArticleApi.Services;
using ArticleApi.Dtos;

using Microsoft.AspNetCore.Mvc;
// provides controllers like ControllerBase (base class for api controllers) and ApiController (for automatic model validation and better error messages)


namespace ArticleApi.Controllers;

[ApiController]
[Route("api/auth")] // automatically mapping this controller to 'api/auth'
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("register")] // becomes -> "/api/auth/register"
    public async Task<ActionResult<User>> Register (RegistrationDto req)
    {
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
        };

        var createdUser = await _authService.Register(user, req.Password);
        return Ok(createdUser);
    }

    [HttpPost("login")] // becomes -> "/api/auth/login"
    public async Task<ActionResult<string>> Login(LoginDto req)
    {
        var token = await _authService.Login(req.Email, req.Password);
        if(token == null)
            return Unauthorized("Invalid Email or Password");
        
        return Ok(new { token }); 
    }
}