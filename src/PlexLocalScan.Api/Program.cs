using System.Reflection;
using PlexLocalScan.Data.Data;
using PlexLocalScan.Shared.Options;
using PlexLocalScan.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlexLocalScan.Shared.Interfaces;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Configure YAML configuration
var configPath = Path.Combine(AppContext.BaseDirectory, "config", "config.yml");
Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

var configDir = Path.GetDirectoryName(configPath) 
    ?? throw new InvalidOperationException("Config directory path cannot be null");

builder.Configuration
    .SetBasePath(configDir)
    .AddYamlFile(Path.GetFileName(configPath), false)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Add services to the container
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

        // Reuse the same services from Console project
        services.Configure<PlexOptions>(builder.Configuration.GetSection("Plex"))
                .Configure<TMDbOptions>(builder.Configuration.GetSection("TMDb"))
                .Configure<MediaDetectionOptions>(builder.Configuration.GetSection("MediaDetection"))
                .AddSingleton<IPlexHandler, PlexHandler>()
                .AddScoped<ISymlinkHandler, SymlinkHandler>()
                .AddScoped<ITMDbClientWrapper>(sp =>
                {
                    var tmdbOptions = sp.GetRequiredService<IOptions<TMDbOptions>>();
                    return new TMDbClientWrapper(tmdbOptions.Value.ApiKey);
                })
                .AddScoped<IMovieDetectionService, MovieDetectionService>()
                .AddScoped<ITvShowDetectionService, TvShowDetectionService>()
                .AddScoped<IMediaDetectionService, MediaDetectionService>()
                .AddScoped<IDateTimeProvider, DateTimeProvider>()
                .AddScoped<IFileSystemService, FileSystemService>()
                .AddScoped<IFileTrackingService, FileTrackingService>()
                .AddHostedService<FileWatcherService>()
                .AddDbContext<PlexScanContext>((serviceProvider, options) =>
                {
                    var databaseOptions = "Data Source=" + Path.Combine(configDir, "plexscan.db");
                    options.UseSqlite(databaseOptions);
                })
                .AddHttpClient()
                .AddMemoryCache()
                .AddScoped<ICleanupHandler, CleanupHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
app.MapScalarApiReference(options =>
{
    options.Theme = ScalarTheme.Mars;
});

app.UseRouting();
//app.UseHttpsRedirection();
//app.UseAuthorization();

// Replace the root path handler with a redirect
app.MapGet("/", () => Results.Redirect("/scalar/v1"));
app.MapControllers();

app.Run();
