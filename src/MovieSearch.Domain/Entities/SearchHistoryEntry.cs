namespace MovieSearch.Domain.Entities;

public class SearchHistoryEntry
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
}
