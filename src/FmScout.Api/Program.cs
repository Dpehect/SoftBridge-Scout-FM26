using FmScout.Api.Auth;
using FmScout.Api.Middleware;
using FmScout.Application.Players;
using FmScout.Application.Scouting;
using FmScout.Domain.Entities;
using FmScout.Infrastructure;
using FmScout.Infrastructure.Persistence;
using FmScout.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<ScoutingService>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();

var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt__Secret is missing or shorter than 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "fm26-scout-api",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "fm26-scout-web",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks().AddDbContextCheck<FmScoutDbContext>();
builder.Services.AddResponseCompression();

builder.Services.AddRateLimiter(options =>
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 120;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.AutoReplenishment = true;
    }));

var origins = builder.Configuration
    .GetSection("Cors:Origins")
    .Get<string[]>()
    ?.Where(x => !string.IsNullOrWhiteSpace(x))
    .Select(x => x.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("web", policy =>
    {
        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    }));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseResponseCompression();
app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("api");
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new
{
    service = "FM26 Scout API",
    status = "healthy"
}));

await InitializeDatabaseAsync(app.Services, app.Logger);

app.Run();

static async Task InitializeDatabaseAsync(
    IServiceProvider services,
    ILogger logger)
{
    const int maxAttempts = 8;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FmScoutDbContext>();

            await db.Database.CanConnectAsync();
            await DatabaseSeeder.SeedAsync(db);

            logger.LogInformation("Database connection and seed completed.");
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            var delay = TimeSpan.FromSeconds(Math.Min(5 * attempt, 30));

            logger.LogWarning(
                exception,
                "Database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds} seconds.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay);
        }
    }

    using var finalScope = services.CreateScope();
    var finalDb = finalScope.ServiceProvider.GetRequiredService<FmScoutDbContext>();
    await finalDb.Database.CanConnectAsync();
    await DatabaseSeeder.SeedAsync(finalDb);
}

public partial class Program;
