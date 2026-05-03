using Filmograf.BaseLibrary.Caching;
using Filmograf.BaseLibrary.DataAccess.Repositories;
using Filmograf.BaseLibrary.Services;
using Filmograf.SearchIndexerService.Services;

namespace Filmograf.SearchIndexerService.Extendions;

internal static class ComponentsExtension
{
    public static IServiceCollection AddComponents(this IServiceCollection services)
    {
        // contexts
        // ...
        
        // services
        services.AddScoped<RedisService>();
        services.AddScoped<MissionPlannerService>();
        services.AddScoped<SearchIndexService>();
        
        // providers
        // ...
        
        // repositories
        services.AddScoped<MovieRepository>();
        services.AddScoped<CollectionRepository>();
        services.AddScoped<MoviesClicksAnalyticRepository>();
        services.AddScoped<CollectionsClicksAnalyticRepository>();
        
        // cache
        services.AddScoped<MissionPlannerCache>();

        return services;
    }
}