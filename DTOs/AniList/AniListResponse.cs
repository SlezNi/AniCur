using System.Text.Json.Serialization;

namespace AnimeWiki.Api.DTOs.AniList;

public class AniListResponse
{
    [JsonPropertyName("data")]
    public AniListData? Data { get; set; }
}

public class AniListData
{
    [JsonPropertyName("Media")]
    public AniListMedia? Media { get; set; }
}

public class AniListMedia
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public AniListTitle Title { get; set; } = new();

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("averageScore")]
    public int? AverageScore { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    [JsonPropertyName("characters")]
    public AniListCharacterConnection? Characters { get; set; }
}

public class AniListTitle
{
    [JsonPropertyName("romaji")]
    public string? Romaji { get; set; }

    [JsonPropertyName("english")]
    public string? English { get; set; }

    [JsonPropertyName("native")]
    public string? Native { get; set; }
}

public class AniListCharacterConnection
{
    [JsonPropertyName("nodes")]
    public List<AniListCharacterNode> Nodes { get; set; } = new();
}

public class AniListCharacterNode
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public AniListCharacterName Name { get; set; } = new();

    [JsonPropertyName("image")]
    public AniListCharacterImage? Image { get; set; }
}

public class AniListCharacterName
{
    [JsonPropertyName("full")]
    public string? Full { get; set; }

    [JsonPropertyName("native")]
    public string? Native { get; set; }
}

public class AniListCharacterImage
{
    [JsonPropertyName("large")]
    public string? Large { get; set; }
}
