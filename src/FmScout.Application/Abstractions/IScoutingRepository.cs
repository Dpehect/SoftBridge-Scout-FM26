using FmScout.Application.Scouting;
namespace FmScout.Application.Abstractions;
public interface IScoutingRepository
{
    Task<IReadOnlyList<ScoutRecommendationDto>> RecommendAsync(ScoutRequest request, CancellationToken cancellationToken);
    Task<PlayerComparisonDto> CompareAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken);
    Task<PlatformStatsDto> GetStatsAsync(CancellationToken cancellationToken);
}
