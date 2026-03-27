using Dapper;
using MediaLibrary.Models;

namespace MediaLibrary.Data;

public class MediaRepository(DatabaseService db)
{
    public async Task<IEnumerable<MediaItem>> GetAllAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync(
            "SELECT id, file_name, file_size, media_type, thumbnail_path, description, created_at FROM media ORDER BY created_at DESC");

        return rows.Select(r => new MediaItem
        {
            Id            = (int)r.id,
            FileName      = (string)r.file_name,
            FileSize      = (long)r.file_size,
            MediaType     = (string)r.media_type,
            ThumbnailPath = (string?)r.thumbnail_path,
            Description   = (string)r.description,
            CreatedAt     = DateTime.Parse((string)r.created_at)
        });
    }

    public async Task<int> InsertAsync(MediaItem item)
    {
        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO media (file_name, file_size, media_type, thumbnail_path, description, created_at)
            VALUES (@FileName, @FileSize, @MediaType, @ThumbnailPath, @Description, @CreatedAt);
            SELECT last_insert_rowid();
            """,
            new
            {
                item.FileName,
                item.FileSize,
                item.MediaType,
                item.ThumbnailPath,
                item.Description,
                CreatedAt = item.CreatedAt.ToString("o")
            });
    }

    public async Task UpdateThumbnailAsync(int id, string thumbnailPath)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE media SET thumbnail_path = @ThumbnailPath WHERE id = @Id",
            new { ThumbnailPath = thumbnailPath, Id = id });
    }

    public async Task UpdateDescriptionAsync(int id, string description)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE media SET description = @Description WHERE id = @Id",
            new { Description = description, Id = id });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM media WHERE id = @Id", new { Id = id });
    }

    public async Task<MediaItem?> GetByIdAsync(int id)
    {
        using var conn = db.CreateConnection();
        var r = await conn.QuerySingleOrDefaultAsync(
            "SELECT id, file_name, file_size, media_type, thumbnail_path, description, created_at FROM media WHERE id = @Id",
            new { Id = id });

        if (r is null) return null;

        return new MediaItem
        {
            Id            = (int)r.id,
            FileName      = (string)r.file_name,
            FileSize      = (long)r.file_size,
            MediaType     = (string)r.media_type,
            ThumbnailPath = (string?)r.thumbnail_path,
            Description   = (string)r.description,
            CreatedAt     = DateTime.Parse((string)r.created_at)
        };
    }
}
