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

    public async Task AddAsync(SearchHistoryEntry entry, CancellationToken ct = default)
    {
        _context.SearchHistory.Add(entry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.SearchHistory.CountAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entry = await _context.SearchHistory.FindAsync(id, ct);
        if (entry is not null)
        {
            _context.SearchHistory.Remove(entry);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(string query, CancellationToken ct = default) =>
        await _context.SearchHistory
            .AnyAsync(e => e.Query.ToLower() == query.ToLower(), ct);

    public async Task<IEnumerable<SearchHistoryEntry>> GetAllAsync(CancellationToken ct = default) =>
        await _context.SearchHistory
            .OrderByDescending(e => e.SearchedAt)
            .ToListAsync(ct);

    public async Task<SearchHistoryEntry?> GetOldestAsync(CancellationToken ct = default) =>
        await _context.SearchHistory
            .OrderBy(e => e.SearchedAt)
            .FirstOrDefaultAsync(ct);

    public async Task UpdateDateAsync(int id, CancellationToken ct = default)
    {
        var entry = await _context.SearchHistory.FindAsync(id, ct);
        if (entry is not null)
        {
            entry.SearchedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<SearchHistoryEntry?> GetByQueryAsync(string query, CancellationToken ct = default) =>
        await _context.SearchHistory.FirstOrDefaultAsync(e => e.Query.ToLower() == query.ToLower(), ct);
}
