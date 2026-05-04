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

        // Note: we store all unique queries regardless of search results,
        // as the requirement states "storing the history of the last five unique search queries"
        // without specifying successful results only.
        // Storing only successful queries could be a UX improvement worth discussing.
        await SaveToHistoryAsync(query.Trim());

        return await _apiClient.SearchMoviesAsync(query.Trim());
    }

    public async Task<Movie?> GetDetailsAsync(string imdbId)
    {
        return await _apiClient.GetMovieByIdAsync(imdbId);
    }

    private async Task SaveToHistoryAsync(string query)
    {
        var duplicate = await _historyRepository.GetByQueryAsync(query);

        if (duplicate is not null)
        {
            await _historyRepository.UpdateDateAsync(duplicate.Id);
            return;
        }

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