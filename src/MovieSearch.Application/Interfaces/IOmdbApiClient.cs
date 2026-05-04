using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Interfaces;

public interface IOmdbApiClient
{
    Task<IEnumerable<Movie>> SearchMoviesAsync(string query, CancellationToken ct = default);
    Task<Movie?> GetMovieByIdAsync(string imdbId, CancellationToken ct = default);
}
