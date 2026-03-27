using Dapper;
using MediaLibrary.Models;

namespace MediaLibrary.Data;

public class DetectionRepository(DatabaseService db)
{
    public async Task<int> InsertAsync(Detection detection)
    {
        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO detections (media_id, research_word, detected_json, created_at)
            VALUES (@MediaId, @ResearchWord, @DetectedJson, @CreatedAt);
            SELECT last_insert_rowid();
            """,
            new
            {
                detection.MediaId,
                detection.ResearchWord,
                detection.DetectedJson,
                CreatedAt = detection.CreatedAt.ToString("o")
            });
    }

    public async Task<IEnumerable<Detection>> GetByMediaIdAsync(int mediaId)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync(
            "SELECT id, media_id, research_word, detected_json, created_at FROM detections WHERE media_id = @MediaId ORDER BY created_at DESC",
            new { MediaId = mediaId });

        return rows.Select(r => new Detection
        {
            Id           = (int)r.id,
            MediaId      = (int)r.media_id,
            ResearchWord = (string)r.research_word,
            DetectedJson = (string)r.detected_json,
            CreatedAt    = DateTime.Parse((string)r.created_at)
        });
    }
}
