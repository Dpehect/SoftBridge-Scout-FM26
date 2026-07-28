using Microsoft.AspNetCore.Mvc;
namespace FmScout.Api.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex) {
            logger.LogError(ex,"Unhandled exception"); var status = ex is KeyNotFoundException ? 404 : 500;
            context.Response.StatusCode=status; await context.Response.WriteAsJsonAsync(new ProblemDetails{Status=status,Title=status==404?"Resource not found":"Unexpected server error",Detail=context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()?ex.Message:null,Instance=context.Request.Path});
        }
    }
}
