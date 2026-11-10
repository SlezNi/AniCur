using System.Net.Http.Json;
using AnimeWiki.Api.DTOs.AniList;

namespace AnimeWiki.Api.Services;

public class AniListService
{
    private readonly HttpClient _httpClient;

    public AniListService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://graphql.anilist.co");
    }

    public async Task<AniListMedia?> GetAnimeByIdAsync(int aniListId)
    {
        const string query = """
            query ($id: Int) {
              Media(id: $id, type: ANIME) {
                id
                title { romaji english native }
                description
                episodes
                averageScore
                genres
                characters(sort: [ROLE, RELEVANCE], perPage: 15) {
                  nodes {
                    id
                    name { full native }
                    image { large }
                  }
                }
              }
            }
            """;

        var requestBody = new
        {
            query,
            variables = new { id = aniListId }
        };

        using var response = await _httpClient.PostAsJsonAsync("", requestBody);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AniListResponse>();
        return result?.Data?.Media;
    }
}
