using System.ComponentModel.DataAnnotations;


namespace PlexLocalScan.Data.Models;

public class ScannedFile
{
    [Key]
    public int Id { get; init; }
    
    [Required]
    public string SourceFile { get; init; } = string.Empty;
    
    public string? DestFile { get; set; } = string.Empty;
    
    public MediaType? MediaType { get; set; } = null;
    
    public int? TmdbId { get; set; } = null;

    public int? SeasonNumber { get; set; } = null;
    public int? EpisodeNumber { get; set; } = null;
    
    [Required]
    public FileStatus Status { get; set; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}

public enum FileStatus
{
    Working,
    Success,
    Failed
}

public enum MediaType
{
    Movies,
    TvShows,
    Unknown
}