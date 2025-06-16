using ArticleApi.Models;

using ArticleApi.Data;
using System.Text; 
// work with strings & byte arrays via Encoding.UTF8.GetBytes(string) for password hashing and encoding jwt keys

using Microsoft.EntityFrameworkCore;
// interact with postgreSQL to create queries (context.Users.FirstOrDefaultAsync(...)), save to DB (context.SaveChangesAsync()), and also for migrations & model building

using Microsoft.IdentityModel.Tokens; // sign JWTs securely
// provides SymmetricSecurityKey and SigningCredentials for signing and validating jwt tokens
using System.IdentityModel.Tokens.Jwt;
// create/read jwt tokens using JwtSecurityToken and JwtSecurityTokenHandler to build token, write it as a string, and return it to the client

using System.Security.Claims; 
// part of jwt payload, used for creating user identity claims (e.g username, user Id) to help backend know "who" is calling
using System.Security.Cryptography; 
// needed for HMACSHA512 in hashing creation and verification



namespace ArticleApi.Services;


public class AuthService
{
    // instance variables
    private readonly AppDbContext _ctxt; // granting read/write access to postgreSQL database tables (AppDbContext -> EF core)
    // private readonly IConfiguration _config; // granting AuthService read access to secrets from appsettings.json or env var

    // contructor injection; for ease in unit-test & also keeps dependencies explicit
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _ctxt = context;
        // _config = configuration;
    }

    public async Task<User> Register(User user, string password)
    {
        CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        _ctxt.Users.Add(user);
        await _ctxt.SaveChangesAsync();

        return user;
    }

    public async Task<string?> Login(string email, string password)
    {
        var user = await _ctxt.Users.FirstOrDefaultAsync(user => user.Email == email);
        if(user == null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            return null;

        return CreateToken(user);
    }


    public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA512();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPasswordHash(string password, byte[] hash, byte[] salt)
    {
        using var hmac = new HMACSHA512(salt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(hash);
    }

    private string CreateToken(User user)
    {
        // build a list of claims (data carried in the token): in this case user Id and userName
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        // reading key from dotenv file
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        if(string.IsNullOrEmpty(jwtKey))
            throw new Exception("JWT_KEY is not empty or not set!");

        // load key from appsettings.json and convert it into a SymmetricSecurityKey -> loading of key
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey!));
        
        // bundle converted key and hmac algorithm into signing cred -> -> signing of key
        var creds = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha512Signature);
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        // serialize token to a compact string the browser/mobile app can store in Authorization: Bearer<Token> headers
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
} 