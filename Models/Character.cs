using System.Text.Json.Serialization;

namespace AnimeWiki.Api.Models;

public class Character
{
    public int Id { get; set; }
    public int AniListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public ICollection<Anime> Animes { get; set; } = new List<Anime>();
}
