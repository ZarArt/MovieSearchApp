using MovieSearch.Application.Interfaces;
using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Services;

public class MovieSearchService(IOmdbApiClient apiClient, ISearchHistoryRepository historyRepository)
{
    private readonly IOmdbApiClient _apiClient = apiClient;
    private readonly ISearchHistoryRepository _historyRepository = historyRepository;

    public async Task<IEnumerable<Movie>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<Movie>();

        await SaveToHistoryAsync(query.Trim());

        return await _apiClient.SearchMoviesAsync(query.Trim());
    }

    public async Task<Movie?> GetDetailsAsync(string imdbId)
    {
        return await _apiClient.GetMovieByIdAsync(imdbId);
    }

    private async Task SaveToHistoryAsync(string query)
    {
        if (await _historyRepository.ExistsAsync(query))
            return;

        if (await _historyRepository.CountAsync() >= 5)
        {
            var oldest = await _historyRepository.GetOldestAsync();
            await _historyRepository.DeleteAsync(oldest!.Id);
        }

        await _historyRepository.AddAsync(new SearchHistoryEntry
        {
            Query = query,
            SearchedAt = DateTime.UtcNow
        });
    }
}