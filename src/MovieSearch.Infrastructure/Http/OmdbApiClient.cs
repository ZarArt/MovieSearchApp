using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieSearch.Application.Interfaces;
using MovieSearch.Domain.Entities;
using MovieSearch.Infrastructure.DTO;
using MovieSearch.Infrastructure.Responses;
using System.Text.Json;

namespace MovieSearch.Infrastructure.Http;

public class OmdbApiClient : IOmdbApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OmdbApiClient> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public OmdbApiClient(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache,
        ILogger<OmdbApiClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _apiKey = configuration["Omdb:ApiKey"]
            ?? throw new InvalidOperationException("OMDb API key is not configured");
    }

    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query, CancellationToken ct = default)
    {
        var cacheKey = $"search:{query.ToLower()}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<Movie>? cached))
        {
            _logger.LogInformation("Cache hit for search query: {Query}", query);
            return cached!;
        }

        _logger.LogInformation("Fetching search results from OMDb for query: {Query}", query);

        var url = $"?s={Uri.EscapeDataString(query)}&type=movie&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OmdbSearchResponse>(json);

        if (result?.Response != "True")
        {
            _logger.LogWarning("OMDb returned no results for query: {Query}", query);
            return Enumerable.Empty<Movie>();
        }

        var movies = result.Search ?? Enumerable.Empty<Movie>();
        _cache.Set(cacheKey, movies, CacheDuration);

        return movies;
    }

    public async Task<Movie?> GetMovieByIdAsync(string imdbId, CancellationToken ct = default)
    {
        var cacheKey = $"movie:{imdbId}";

        if (_cache.TryGetValue(cacheKey, out Movie? cached))
        {
            _logger.LogInformation("Cache hit for movie: {ImdbId}", imdbId);
            return cached;
        }

        _logger.LogInformation("Fetching movie details from OMDb for: {ImdbId}", imdbId);

        var url = $"?i={imdbId}&plot=full&apikey={_apiKey}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OmdbMovieResponse>(json);

        if (result?.Response != "True")
        {
            _logger.LogWarning("OMDb returned no details for movie: {ImdbId}", imdbId);
            return null;
        }

        var movie = result.ToMovie();
        _cache.Set(cacheKey, movie, CacheDuration);

        return movie;
    }
}