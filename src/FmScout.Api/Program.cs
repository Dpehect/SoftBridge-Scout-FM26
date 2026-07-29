var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "SoftBridge Scout FM26 API",
    status = "healthy",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/status", () => Results.Ok(new
{
    success = true,
    message = "FM26 Scout backend is running."
}));

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
