using Microsoft.EntityFrameworkCore;
using MovieSearch.Domain.Entities;
using MovieSearch.Infrastructure.Persistence;

namespace MovieSearch.Tests.Repositories;

public class SearchHistoryRepositoryTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AddsEntryToDatabase()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry
        {
            Query = "inception",
            SearchedAt = DateTime.UtcNow
        });

        Assert.Equal(1, await context.SearchHistory.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEntriesOrderedByDateDescending()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "old", SearchedAt = DateTime.UtcNow.AddDays(-2) });
        await repository.AddAsync(new SearchHistoryEntry { Query = "new", SearchedAt = DateTime.UtcNow });

        var result = (await repository.GetAllAsync()).ToList();

        Assert.Equal("new", result.First().Query);
        Assert.Equal("old", result.Last().Query);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntryFromDatabase()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "batman", SearchedAt = DateTime.UtcNow });
        var entry = await context.SearchHistory.FirstAsync();

        await repository.DeleteAsync(entry.Id);

        Assert.Equal(0, await context.SearchHistory.CountAsync());
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "batman", SearchedAt = DateTime.UtcNow });
        await repository.AddAsync(new SearchHistoryEntry { Query = "hobbit", SearchedAt = DateTime.UtcNow });

        var count = await repository.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrueForExistingQuery()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "batman", SearchedAt = DateTime.UtcNow });

        var exists = await repository.ExistsAsync("batman");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_IsCaseInsensitive()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "Batman", SearchedAt = DateTime.UtcNow });

        var exists = await repository.ExistsAsync("batman");

        Assert.True(exists);
    }

    [Fact]
    public async Task GetByQueryAsync_ReturnsCorrectEntry()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "inception", SearchedAt = DateTime.UtcNow });

        var result = await repository.GetByQueryAsync("inception");

        Assert.NotNull(result);
        Assert.Equal("inception", result.Query);
    }

    [Fact]
    public async Task GetByQueryAsync_ReturnsNullForNonExistingQuery()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        var result = await repository.GetByQueryAsync("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOldestAsync_ReturnsOldestEntry()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        await repository.AddAsync(new SearchHistoryEntry { Query = "newest", SearchedAt = DateTime.UtcNow });
        await repository.AddAsync(new SearchHistoryEntry { Query = "oldest", SearchedAt = DateTime.UtcNow.AddDays(-5) });
        await repository.AddAsync(new SearchHistoryEntry { Query = "middle", SearchedAt = DateTime.UtcNow.AddDays(-2) });

        var result = await repository.GetOldestAsync();

        Assert.NotNull(result);
        Assert.Equal("oldest", result.Query);
    }

    [Fact]
    public async Task UpdateDateAsync_UpdatesSearchedAt()
    {
        await using var context = CreateDbContext();
        var repository = new SearchHistoryRepository(context);

        var oldDate = DateTime.UtcNow.AddDays(-1);
        await repository.AddAsync(new SearchHistoryEntry { Query = "batman", SearchedAt = oldDate });
        var entry = await context.SearchHistory.FirstAsync();

        await repository.UpdateDateAsync(entry.Id);

        var updated = await context.SearchHistory.FindAsync(entry.Id);
        Assert.True(updated!.SearchedAt > oldDate);
    }
}