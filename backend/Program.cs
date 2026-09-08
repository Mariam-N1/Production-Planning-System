using BackendProject.Data;
using BackendProject.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- settings check ----------
// The connection string and the signing key are NOT in appsettings.json,
// so they never reach GitHub. They come from user-secrets instead.
// Failing here with a readable message beats a cryptic crash later.
var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "";

if (string.IsNullOrWhiteSpace(connection) || jwtKey.Length < 32)
{
    Console.WriteLine();
    Console.WriteLine("  This project reads its secrets from user-secrets, not appsettings.json.");
    Console.WriteLine("  Run these once, from this folder:");
    Console.WriteLine();
    Console.WriteLine("    dotnet user-secrets init");
    Console.WriteLine("    dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Server=localhost,1433;Database=CrudDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;\"");
    Console.WriteLine("    dotnet user-secrets set \"Jwt:Key\" \"any-long-random-string-at-least-32-characters\"");
    Console.WriteLine();
    return;
}

// Database connection
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

builder.Services.AddControllers();
builder.Services.AddScoped<EmailSender>();

// Swagger (the testing page at /swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- JWT authentication ----------
// Checks the "Authorization: Bearer <token>" header on every request
// that reaches an [Authorize] endpoint.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,            // expired tokens are rejected
            ValidateIssuerSigningKey = true,    // and tampered ones
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

// CORS - lets the Angular app on another port call this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Order matters: who are you, then what may you do.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Backend is working!");

app.Run();
