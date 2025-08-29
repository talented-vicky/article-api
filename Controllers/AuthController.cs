using ArticleApi.Models;
using ArticleApi.Services;
using ArticleApi.Dtos;
using ArticleApi.Helpers;

using Microsoft.AspNetCore.Mvc;
// provides controllers like ControllerBase (base class for api controllers) and ApiController (for automatic model validation and better error messages)


namespace ArticleApi.Controllers;

[ApiController]
[Route("api/auth")] // automatically mapping this controller to 'api/auth'
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("signup")] // becomes -> "/api/auth/signup"
    public async Task<IActionResult> SignUp (RegistrationDto req)
    {
        var user = new User
        {
            Username = req.Username,
            Email = req.Email,
            State = req.State,
            Lga = req.Lga,
        };

        var createdUser = await _authService.SignUp(user, req.Password);
        return ApiResponse.Success(createdUser, "Registration Successful");
    }

    [HttpPost("login")] // becomes -> "/api/auth/login"
    public async Task<IActionResult> Login(LoginDto req)
    {
        var token = await _authService.Login(req.Email, req.Password);
        if(token == null)
            return Unauthorized("Invalid Email or Password");
        
        return ApiResponse.Success(token, "Now Logged in"); 
    }
}