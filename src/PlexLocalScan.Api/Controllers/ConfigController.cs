using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlexLocalScan.Shared.Options;

namespace PlexLocalScan.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IOptions<PlexOptions> _plexOptions;
    private readonly IOptions<TMDbOptions> _tmdbOptions;
    private readonly IOptions<MediaDetectionOptions> _mediaDetectionOptions;

    public ConfigController(
        IOptions<PlexOptions> plexOptions,
        IOptions<TMDbOptions> tmdbOptions,
        IOptions<MediaDetectionOptions> mediaDetectionOptions)
    {
        _plexOptions = plexOptions;
        _tmdbOptions = tmdbOptions;
        _mediaDetectionOptions = mediaDetectionOptions;
    }

    [HttpGet]
    public IActionResult GetAllConfigurations()
    {
        var config = new
        {
            Plex = _plexOptions.Value,
            TMDb = _tmdbOptions.Value,
            MediaDetection = _mediaDetectionOptions.Value
        };

        return Ok(config);
    }

    [HttpGet("plex")]
    public IActionResult GetPlexConfig()
    {
        return Ok(_plexOptions.Value);
    }

    [HttpGet("tmdb")]
    public IActionResult GetTMDbConfig()
    {
        return Ok(_tmdbOptions.Value);
    }

    [HttpGet("media-detection")]
    public IActionResult GetMediaDetectionConfig()
    {
        return Ok(_mediaDetectionOptions.Value);
    }
} 