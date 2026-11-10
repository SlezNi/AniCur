using AnimeWiki.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnimeWiki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly AnimeService _animeService;

    public ImportController(AnimeService animeService)
    {
        _animeService = animeService;
    }

    [HttpPost("anilist/{id:int}")]
    public async Task<IActionResult> ImportAnime(int id)
    {
        try
        {
            var anime = await _animeService.ImportOrUpdateAnimeAsync(id);
            return Ok(anime);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return BadRequest(new { message = $"Помилка запиту до AniList: {ex.Message}" });
        }
    }
}
