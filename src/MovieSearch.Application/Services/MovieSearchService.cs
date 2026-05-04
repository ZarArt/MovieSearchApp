using Microsoft.Extensions.Logging;
using MovieSearch.Application.Interfaces;
using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Services;

public class MovieSearchService(IOmdbApiClient apiClient, ISearchHistoryRepository historyRepository, ILogger<MovieSearchService> logger)
{
    private readonly IOmdbApiClient _apiClient = apiClient;
    private readonly ISearchHistoryRepository _historyRepository = historyRepository;
    private readonly ILogger<MovieSearchService> _logger = logger;

    public async Task<IEnumerable<Movie>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Movie>();

        _logger.LogInformation("Searching movies for query: {Query}", query);

        // Note: we store all unique queries regardless of search results,
        // as the requirement states "storing the history of the last five unique search queries"
        // without specifying successful results only.
        // Storing only successful queries could be a UX improvement worth discussing.
        await SaveToHistoryAsync(query.Trim(), ct);

        return await _apiClient.SearchMoviesAsync(query.Trim(), ct);
    }

    public async Task<Movie?> GetDetailsAsync(string imdbId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching details for movie: {ImdbId}", imdbId);
        return await _apiClient.GetMovieByIdAsync(imdbId, ct);
    }

    private async Task SaveToHistoryAsync(string query, CancellationToken ct = default)
    {
        var duplicate = await _historyRepository.GetByQueryAsync(query, ct);

        if (duplicate is not null)
        {
            _logger.LogInformation("Query already exists in history, updating date: {Query}", query);
            await _historyRepository.UpdateDateAsync(duplicate.Id, ct);
            return;
        }

        if (await _historyRepository.CountAsync(ct) >= 5)
        {
            var oldest = await _historyRepository.GetOldestAsync(ct);
            if (oldest is not null)
            {
                _logger.LogInformation("History full, removing oldest entry: {Query}", oldest.Query);
                await _historyRepository.DeleteAsync(oldest.Id, ct);
            }
        }

        await _historyRepository.AddAsync(new SearchHistoryEntry
        {
            Query = query,
            SearchedAt = DateTime.UtcNow
        });
    }
}