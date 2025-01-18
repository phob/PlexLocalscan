using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PlexLocalScan.Abstractions;
using PlexLocalScan.Api.Config;
using PlexLocalScan.Api.Endpoints;
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
using PlexLocalScan.SignalR.Hubs;
using PlexLocalScan.SignalR.Services;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

using PlexLocalScan.Shared.DbContext.Interfaces;
using PlexLocalScan.Shared.DbContext.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
            .WithOrigins("http://localhost:3000")
            .WithOrigins("http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// Configure YAML configuration
var configPath = Path.Combine(AppContext.BaseDirectory, "config", "config.yml");
Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
Console.WriteLine("configPath: " + configPath);
var configDir = Path.GetDirectoryName(configPath) 
    ?? throw new InvalidOperationException("Config directory path cannot be null");

await ConfigurationHelper.EnsureDefaultConfigAsync(configPath);

builder.Configuration
    .SetBasePath(configDir)
    .AddYamlFile(Path.GetFileName(configPath), false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console());

var services = builder.Services;

// Add SignalR
services.AddSignalR();

// Add services to the container
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
// Add HeartbeatService
services.AddHostedService<HeartbeatService>();

// Reuse the same services from Console project
services.AddOptions()
    .Configure<PlexOptions>(builder.Configuration.GetSection("Plex"))
    .Configure<TmDbOptions>(builder.Configuration.GetSection("TMDb"))
    .Configure<MediaDetectionOptions>(builder.Configuration.GetSection("MediaDetection"))
    .Configure<FolderMappingOptions>(builder.Configuration.GetSection("FolderMapping"))
    .AddSingleton(new YamlConfigurationService(builder.Configuration, configPath))
    .AddSingleton<IPlexHandler, PlexHandler>()
    .AddScoped<INotificationService, NotificationService>()
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
    .AddDbContext<PlexScanContext>((_, options) =>
    {
        var connectionString = $"Data Source={Path.Combine(AppContext.BaseDirectory, "config", "plexscan.db")}";
        options.UseSqlite(connectionString);
    })
    .AddHostedService<FilePollerService>()
    .AddHttpClient()
    .AddMemoryCache();

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlexScanContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            Log.Error(ex, "An unhandled exception occurred");
            await context.Response.WriteAsJsonAsync(new
            {
                error = "An internal server error occurred.",
                details = app.Environment.IsDevelopment() ? ex.Message : null
            });
        }
    }));

// Add middleware in the correct order
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

// Map endpoints
app.MapControllers();
app.MapHub<ContextHub>(ContextHub.Route);

// Register all API endpoints
app.MapApiEndpoints();

app.MapOpenApi();
app.MapScalarApiReference(options => options.Theme = ScalarTheme.Mars);
app.MapGet("/", () => Results.Redirect("/scalar/v1"));

try
{
    Log.Information("Starting PlexLocalScan API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
