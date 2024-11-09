using Microsoft.Extensions.Options;
using PlexLocalscan.Options;
using Microsoft.Extensions.Logging;
using System.Web;

namespace PlexLocalscan.Services;

public class PlexHandler : IPlexHandler
{
    private readonly ILogger<PlexHandler> _logger;
    private readonly PlexOptions _options;
    private readonly HttpClient _httpClient;

    public PlexHandler(ILogger<PlexHandler> logger, IOptions<PlexOptions> options, HttpClient httpClient)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClient;
    }

    private async Task<string> GetSectionIdForPathAsync(string folderPath)
    {
        var url = $"{_options.ApiEndpoint}/library/sections?X-Plex-Token={_options.PlexToken}";
        var response = await _httpClient.GetStringAsync(url);
        
        // Basic XML parsing to find matching section
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(response);
        
        var directories = doc.SelectNodes("//Directory");
        if (directories == null) return "all";
        
        foreach (System.Xml.XmlNode dir in directories)
        {
            var locations = dir.SelectNodes("Location");
            if (locations == null) continue;
            
            foreach (System.Xml.XmlNode loc in locations)
            {
                var pathAttr = loc.Attributes?["path"];
                if (pathAttr?.Value != null && folderPath.StartsWith(pathAttr.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var keyAttr = dir.Attributes?["key"];
                    return keyAttr?.Value ?? "all";
                }
            }
        }
        
        _logger.LogWarning("No matching section found for path: {FolderPath}, falling back to 'all'", folderPath);
        return "all";
    }

    public async Task AddFolderForScanningAsync(string folderPath, string baseFolder)
    {
        try
        {
            _logger.LogInformation("Adding folder for scanning: {FolderPath}", folderPath);
            
            var sectionId = await GetSectionIdForPathAsync(baseFolder);
            _logger.LogDebug("Found section ID: {SectionId} for base folder: {BaseFolder}", sectionId, baseFolder);
            
            var encodedPath = HttpUtility.UrlEncode(folderPath);
            var url = $"{_options.ApiEndpoint}/library/sections/{sectionId}/refresh?path={encodedPath}&X-Plex-Token={_options.PlexToken}";
            
            _logger.LogDebug("Request URL: {Url}", url.Replace(_options.PlexToken, "REDACTED"));
            
            _logger.LogDebug("Sending GET request to Plex API...");
            var response = await _httpClient.GetAsync(url);
            _logger.LogDebug("Response status code: {StatusCode}", response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully initiated scan for folder: {FolderPath}", folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add folder for scanning: {FolderPath}", folderPath);
            throw;
        }
    }

    public async Task DeleteFolderFromPlexAsync(string folderPath)
    {
        try
        {
            _logger.LogInformation("Deleting folder from Plex: {FolderPath}", folderPath);
            
            var sectionId = await GetSectionIdForPathAsync(folderPath);
            _logger.LogDebug("Found section ID: {SectionId} for folder: {FolderPath}", sectionId, folderPath);
            
            var encodedPath = HttpUtility.UrlEncode(folderPath);
            var url = $"{_options.ApiEndpoint}/library/sections/{sectionId}/refresh?path={encodedPath}&X-Plex-Token={_options.PlexToken}&type=1";
            
            _logger.LogDebug("Request URL: {Url}", url.Replace(_options.PlexToken, "REDACTED"));
            
            _logger.LogDebug("Sending DELETE request to Plex API...");
            var response = await _httpClient.DeleteAsync(url);
            _logger.LogDebug("Response status code: {StatusCode}", response.StatusCode);
            
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully deleted folder from Plex: {FolderPath}", folderPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete folder from Plex: {FolderPath}", folderPath);
            throw;
        }
    }
} 