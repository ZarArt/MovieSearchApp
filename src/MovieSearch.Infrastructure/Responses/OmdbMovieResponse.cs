using MovieSearch.Domain.Entities;
using System.Text.Json.Serialization;

namespace MovieSearch.Infrastructure.DTO;

internal class OmdbMovieResponse
{
    [JsonPropertyName("imdbID")]
    public string? ImdbId { get; set; }
    public string? Title { get; set; }
    public string? Year { get; set; }
    public string? Poster { get; set; }
    public string? Plot { get; set; }

    [JsonPropertyName("imdbRating")]
    public string? ImdbRating { get; set; }

    public string? Genre { get; set; }
    public string? Director { get; set; }
    public string? Actors { get; set; }
    public string? Runtime { get; set; }
    public string? Response { get; set; }

    public Movie ToMovie() => new Movie
    {
        ImdbId = ImdbId ?? string.Empty,
        Title = Title ?? string.Empty,
        Year = Year ?? string.Empty,
        Poster = Poster ?? string.Empty,
        Plot = Plot ?? string.Empty,
        ImdbRating = ImdbRating ?? string.Empty,
        Genre = Genre ?? string.Empty,
        Director = Director ?? string.Empty,
        Actors = Actors ?? string.Empty,
        Runtime = Runtime ?? string.Empty
    };
}