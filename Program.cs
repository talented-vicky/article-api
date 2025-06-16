using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// register controllers
builder.Services.AddControllers();

// register DbContext
builder.Services.AddDbContext<AppDbContext>(opts => 
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// register AuthService
builder.Services.AddScoped<AuthService>();

// JWT Bearer Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
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

app.UseHttpsRedirection();

// Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // expose '/api/auth'
app.Run();