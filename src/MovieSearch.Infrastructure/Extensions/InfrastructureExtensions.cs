using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieSearch.Application.Interfaces;
using MovieSearch.Application.Services;
using MovieSearch.Infrastructure.Http;
using MovieSearch.Infrastructure.Persistence;

namespace MovieSearch.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();

        services.AddHttpClient<IOmdbApiClient, OmdbApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Omdb:BaseUrl"]
                ?? throw new InvalidOperationException("OMDb BaseUrl is not configured"));
        });

        services.AddScoped<MovieSearchService>();

        return services;
    }
}