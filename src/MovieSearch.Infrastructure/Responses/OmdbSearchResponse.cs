using MovieSearch.Domain.Entities;
using System.Text.Json.Serialization;

namespace MovieSearch.Infrastructure.Responses;

internal class OmdbSearchResponse
{
    public List<Movie>? Search { get; set; }

    [JsonPropertyName("totalResults")]
    public string? TotalResults { get; set; }

    public string? Response { get; set; }
}
