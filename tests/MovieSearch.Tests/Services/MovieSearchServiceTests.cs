using Microsoft.Extensions.Logging;
using Moq;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Services;
using MovieSearch.Domain.Entities;

namespace MovieSearch.Tests.Services;

public class MovieSearchServiceTests
{
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly Mock<IOmdbApiClient> _apiClientMock;
    private readonly Mock<ISearchHistoryRepository> _historyRepositoryMock;
    private readonly Mock<ILogger<MovieSearchService>> _loggerMock;
    private readonly MovieSearchService _sut;

    public MovieSearchServiceTests()
    {
        _apiClientMock = new Mock<IOmdbApiClient>();
        _historyRepositoryMock = new Mock<ISearchHistoryRepository>();
        _loggerMock = new Mock<ILogger<MovieSearchService>>();
        _sut = new MovieSearchService(_apiClientMock.Object, _historyRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmptyList()
    {
        var result = await _sut.SearchAsync("   ", Ct);

        Assert.Empty(result);
        _apiClientMock.Verify(x => x.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_ReturnsMovies()
    {
        var movies = new List<Movie> { new Movie { Title = "Inception" } };
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync("inception", It.IsAny<CancellationToken>()))
            .ReturnsAsync(movies);

        var result = await _sut.SearchAsync("inception", Ct);

        Assert.Single(result);
        Assert.Equal("Inception", result.First().Title);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_SavesToHistory()
    {
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await _sut.SearchAsync("batman", Ct);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(It.Is<SearchHistoryEntry>(e => e.Query == "batman"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_DuplicateQuery_UpdatesDateInsteadOfAdding()
    {
        var existing = new SearchHistoryEntry { Id = 1, Query = "batman", SearchedAt = DateTime.UtcNow };

        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync("batman", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.SearchAsync("batman", Ct);

        _historyRepositoryMock.Verify(x => x.UpdateDateAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchHistoryEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_HistoryFull_DeletesOldestBeforeAdding()
    {
        var oldest = new SearchHistoryEntry { Id = 99, Query = "old", SearchedAt = DateTime.UtcNow };

        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _historyRepositoryMock
            .Setup(x => x.GetOldestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldest);

        await _sut.SearchAsync("newquery", Ct);

        _historyRepositoryMock.Verify(x => x.DeleteAsync(99, It.IsAny<CancellationToken>()), Times.Once);
        _historyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchHistoryEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_TrimsQuery_BeforeSaving()
    {
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync("batman", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await _sut.SearchAsync("  batman  ", Ct);

        _historyRepositoryMock.Verify(
            x => x.AddAsync(It.Is<SearchHistoryEntry>(e => e.Query == "batman"), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
