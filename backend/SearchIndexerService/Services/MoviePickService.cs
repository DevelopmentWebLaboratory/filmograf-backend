using Filmograf.SearchIndexerService.Caching;

namespace Filmograf.SearchIndexerService.Services;

public class MoviePickService
{
    private readonly PickMoviesCaching _pickMoviesCaching;
    
    public MoviePickService(PickMoviesCaching pickMoviesCaching)
    {
        _pickMoviesCaching = pickMoviesCaching;
    }
    
    public async Task PickMovieAsync(string movieId)
    {
        await _pickMoviesCaching.SetAsync(movieId);
    }

    public async Task<List<string>> PullMoviesIdsAsync()
    {
        return await _pickMoviesCaching.PullMovieIdsAsync();
    }
}