namespace AnimeWiki.Api.Models;

public class Anime
{
    public int Id { get; set; }
    public int AniListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int? Episodes { get; set; }
    public double? Rating { get; set; }

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Character> Characters { get; set; } = new List<Character>();
}
