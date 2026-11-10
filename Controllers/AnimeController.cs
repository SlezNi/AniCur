using AnimeWiki.Api.Data;
using AnimeWiki.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeWiki.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimeController : ControllerBase
{
    private readonly AppDbContext _context;

    public AnimeController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Отримує список аніме з пошуком, фільтрацією за жанром та пагінацією.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<AnimeListItemDto>>> GetAnimeList(
        [FromQuery] string? search,
        [FromQuery] string? genre,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Anime.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(searchLower) ||
                (a.OriginalTitle != null && a.OriginalTitle.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var genreLower = genre.Trim().ToLower();
            query = query.Where(a => a.Genres.Any(g => g.Name.ToLower() == genreLower));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Rating)
            .ThenBy(a => a.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AnimeListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                Episodes = a.Episodes,
                Rating = a.Rating
            })
            .ToListAsync();

        return Ok(new PagedResponse<AnimeListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>Отримує детальну інформацію про аніме з жанрами та персонажами.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnimeDetailDto>> GetAnimeDetails(int id)
    {
        var anime = await _context.Anime
            .AsNoTracking()
            .Include(a => a.Genres)
            .Include(a => a.Characters)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (anime == null)
            return NotFound(new { message = $"Аніме з ID {id} не знайдено." });

        var dto = new AnimeDetailDto
        {
            Id = anime.Id,
            Title = anime.Title,
            OriginalTitle = anime.OriginalTitle,
            Description = anime.Description,
            Episodes = anime.Episodes,
            Rating = anime.Rating,
            Genres = anime.Genres.Select(g => g.Name).ToList(),
            Characters = anime.Characters.Select(c => new CharacterDto
            {
                Id = c.Id,
                Name = c.Name,
                ImageUrl = c.ImageUrl
            }).ToList()
        };

        return Ok(dto);
    }
}
