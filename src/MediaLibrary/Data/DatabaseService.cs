using Dapper;
using Microsoft.Data.Sqlite;

namespace MediaLibrary.Data;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        var dbPath = configuration["Database:Path"] ?? "/data/media-library.db";
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        _connectionString = $"Data Source={dbPath}";
    }

    public SqliteConnection CreateConnection() => new(_connectionString);

    public void Initialize()
    {
        using var conn = CreateConnection();
        conn.Open();

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS media (
                id            INTEGER PRIMARY KEY AUTOINCREMENT,
                file_name     TEXT    NOT NULL,
                file_size     INTEGER NOT NULL,
                media_type    TEXT    NOT NULL,
                thumbnail_path TEXT,
                description   TEXT    NOT NULL DEFAULT '',
                created_at    TEXT    NOT NULL
            )
            """);

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS connections (
                id       INTEGER PRIMARY KEY AUTOINCREMENT,
                name     TEXT NOT NULL,
                url      TEXT NOT NULL,
                api_key  TEXT NOT NULL,
                model    TEXT NOT NULL
            )
            """);

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS settings (
                key   TEXT PRIMARY KEY,
                value TEXT NOT NULL
            )
            """);

        // Seed defaults (only insert if not already present)
        conn.Execute("""
            INSERT OR IGNORE INTO settings (key, value)
            VALUES ('prompt', 'Describe this media in 2-3 sentences.')
            """);

        conn.Execute("""
            INSERT OR IGNORE INTO settings (key, value)
            VALUES ('active_connection_id', '')
            """);

        conn.Execute("""
            CREATE TABLE IF NOT EXISTS detections (
                id             INTEGER PRIMARY KEY AUTOINCREMENT,
                media_id       INTEGER NOT NULL,
                research_word  TEXT    NOT NULL,
                detected_json  TEXT    NOT NULL,
                created_at     TEXT    NOT NULL
            )
            """);
    }
}
