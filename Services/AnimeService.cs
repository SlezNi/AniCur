using AnimeWiki.Api.Data;
using AnimeWiki.Api.DTOs.AniList;
using AnimeWiki.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeWiki.Api.Services;

public class AnimeService
{
    private readonly AppDbContext _context;
    private readonly AniListService _aniListService;

    public AnimeService(AppDbContext context, AniListService aniListService)
    {
        _context = context;
        _aniListService = aniListService;
    }

    public async Task<Anime> ImportOrUpdateAnimeAsync(int aniListId)
    {
        var aniListData = await _aniListService.GetAnimeByIdAsync(aniListId);
        if (aniListData == null)
            throw new KeyNotFoundException($"Аніме з ID {aniListId} не знайдено в AniList.");

        var incomingGenres = aniListData.Genres ?? new List<string>();
        var existingGenres = await _context.Genres
            .Where(g => incomingGenres.Contains(g.Name))
            .ToListAsync();

        var existingGenreNames = existingGenres.Select(g => g.Name).ToHashSet();
        var newGenres = incomingGenres
            .Where(name => !existingGenreNames.Contains(name))
            .Distinct()
            .Select(name => new Genre { Name = name })
            .ToList();

        var animeGenres = existingGenres.Concat(newGenres).ToList();

        var incomingCharNodes = aniListData.Characters?.Nodes ?? new List<AniListCharacterNode>();
        var incomingCharIds = incomingCharNodes.Select(c => c.Id).Distinct().ToList();

        var existingCharacters = await _context.Characters
            .Where(c => incomingCharIds.Contains(c.AniListId))
            .ToListAsync();

        var existingCharIds = existingCharacters.Select(c => c.AniListId).ToHashSet();

        var newCharacters = incomingCharNodes
            .Where(c => !existingCharIds.Contains(c.Id))
            .Select(c => new Character
            {
                AniListId = c.Id,
                Name = c.Name.Full ?? c.Name.Native ?? "Невідомий",
                ImageUrl = c.Image?.Large
            })
            .ToList();

        var animeCharacters = existingCharacters.Concat(newCharacters).ToList();

        var existingAnime = await _context.Anime
            .Include(a => a.Genres)
            .Include(a => a.Characters)
            .FirstOrDefaultAsync(a => a.AniListId == aniListId);

        if (existingAnime != null)
        {
            MapAnime(existingAnime, aniListData);
            existingAnime.Genres.Clear();
            foreach (var genre in animeGenres)
                existingAnime.Genres.Add(genre);

            existingAnime.Characters.Clear();
            foreach (var character in animeCharacters)
                existingAnime.Characters.Add(character);

            await _context.SaveChangesAsync();
            return existingAnime;
        }

        var newAnime = new Anime();
        MapAnime(newAnime, aniListData);
        newAnime.Genres = animeGenres;
        newAnime.Characters = animeCharacters;

        _context.Anime.Add(newAnime);
        await _context.SaveChangesAsync();

        return newAnime;
    }

    private static void MapAnime(Anime anime, AniListMedia data)
    {
        anime.AniListId = data.Id;
        anime.Title = data.Title.English ?? data.Title.Romaji ?? data.Title.Native ?? "Без назви";
        anime.OriginalTitle = data.Title.Native;
        anime.Description = data.Description;
        anime.Episodes = data.Episodes;
        anime.Rating = data.AverageScore.HasValue
            ? data.AverageScore.Value / 10.0
            : null;
    }
}
