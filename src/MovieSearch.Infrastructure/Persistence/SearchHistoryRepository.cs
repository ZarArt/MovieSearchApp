using MovieSearch.Application.Interfaces;
using MovieSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MovieSearch.Infrastructure.Persistence;

public class SearchHistoryRepository : ISearchHistoryRepository
{
    private readonly AppDbContext _context;

    public SearchHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SearchHistoryEntry entry)
    {
        _context.SearchHistory.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.SearchHistory.CountAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await _context.SearchHistory.FindAsync(id);
        if (entry is not null)
        {
            _context.SearchHistory.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string query) =>
        await _context.SearchHistory
            .AnyAsync(e => e.Query.ToLower() == query.ToLower());

    public async Task<IEnumerable<SearchHistoryEntry>> GetAllAsync() =>
        await _context.SearchHistory
            .OrderByDescending(e => e.SearchedAt)
            .ToListAsync();

    public async Task<SearchHistoryEntry?> GetOldestAsync() =>
        await _context.SearchHistory
            .OrderBy(e => e.SearchedAt)
            .FirstOrDefaultAsync();

    public async Task UpdateDateAsync(int id)
    {
        var entry = await _context.SearchHistory.FindAsync(id);
        if (entry is not null)
        {
            entry.SearchedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<SearchHistoryEntry?> GetByQueryAsync(string query) =>
        await _context.SearchHistory.FirstOrDefaultAsync(e => e.Query.ToLower() == query.ToLower());
}
