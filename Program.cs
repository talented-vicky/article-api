using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Services;

using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// accessing dotenv file
Env.Load();

// register controllers
builder.Services.AddControllers();

// register DbContext
builder.Services.AddDbContext<AppDbContext>(opts => 
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// register AuthService
builder.Services.AddScoped<AuthService>();

// CORS policy
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
    });
});

// JWT Bearer Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
if(string.IsNullOrEmpty(jwtKey))
    throw new Exception("JWT_KEY is not set in the environment!");
    
var key = Encoding.UTF8.GetBytes(jwtKey!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => 
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocalhost");

app.UseHttpsRedirection();

// Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // expose '/api/auth'
app.Run();