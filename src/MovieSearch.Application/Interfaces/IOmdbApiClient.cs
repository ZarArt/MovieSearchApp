using MovieSearch.Domain.Entities;

namespace MovieSearch.Application.Interfaces;

public interface IOmdbApiClient
{
    Task<IEnumerable<Movie>> SearchMoviesAsync(string query);
    Task<Movie?> GetMovieByIdAsync(string imdbId);
}
