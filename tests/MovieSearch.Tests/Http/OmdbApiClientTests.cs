using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MovieSearch.Infrastructure.Http;

namespace MovieSearch.Tests.Http;

public class OmdbApiClientTests
{
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    private OmdbApiClient CreateClient(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.omdbapi.com/")
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Omdb:ApiKey", "test-api-key" }
            })
            .Build();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<OmdbApiClient>>().Object;

        return new OmdbApiClient(httpClient, configuration, cache, logger);
    }

    [Fact]
    public async Task SearchMoviesAsync_ValidResponse_ReturnsMovies()
    {
        var json = JsonSerializer.Serialize(new
        {
            Search = new[]
            {
                new { imdbID = "tt1375666", Title = "Inception", Year = "2010", Poster = "N/A" },
                new { imdbID = "tt0816692", Title = "Interstellar", Year = "2014", Poster = "N/A" }
            },
            totalResults = "2",
            Response = "True"
        });

        var client = CreateClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var result = await client.SearchMoviesAsync("inception", Ct);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, m => m.Title == "Inception");
    }

    [Fact]
    public async Task SearchMoviesAsync_NoResults_ReturnsEmptyList()
    {
        var json = JsonSerializer.Serialize(new
        {
            Response = "False",
            Error = "Movie not found!"
        });

        var client = CreateClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var result = await client.SearchMoviesAsync("xyzxyzxyz", Ct);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMovieByIdAsync_ValidResponse_ReturnsMovie()
    {
        var json = JsonSerializer.Serialize(new
        {
            imdbID = "tt1375666",
            Title = "Inception",
            Year = "2010",
            Poster = "N/A",
            Plot = "A thief who steals corporate secrets.",
            imdbRating = "8.8",
            Genre = "Action, Sci-Fi",
            Director = "Christopher Nolan",
            Actors = "Leonardo DiCaprio",
            Runtime = "148 min",
            Response = "True"
        });

        var client = CreateClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var result = await client.GetMovieByIdAsync("tt1375666", Ct);

        Assert.NotNull(result);
        Assert.Equal("Inception", result.Title);
        Assert.Equal("8.8", result.ImdbRating);
        Assert.Equal("Christopher Nolan", result.Director);
    }

    [Fact]
    public async Task GetMovieByIdAsync_InvalidId_ReturnsNull()
    {
        var json = JsonSerializer.Serialize(new
        {
            Response = "False",
            Error = "Incorrect IMDb ID."
        });

        var client = CreateClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json)
        });

        var result = await client.GetMovieByIdAsync("invalid-id", Ct);

        Assert.Null(result);
    }

    [Fact]
    public async Task SearchMoviesAsync_ServerError_ThrowsException()
    {
        var client = CreateClient(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.InternalServerError
        });

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.SearchMoviesAsync("inception", Ct));
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}