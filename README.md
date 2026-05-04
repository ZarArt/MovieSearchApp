# MovieSearch

A web application for searching movies using the [OMDb API](http://www.omdbapi.com), built with .NET 10 and Blazor Server.

## Features

- Search movies by title
- View detailed movie information (poster, plot, IMDb rating, genre, director, actors, runtime)
- Search history of the last 5 unique queries, persisted between application restarts
- In-memory caching for repeated searches
- Structured logging with Serilog

## Tech Stack

- **Framework**: ASP.NET Core (.NET 10) with Blazor Server
- **Database**: SQLite via Entity Framework Core
- **Logging**: Serilog
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
cd MovieSearchApp
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

5. Open your browser at `http://localhost:5009`

> **Note on HTTPS**: If you want to use HTTPS, trust the development certificate first:
> ```bash
> dotnet dev-certs https --trust
> ```
> Then access the app at `https://localhost:7070`. HTTP works out of the box without any setup.

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

**On the "Backend" requirement** — the assignment requires a separate frontend (Blazor) and backend (.NET). With Blazor Server, there is no separate backend in the traditional sense (e.g. a Web API with controllers). ASP.NET Core acts as the host, and all application logic runs server-side. The layered architecture (Application + Domain + Infrastructure) fulfills the separation of concerns requirement. Had Blazor WebAssembly been chosen, a separate ASP.NET Core Web API would have been required as a backend proxy.

**Search history behaviour** — all unique queries are stored in history regardless of whether they returned results. The requirement states "storing the history of the last five unique search queries" without specifying successful results only. Storing only successful queries could be a UX improvement worth discussing.

**Caching** — search results and movie details are cached in memory for 30 minutes using `IMemoryCache`. This reduces redundant API calls, especially when navigating history.

**CancellationToken** — all async methods accept `CancellationToken` to support proper cancellation propagation throughout the application.

**Logging** — Serilog is configured to write to both console and rolling daily log files in `src/MovieSearch.Web/logs/`. Log files are excluded from the repository via `.gitignore`.

## Tests

The test project covers:

- **`MovieSearchService`** — search and history-saving business logic (empty queries, duplicates, full history eviction, query trimming).
- **`SearchHistoryRepository`** — database operations against an in-memory EF Core provider (CRUD, case-insensitive lookups, ordering).
- **`OmdbApiClient`** — HTTP client behaviour with mocked `HttpMessageHandler` (successful responses, no results, server errors).
