using FmScout.Application.Abstractions;
using FmScout.Infrastructure.Persistence;
using FmScout.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace FmScout.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        services.AddDbContextPool<FmScoutDbContext>(options => options.UseNpgsql(connectionString, npgsql =>
            npgsql.EnableRetryOnFailure(4, TimeSpan.FromSeconds(5), null)));
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IScoutingRepository, ScoutingRepository>();
        return services;
    }
}
