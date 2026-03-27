namespace MediaLibrary.Models;

public class MediaItem
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSize { get; set; }
    public string MediaType { get; set; } = "";   // "image" | "video"
    public string? ThumbnailPath { get; set; }
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public bool IsVideo => MediaType == "video";

    public string ThumbnailUrl => IsVideo && ThumbnailPath != null
        ? $"/media/thumbnails/{Path.GetFileName(ThumbnailPath)}"
        : $"/media/{FileName}";

    public string FileSizeDisplay => FileSize switch
    {
        < 1024 => $"{FileSize} B",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        _ => $"{FileSize / (1024.0 * 1024):F1} MB"
    };
}
