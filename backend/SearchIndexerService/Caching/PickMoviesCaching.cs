using Filmograf.BaseLibrary.Caching;
using Filmograf.SearchIndexerService.Models.Types;
using StackExchange.Redis;

namespace Filmograf.SearchIndexerService.Caching;

public class PickMoviesCaching
{
    protected readonly CachingProviderAtomic<PickMovie> _cachingAtomic;
    protected readonly IConnectionMultiplexer _redis;
    protected readonly string _baseKey = "movie-index-picks";

    public PickMoviesCaching(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _cachingAtomic = new CachingProviderAtomic<PickMovie>(redis, $"{_baseKey}");
    }

    private string MakeKey(string movieId)
    {
        return _cachingAtomic.MakeIdKey(movieId);
    }

    public async Task<PickMovie?> GetOrDefaultAsync(string movieId)
    {
        var key = MakeKey(movieId);
        return await _cachingAtomic.GetOrDefaultAsync(key);
    }

    public async Task SetAsync(string movieId)
    {
        var key = MakeKey(movieId);
        var pick = new PickMovie();
        await _cachingAtomic.CreateAsync(key, pick);
    }

    public async Task<bool> DeleteAsync(string movieId)
    {
        var key = MakeKey(movieId);
        return await _cachingAtomic.RemoveAsync(key);
    }

    public async Task<long> DeleteByRootAsync()
    {
        return await _cachingAtomic.RemoveByRootAsync();
    }
    
    public async Task<List<string>> PullMovieIdsAsync()
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints[0]);
        var db = _redis.GetDatabase();
    
        var pattern = $"{_baseKey}:*";
        var keys = new List<RedisKey>();
        var movieIds = new List<string>();

        // находим все ключи по паттерну
        await foreach (var key in server.KeysAsync(db.Database, pattern))
        {
            keys.Add(key);
        
            // извлекаем ID. если ключ "movie-index-picks:{XXX}", 
            // то берем всё, что после последнего двоеточия.
            var keyString = key.ToString();
            var id = keyString.Substring(keyString.LastIndexOf(':') + 1);
            movieIds.Add(id);
        }

        if (keys.Count > 0)
        {
            // удаляем именно те ключи, которые мы прочитали
            // это важно, ибо если за время обработки прилетел новый Pick, 
            // мы его не удалим, так как его ID не было в списке keys.
            await db.KeyDeleteAsync(keys.ToArray());
        }

        return movieIds;
    }
}