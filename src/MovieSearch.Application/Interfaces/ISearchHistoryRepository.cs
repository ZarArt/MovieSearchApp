using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task<IEnumerable<SearchHistoryEntry>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(SearchHistoryEntry entry, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
        Task<bool> ExistsAsync(string query, CancellationToken ct = default);
        Task<SearchHistoryEntry?> GetOldestAsync(CancellationToken ct = default);
        Task UpdateDateAsync(int id, CancellationToken ct = default);
        Task<SearchHistoryEntry?> GetByQueryAsync(string query, CancellationToken ct = default);
    }
}
