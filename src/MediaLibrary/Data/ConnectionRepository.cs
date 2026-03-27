using Dapper;
using MediaLibrary.Models;

namespace MediaLibrary.Data;

public class ConnectionRepository(DatabaseService db)
{
    public async Task<IEnumerable<Connection>> GetAllAsync()
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync(
            "SELECT id, name, url, api_key, model FROM connections ORDER BY name");

        return rows.Select(r => new Connection
        {
            Id     = (int)r.id,
            Name   = (string)r.name,
            Url    = (string)r.url,
            ApiKey = (string)r.api_key,
            Model  = (string)r.model
        });
    }

    public async Task<Connection?> GetByIdAsync(int id)
    {
        using var conn = db.CreateConnection();
        var r = await conn.QuerySingleOrDefaultAsync(
            "SELECT id, name, url, api_key, model FROM connections WHERE id = @Id",
            new { Id = id });

        if (r is null) return null;
        return new Connection
        {
            Id     = (int)r.id,
            Name   = (string)r.name,
            Url    = (string)r.url,
            ApiKey = (string)r.api_key,
            Model  = (string)r.model
        };
    }

    public async Task<int> InsertAsync(Connection c)
    {
        using var conn = db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            """
            INSERT INTO connections (name, url, api_key, model)
            VALUES (@Name, @Url, @ApiKey, @Model);
            SELECT last_insert_rowid();
            """,
            new { c.Name, c.Url, c.ApiKey, c.Model });
    }

    public async Task UpdateAsync(Connection c)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE connections SET name = @Name, url = @Url, api_key = @ApiKey, model = @Model WHERE id = @Id",
            new { c.Name, c.Url, c.ApiKey, c.Model, c.Id });
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = db.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM connections WHERE id = @Id", new { Id = id });
    }
}
