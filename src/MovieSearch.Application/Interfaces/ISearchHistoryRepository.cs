using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task<IEnumerable<SearchHistoryEntry>> GetAllAsync();
        Task AddAsync(SearchHistoryEntry entry);
        Task DeleteAsync(int id);
        Task<int> CountAsync();
        Task<bool> ExistsAsync(string query);
        Task<SearchHistoryEntry?> GetOldestAsync();
    }
}
