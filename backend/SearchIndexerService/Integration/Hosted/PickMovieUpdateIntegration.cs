using Filmograf.BaseLibrary.Integrations;
using Filmograf.BaseLibrary.Integrations.Payload;
using Filmograf.SearchIndexerService.Services;
using RabbitMQ.Client;

namespace Filmograf.SearchIndexerService.Integration.Hosted;

public class PickMovieUpdateIntegrationRequestPayload : IntegrationRequestPayloadBase
{
    public string MovieId { get; set; }
}

public class PickMovieUpdateIntegrationContext : IntegrationContextBase
{
    public MoviePickService MoviePickService { get; set; }
}

public class PickMovieUpdateIntegration : NoAskIntegrationBase<PickMovieUpdateIntegrationRequestPayload, PickMovieUpdateIntegrationContext>
{
    public PickMovieUpdateIntegration(IChannel channel, string actionName) : base(channel, actionName)
    {
    }

    protected override Task ProcessingAsync(IntegrationRequest request, PickMovieUpdateIntegrationRequestPayload? payload,
        PickMovieUpdateIntegrationContext context)
    {
        if (payload == null) return Task.CompletedTask;
        
        var movieId = payload.MovieId;
        return context.MoviePickService.PickMovieAsync(movieId);
    }
}