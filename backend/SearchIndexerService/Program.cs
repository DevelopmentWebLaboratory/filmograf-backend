using StackExchange.Redis;
using MongoDB.Bson;
using MongoDB.Driver;

using Filmograf.BaseLibrary.Util;
using Filmograf.BaseLibrary.Caching;
using Filmograf.BaseLibrary.DataAccess.Serializers;
using Filmograf.BaseLibrary.Integrations;
using Filmograf.BaseLibrary.Integrations.Requested;
using Filmograf.BaseLibrary.Models.Repo;
using Filmograf.BaseLibrary.Services;
using Filmograf.SearchIndexerService.Util;
using Filmograf.SearchIndexerService.Services;
using Filmograf.SearchIndexerService.Services.Integrations;
using Filmograf.SearchIndexerService.Services.Middlewares;

namespace Filmograf.SearchIndexerService;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        AppSettingsUtil.LoadAppSettingsData();
        LocalAppSettingsUtil.LoadAppSettingsData();
        
        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Add AutoMapper
        builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
        
        SettingUpRedis(builder);
        SettingUpMongoDB(builder);
        SettingRabbitMQ(builder);
        SettingComponents(builder);
        
        var app = builder.Build();
        
        // ловушка для ошибок
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Configure the HTTP request pipeline.
        if (AppSettingsUtil.AppSettings.DevMode)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("AllowAll"); // todo: в проде поменять
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.Run();
    }

    private static void SettingUpRedis(WebApplicationBuilder builder)
    {
        var redisSettings = AppSettingsUtil.AppSettings.RedisSettings;
        Console.WriteLine(redisSettings.Host);
        
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
            ConnectionMultiplexer.Connect($"{redisSettings.Host}:6379,abortConnect=false"));
    }
    
    private static void SettingUpMongoDB(WebApplicationBuilder builder)
    {
        var mongoDbSettings = AppSettingsUtil.AppSettings.MongoDbSettings;
        
        // mongoDB из коробки не понимает что надо хранить Guid в стандартном формате (Standard UUID)
        var guidSerializer = new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard);
        MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(guidSerializer);
        
        // с date only этот еблан тоже не дружит
        var dateOnlySerializer = new DateOnlySerializer();
        MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(dateOnlySerializer);

        builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var client = new MongoClient(mongoDbSettings.ConnectionString);
            return client.GetDatabase(mongoDbSettings.DatabaseName);
        });

        builder.Services.AddHostedService<MongoIndexService>();
    }
    
    private static void SettingRabbitMQ(WebApplicationBuilder builder)
    {
        // rabbitqm hosted service
        builder.Services.AddHostedService<RabbitMqHostedShell>();
        
        // rabbitqm requests service
        builder.Services.AddSingleton<IRabbitMqRequestedService, RabbitMqRequestedServiceShell>();
        
        // integration contexts
        builder.Services.AddScoped<IntegrationContextBase>();
    }

    private static void SettingComponents(WebApplicationBuilder builder)
    {
        // contexts
        // ...
        
        // services
        builder.Services.AddScoped<RedisService>();
        builder.Services.AddScoped<MissionPlannerService>();
        
        // providers
        // ...
        
        // repositories
        builder.Services.AddScoped<MoviesClicksAnalyticRepo>();
        builder.Services.AddScoped<CollectionClicksAnalyticRepo>();
        
        // cache
        builder.Services.AddScoped<MissionPlannerCache>();
    }
}
