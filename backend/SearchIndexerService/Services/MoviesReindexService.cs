namespace Filmograf.SearchIndexerService.Services;

public class MoviesReindexService
{
    private readonly MoviePickService _moviePickService;
    private readonly MovieSearchIndexService _movieSearchIndexService;
    
    public MoviesReindexService(MoviePickService moviePickService, MovieSearchIndexService movieSearchIndexService)
    {
        _moviePickService = moviePickService;
        _movieSearchIndexService = movieSearchIndexService;
    }

    public async Task ReindexPickedMoviesAsync(CancellationToken ct)
    {
        // достаем фильмы которые необходимо заново проиндексировать
        var movieIds = await _moviePickService.PullMoviesIdsAsync();
            
        // индексируем фильмы по ids
        await _movieSearchIndexService.ReindexMoviesByIdsAsync(movieIds, ct);
    }
}