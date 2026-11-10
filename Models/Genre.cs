using System.Text.Json.Serialization;

namespace AnimeWiki.Api.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Anime> Animes { get; set; } = new List<Anime>();
}
