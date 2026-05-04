using Moq;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Services;
using MovieSearch.Domain.Entities;

namespace MovieSearch.Tests.Services;

public class MovieSearchServiceTests
{
    private readonly Mock<IOmdbApiClient> _apiClientMock;
    private readonly Mock<ISearchHistoryRepository> _historyRepositoryMock;
    private readonly MovieSearchService _sut;

    public MovieSearchServiceTests()
    {
        _apiClientMock = new Mock<IOmdbApiClient>();
        _historyRepositoryMock = new Mock<ISearchHistoryRepository>();
        _sut = new MovieSearchService(_apiClientMock.Object, _historyRepositoryMock.Object);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmptyList()
    {
        var result = await _sut.SearchAsync("   ");

        Assert.Empty(result);
        _apiClientMock.Verify(x => x.SearchMoviesAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_ReturnsMovies()
    {
        var movies = new List<Movie> { new Movie { Title = "Inception" } };
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync("inception"))
            .ReturnsAsync(movies);

        var result = await _sut.SearchAsync("inception");

        Assert.Single(result);
        Assert.Equal("Inception", result.First().Title);
    }

    [Fact]
    public async Task SearchAsync_ValidQuery_SavesToHistory()
    {
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync())
            .ReturnsAsync(0);

        await _sut.SearchAsync("batman");

        _historyRepositoryMock.Verify(
            x => x.AddAsync(It.Is<SearchHistoryEntry>(e => e.Query == "batman")),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_DuplicateQuery_UpdatesDateInsteadOfAdding()
    {
        var existing = new SearchHistoryEntry { Id = 1, Query = "batman", SearchedAt = DateTime.UtcNow };

        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync("batman"))
            .ReturnsAsync(existing);

        await _sut.SearchAsync("batman");

        _historyRepositoryMock.Verify(x => x.UpdateDateAsync(1), Times.Once);
        _historyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchHistoryEntry>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_HistoryFull_DeletesOldestBeforeAdding()
    {
        var oldest = new SearchHistoryEntry { Id = 99, Query = "old", SearchedAt = DateTime.UtcNow };

        _apiClientMock
            .Setup(x => x.SearchMoviesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync())
            .ReturnsAsync(5);

        _historyRepositoryMock
            .Setup(x => x.GetOldestAsync())
            .ReturnsAsync(oldest);

        await _sut.SearchAsync("newquery");

        _historyRepositoryMock.Verify(x => x.DeleteAsync(99), Times.Once);
        _historyRepositoryMock.Verify(x => x.AddAsync(It.IsAny<SearchHistoryEntry>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_TrimsQuery_BeforeSaving()
    {
        _apiClientMock
            .Setup(x => x.SearchMoviesAsync("batman"))
            .ReturnsAsync(new List<Movie>());

        _historyRepositoryMock
            .Setup(x => x.GetByQueryAsync(It.IsAny<string>()))
            .ReturnsAsync((SearchHistoryEntry?)null);

        _historyRepositoryMock
            .Setup(x => x.CountAsync())
            .ReturnsAsync(0);

        await _sut.SearchAsync("  batman  ");

        _historyRepositoryMock.Verify(
            x => x.AddAsync(It.Is<SearchHistoryEntry>(e => e.Query == "batman")),
            Times.Once);
    }
}
