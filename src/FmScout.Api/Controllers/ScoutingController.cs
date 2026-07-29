using FmScout.Application.Scouting;
using Microsoft.AspNetCore.Mvc;
namespace FmScout.Api.Controllers;
[ApiController]
[Route("api/scouting")]
public sealed class ScoutingController(ScoutingService service) : ControllerBase
{
    [HttpGet("recommendations")]
    [ProducesResponseType<IReadOnlyList<ScoutRecommendationDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<ScoutRecommendationDto>> Recommend([FromQuery] ScoutRequest request, CancellationToken ct)
        => service.RecommendAsync(request, ct);

    [HttpGet("compare")]
    [ProducesResponseType<PlayerComparisonDto>(StatusCodes.Status200OK)]
    public Task<PlayerComparisonDto> Compare([FromQuery] string[] slugs, CancellationToken ct)
        => service.CompareAsync(slugs, ct);

    [HttpGet("stats")]
    public Task<PlatformStatsDto> Stats(CancellationToken ct) => service.GetStatsAsync(ct);
}
