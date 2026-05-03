using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Filmograf.BaseLibrary.DataAccess.Repositories;
using Filmograf.BaseLibrary.Models.SearchIndexes;

namespace Filmograf.SearchIndexerService.Services;

public class SearchIndexService
{
    private readonly IMapper _mapper;
    private readonly ElasticsearchClient _elasticsearch;
    private readonly MovieRepository _movieRepository; // scoped
    
    public SearchIndexService(IMapper mapper, ElasticsearchClient elasticsearch, MovieRepository movieRepository)
    {
        _mapper = mapper;
        _elasticsearch = elasticsearch;
        _movieRepository = movieRepository;
    }

    public async Task ReindexAllMoviesAsync(int batchSize = 1000, CancellationToken ct = default)
    {
        using var cursor = await _movieRepository.GetCursorAsync(ct);

        while (await cursor.MoveNextAsync(cancellationToken: ct))
        {
            var mongoMovies = cursor.Current;
            if (!mongoMovies.Any()) continue;

            // маппим
            var searchIndexes = _mapper.Map<MovieSearchIndex[]>(mongoMovies);

            // отправляем в Elastic
            var bulkResponse = await _elasticsearch.BulkAsync(b => b
                .Index("movies")
                .UpdateMany(searchIndexes, (descriptor, movie) => descriptor
                    .Doc(movie)
                    .DocAsUpsert(true) 
                ),
                cancellationToken: ct
            );

            // todo: логгирование настрой блять заебал уже
            if (bulkResponse.IsSuccess())
                Console.WriteLine($"Обработано {searchIndexes.Length} фильмов.");
            else
                Console.WriteLine($"Ошибка батча: {bulkResponse.DebugInformation}");
        }
    }
}