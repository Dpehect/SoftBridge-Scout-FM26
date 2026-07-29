using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
namespace FmScout.Api.Security;
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
public sealed class AdminKeyAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expected=context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Admin:ApiKey"];
        var supplied=context.HttpContext.Request.Headers["X-Admin-Key"].FirstOrDefault();
        if(string.IsNullOrWhiteSpace(expected)||!CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(expected),System.Text.Encoding.UTF8.GetBytes(supplied??"")))
        { context.Result=new UnauthorizedObjectResult(new{error="Valid X-Admin-Key required."}); return; }
        await next();
    }
}
