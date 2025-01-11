using System.Collections.ObjectModel;
using PlexLocalScan.Core.Series;
using PlexLocalScan.Core.Tables;

namespace PlexLocalScan.Core.Media;

public record MediaInfo
{
    public string? Title { get; init; }
    public int? Year { get; init; }
    public int? TmdbId { get; init; }
    public string? ImdbId { get; init; }
    public ReadOnlyCollection<string>? Genres { get; init; }
    public MediaType? MediaType { get; set; }
    public int? SeasonNumber { get; init; }
    public int? EpisodeNumber { get; init; }
    public string? EpisodeTitle { get; init; }
    public int? EpisodeNumber2 { get; init; }
    public int? EpisodeTmdbId { get; set; }
    public string? PosterPath { get; set; }
    public string? Summary { get; set; }
    public string? Status { get; set; }
    public ReadOnlyCollection<SeasonInfo>? Seasons { get; init; }
    public ReadOnlyCollection<SeasonInfo>? SeasonsScanned { get; init; }
}