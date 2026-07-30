using FmScout.Api.Services;
using FmScout.Infrastructure;
using FmScout.Infrastructure.Persistence;
using FmScout.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

var builder=WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx,cfg)=>cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient<SortitoutsiImporter>(client=>
{
    client.Timeout=TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-GB,en;q=0.9");
});

var configuredOrigins=builder.Configuration.GetSection("Cors:Origins").Get<string[]>()??Array.Empty<string>();
builder.Services.AddCors(options=>options.AddPolicy("web",policy=>policy
    .SetIsOriginAllowed(origin=>
        configuredOrigins.Contains(origin,StringComparer.OrdinalIgnoreCase)||
        Uri.TryCreate(origin,UriKind.Absolute,out var uri)&&
        (uri.Host.Equals("localhost",StringComparison.OrdinalIgnoreCase)||
         uri.Host.EndsWith(".vercel.app",StringComparison.OrdinalIgnoreCase)))
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app=builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions{ForwardedHeaders=ForwardedHeaders.XForwardedFor|ForwardedHeaders.XForwardedProto});
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseCors("web");
app.MapControllers();
app.MapGet("/",()=>Results.Ok(new{service="SoftBridge Scout FM26 API",status="healthy",databaseConfigured=!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection"))}));
app.MapGet("/api/status",()=>Results.Ok(new{success=true,message="FM26 Scout backend is running."}));
app.MapGet("/health",()=>Results.Ok(new{status="healthy"}));
app.MapGet("/health/db",async(FmScoutDbContext? db,CancellationToken ct)=>{if(db is null)return Results.Problem("Database connection is not configured.",statusCode:503);try{return await db.Database.CanConnectAsync(ct)?Results.Ok(new{status="healthy",database="connected"}):Results.Problem("Database connection failed.",statusCode:503);}catch(Exception ex){return Results.Problem(ex.Message,statusCode:503);}});
if(!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("DefaultConnection")))
{
    using var scope=app.Services.CreateScope();
    try{var db=scope.ServiceProvider.GetRequiredService<FmScoutDbContext>();await DatabaseSeeder.SeedAsync(db);}catch(Exception ex){app.Logger.LogError(ex,"Database initialization failed; API will continue running.");}
}
app.Run();
public partial class Program;