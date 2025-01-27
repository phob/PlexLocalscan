using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlexLocalScan.Abstractions;
using PlexLocalScan.Api.Config;
using PlexLocalScan.Data.Data;
using PlexLocalScan.Shared.Configuration.Options;
using PlexLocalScan.Shared.MediaDetection.Interfaces;
using PlexLocalScan.Shared.MediaDetection.Services;
using PlexLocalScan.Shared.Plex.Interfaces;
using PlexLocalScan.Shared.Plex.Services;
using PlexLocalScan.Shared.Services;
using PlexLocalScan.Shared.Symlinks.Interfaces;
using PlexLocalScan.Shared.Symlinks.Services;
using PlexLocalScan.Shared.TmDbMediaSearch.Interfaces;
using PlexLocalScan.Shared.TmDbMediaSearch.Services;
using PlexLocalScan.SignalR.Services;
using PlexLocalScan.Shared.DbContext.Interfaces;
using PlexLocalScan.Shared.DbContext.Services;
using System.Text.Json.Serialization;

namespace PlexLocalScan.Api.ServiceCollection;

public static class Application
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add controllers with JSON options
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.WriteIndented = false;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Add SignalR
        services.AddSignalR();

        // Add HeartbeatService
        services.AddHostedService<HeartbeatService>();

        // Configure core services
        services.AddOptions()
            .Configure<PlexOptions>(configuration.GetSection("Plex"))
            .Configure<TmDbOptions>(configuration.GetSection("TMDb"))
            .Configure<MediaDetectionOptions>(configuration.GetSection("MediaDetection"))
            .Configure<FolderMappingOptions>(configuration.GetSection("FolderMapping"));

        // Add singleton services
        services.AddSingleton(new YamlConfigurationService(configuration, Path.Combine(AppContext.BaseDirectory, "config", "config.yml")));

        // Add scoped services
        services.AddScoped<INotificationService, NotificationService>()
            .AddScoped<ISymlinkHandler, SymlinkHandler>()
            .AddScoped<ITmDbClientWrapper>(sp =>
            {
                var tmdbOptions = sp.GetRequiredService<IOptionsSnapshot<TmDbOptions>>();
                return new TmDbClientWrapper(tmdbOptions.Value.ApiKey);
            })
            .AddScoped<IMovieDetectionService, MovieDetectionService>()
            .AddScoped<ITvShowDetectionService, TvShowDetectionService>()
            .AddScoped<IMediaDetectionService, MediaDetectionService>()
            .AddScoped<IMediaSearchService, MediaSearchService>()
            .AddScoped<ICleanupHandler, CleanupHandler>()
            .AddScoped<ISymlinkRecreationService, SymlinkRecreationService>()
            .AddScoped<IContextService, ContextService>()
            .AddScoped<IFileProcessing, FileProcessing>()
            .AddScoped<IPlexHandler, PlexHandler>();

        // Add database context
        services.AddDbContext<PlexScanContext>((_, options) =>
        {
            var connectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "config", "plexscan.db")}";
            options.UseSqlite(connectionString);
        });

        // Add hosted services
        services.AddHostedService<FilePollerService>();

        // Add additional services
        services.AddHttpClient()
            .AddMemoryCache();

        return services;
    }
}
