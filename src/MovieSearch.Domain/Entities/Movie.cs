using System.Text.Json.Serialization;

namespace MovieSearch.Domain.Entities;

public class Movie
{
    [JsonPropertyName("imdbID")]
    public string ImdbId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Year { get; set; } = string.Empty;

    public string Poster { get; set; } = string.Empty;

    public string Plot { get; set; } = string.Empty;

    [JsonPropertyName("imdbRating")]
    public string ImdbRating { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public string Director { get; set; } = string.Empty;

    public string Actors { get; set; } = string.Empty;

    public string Runtime { get; set; } = string.Empty;
}