namespace AnimeWiki.Api.DTOs;

public class AnimeListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Episodes { get; set; }
    public double? Rating { get; set; }
}

public class AnimeDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Description { get; set; }
    public int? Episodes { get; set; }
    public double? Rating { get; set; }
    public List<string> Genres { get; set; } = new();
    public List<CharacterDto> Characters { get; set; } = new();
}

public class CharacterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
