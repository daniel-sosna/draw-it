using System.Text.Json;
using System.Text.Json.Serialization;
using Draw.it.Server.Data;
using Draw.it.Server.Exceptions;
using Draw.it.Server.Hubs;
using Draw.it.Server.Repositories;
using Draw.it.Server.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/v1/auth/unauthorized"; // optional, can point to an endpoint
        options.Cookie.Name = "user-id";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.None; // Allow cross-site cookies
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices().AddApplicationRepositories(builder.Configuration);

// Optional: register EF Core if a Postgres connection string is present
var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Allow frontend to send requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:61528")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

// Use authentication/authorization middlewares
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<LobbyHub>("/lobbyHub");
app.MapHub<GameplayHub>("/gameplayHub");

app.MapFallbackToFile("/index.html");

app.UseMiddleware<ExceptionHandler>();

// Auto-create schema if DbContext is configured
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
    db?.Database.EnsureCreated();
}
catch (Exception e)
{
    Console.WriteLine($"Database initialization skipped or failed: {e.Message}");
}

app.Run();
