using ArticleApi.Models;
using ArticleApi.Data;
using ArticleApi.Services;
using ArticleApi.Dtos;

using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CloudinaryDotNet;
using NetTopologySuite;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// accessing dotenv file
Env.Load();

// register controllers
builder.Services.AddControllers();

// register DbContext
builder.Services.AddDbContext<AppDbContext>(opts => 
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseNetTopologySuite() // for geolocation
    ));

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

        // adding this so "[Authorize]" doesn't block every request involving token validation/expiration at the middleware layer
        opts.Events = new JwtBearerEvents
        {
            OnChallenge = context => 
            {
                context.HandleResponse();

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                var resp = new 
                {
                    success = false,
                    message = "Invalid or Expired Token",
                    data = (object?)null
                };

                return context.Response.WriteAsJsonAsync(resp);
            }
        };
    });

// Cloudinary Config
var CloudinarySettings = new CloudinarySettingsDto
{
    CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_NAME") ?? throw new Exception("Missing CLOUDINARY_NAME!"),
    ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? throw new Exception("Missing CLOUDINARY_API_KEY!"),
    ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? throw new Exception("Missing CLOUDINARY_API_SECRET!")
};

builder.Services.AddSingleton(CloudinarySettings);
builder.Services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));
builder.Services.AddSingleton(s => 
{
    var account = new Account(
        CloudinarySettings.CloudName,
        CloudinarySettings.ApiKey,
        CloudinarySettings.ApiSecret 
    );
    return new Cloudinary(account);
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

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // expose '/api/auth'
app.Run();