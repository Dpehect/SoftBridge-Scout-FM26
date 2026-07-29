using FmScout.Application.Common;
using FmScout.Application.Players;
using Microsoft.AspNetCore.Mvc;
namespace FmScout.Api.Controllers;
[ApiController, Route("api/players")]
public sealed class PlayersController(PlayerService service) : ControllerBase
{
    [HttpGet] [ProducesResponseType<PagedResult<PlayerListItemDto>>(200)]
    public Task<PagedResult<PlayerListItemDto>> Search([FromQuery] PlayerSearchQuery query, CancellationToken ct) => service.SearchAsync(query,ct);
    [HttpGet("{slug}")] [ProducesResponseType<PlayerDetailDto>(200)] [ProducesResponseType(404)]
    public Task<PlayerDetailDto> Get(string slug, CancellationToken ct) => service.GetBySlugAsync(slug,ct);
}
