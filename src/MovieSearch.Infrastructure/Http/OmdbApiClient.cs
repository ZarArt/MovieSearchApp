using Microsoft.Extensions.Configuration;
using MovieSearch.Application.Interfaces;
using MovieSearch.Domain.Entities;
using MovieSearch.Infrastructure.Responses;
using MovieSearch.Infrastructure.DTO;
using System.Text.Json;

namespace MovieSearch.Infrastructure.Http;

public class OmdbApiClient : IOmdbApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OmdbApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Omdb:ApiKey"]
            ?? throw new InvalidOperationException("OMDb API key is not configured");
    }

    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query)
    {
        var url = $"?s={Uri.EscapeDataString(query)}&type=movie&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OmdbSearchResponse>(json);

        if (result?.Response != "True")
            return Enumerable.Empty<Movie>();

        return result.Search ?? Enumerable.Empty<Movie>();
    }

    public async Task<Movie?> GetMovieByIdAsync(string imdbId)
    {
        var url = $"?i={imdbId}&plot=full&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OmdbMovieResponse>(json);

        if (result?.Response != "True")
            return null;

        return result.ToMovie();
    }
}

// OMDb повертає список фільмів у полі "Search"
//internal class OmdbSearchResponse
//{
//    [JsonPropertyName("Search")]
//    public List<Movie>? Search { get; set; }
//}