using Filmograf.BaseLibrary.Util;
using Filmograf.SearchIndexerService.Extendions;
using Filmograf.SearchIndexerService.Util;
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
        builder.Services
            .AddAutoMapper(_ => { }, typeof(Program).Assembly)
            .AddRedis()
            .AddMongoDB()
            .AddComponents()
            .AddElastic();
        
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
}
