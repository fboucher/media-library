using FFMpegCore;

namespace MediaLibrary.Services;

public class ThumbnailService(IConfiguration configuration, ILogger<ThumbnailService> logger)
{
    public async Task<string?> ExtractAsync(string videoPath, int mediaId)
    {
        var mediaRoot = configuration["Media:StoragePath"] ?? "/data/media";
        var thumbDir  = Path.Combine(mediaRoot, "thumbnails");
        Directory.CreateDirectory(thumbDir);

        var outputPath = Path.Combine(thumbDir, $"{mediaId}.jpg");

        try
        {
            await FFMpeg.SnapshotAsync(videoPath, outputPath, captureTime: TimeSpan.FromSeconds(0));
            return outputPath;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract thumbnail for {VideoPath}", videoPath);
            return null;
        }
    }
}
