# MovieSearch

A web application for searching movies using the [OMDb API](http://www.omdbapi.com), built with .NET 10 and Blazor Server.

## Features

- Search movies by title
- View detailed movie information (poster, plot, IMDb rating, genre, director, actors, runtime)
- Search history of the last 5 unique queries, persisted between application restarts
- In-memory caching for repeated searches
- Structured logging with Serilog

## Tech Stack

- **Frontend**: Blazor Server (.NET 10)
- **Backend**: ASP.NET Core (.NET 10)
- **Database**: SQLite via Entity Framework Core
- **Logging**: Serilog (Console + File)
- **Testing**: xUnit + Moq

## Project Structure

```
MovieSearch/
├── src/
│   ├── MovieSearch.Web/             # Blazor Server UI
│   ├── MovieSearch.Application/     # Business logic, interfaces
│   ├── MovieSearch.Domain/          # Domain models
│   └── MovieSearch.Infrastructure/  # EF Core, SQLite, OMDb HTTP client
└── tests/
    └── MovieSearch.Tests/           # Unit tests (xUnit + Moq)
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- OMDb API key — get a free key at [omdbapi.com](http://www.omdbapi.com/apikey.aspx)

### Setup

1. Clone the repository:
```bash
git clone https://github.com/ZarArt/MovieSearchApp.git
cd MovieSearch
```

2. Get a free OMDb API key at [omdbapi.com](http://www.omdbapi.com/apikey.aspx).

3. Add your API key using .NET user secrets (recommended for local development):
```bash
dotnet user-secrets set "Omdb:ApiKey" "your_api_key_here" --project src/MovieSearch.Web
```

   Alternatively, you can edit `src/MovieSearch.Web/appsettings.json` directly:
```json
{
  "Omdb": {
    "ApiKey": "your_api_key_here"
  }
}
```

4. Run the application:
```bash
dotnet run --project src/MovieSearch.Web
```

5. Open your browser at `https://localhost:5001`

### Running Tests

```bash
dotnet test
```

## Architecture

The solution follows a clean layered architecture with clear separation of concerns:

- **Domain** — core models (`Movie`, `SearchHistoryEntry`). No dependencies on other layers.
- **Application** — business logic and interfaces (`MovieSearchService`, `ISearchHistoryRepository`, `IOmdbApiClient`). Depends only on Domain.
- **Infrastructure** — implementations of Application interfaces. Contains EF Core DbContext, SQLite repository, and OMDb HTTP client. Depends on Application and Domain.
- **Web** — Blazor Server UI. Depends on Application (via interfaces) and Infrastructure (for DI registration only).

## Technical Notes

**Blazor Server over Blazor WebAssembly** — Blazor Server was chosen because the application uses a local SQLite database and an API key, both of which are server-side concerns. Blazor WebAssembly would require an additional ASP.NET Core Web API layer acting as a proxy, adding unnecessary complexity for this use case.

**Search history behaviour** — all unique queries are stored in history regardless of whether they returned results. The requirement states "storing the history of the last five unique search queries" without specifying successful results only. Storing only successful queries could be a UX improvement worth discussing.

**Caching** — search results and movie details are cached in memory for 30 minutes using `IMemoryCache`. This reduces redundant API calls, especially when navigating history.

**CancellationToken** — all async methods accept `CancellationToken` to support proper cancellation propagation throughout the application.
