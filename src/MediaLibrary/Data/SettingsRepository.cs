using Dapper;

namespace MediaLibrary.Data;

public class SettingsRepository(DatabaseService db)
{
    public async Task<string?> GetAsync(string key)
    {
        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<string>(
            "SELECT value FROM settings WHERE key = @Key", new { Key = key });
    }

    public async Task SetAsync(string key, string value)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "INSERT INTO settings (key, value) VALUES (@Key, @Value) ON CONFLICT(key) DO UPDATE SET value = excluded.value",
            new { Key = key, Value = value });
    }
}
