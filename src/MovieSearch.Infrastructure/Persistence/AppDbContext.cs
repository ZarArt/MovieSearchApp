using Microsoft.EntityFrameworkCore;
using MovieSearch.Domain.Entities;

namespace MovieSearch.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SearchHistoryEntry> SearchHistory => Set<SearchHistoryEntry>();
}
