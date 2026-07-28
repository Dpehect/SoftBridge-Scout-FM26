using FmScout.Application.Players;
using FmScout.Application.Scouting;
using Microsoft.Extensions.DependencyInjection;
namespace FmScout.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PlayerService>();
        services.AddScoped<ScoutingService>();
        return services;
    }
}
