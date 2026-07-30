using FmScout.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FmScout.Api.Controllers;

[ApiController]
[Route("api/import/sortitoutsi")]
public sealed class ImportController(
    SortitoutsiImporter importer,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("leagues")]
    public IActionResult GetLeagues() => Ok(SortitoutsiImporter.Leagues.Select((x, index) => new
    {
        index,
        x.Nation,
        x.League
    }));

    [HttpPost("run")]
    public async Task<IActionResult> Run(
        [FromQuery] int leagueIndex = 0,
        [FromQuery] int clubOffset = 0,
        [FromQuery] int clubLimit = 3,
        CancellationToken ct = default)
    {
        if (!IsAuthorized()) return Unauthorized(new { error = "Invalid import key." });

        try
        {
            var result = await importer.RunBatchAsync(leagueIndex, clubOffset, clubLimit, ct);
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Source request failed.", detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
    }

    private bool IsAuthorized()
    {
        var configuredKey = configuration["SORTITOUTSI_IMPORT_KEY"];
        if (string.IsNullOrWhiteSpace(configuredKey)) return false;
        return Request.Headers.TryGetValue("X-Import-Key", out var suppliedKey) &&
               string.Equals(configuredKey, suppliedKey.ToString(), StringComparison.Ordinal);
    }
}