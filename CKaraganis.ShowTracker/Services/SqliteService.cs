using System.Data;
using CKaraganis.ShowTracker.Enums;
using CKaraganis.ShowTracker.Models;
using Microsoft.Data.Sqlite;

namespace CKaraganis.ShowTracker.Services;

public class SqliteService : IDataService
{
    private readonly string ConnectionString;

    public SqliteService()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = "shows.db"
        };

        ConnectionString = connectionString.ConnectionString;

        using var connection = new SqliteConnection(connectionString.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = """
                              CREATE TABLE IF NOT EXISTS Shows (
                                  Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                                  ShowName            TEXT    NOT NULL,
                                  EpisodeCount        INTEGER NOT NULL,
                                  IndexingEpisode     INTEGER NOT NULL,
                                  IndexingDate        TEXT    NOT NULL,
                                  EpisodesPerInterval INTEGER NOT NULL,
                                  IntervalUnit        TEXT    NOT NULL
                              );
                              """;

        command.ExecuteNonQuery();
        connection.Close();
    }

    public async Task<IEnumerable<Show>> GetAllShowsAsync()
    {
        var shows = new List<Show>();

        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = "SELECT Id, ShowName, EpisodeCount, IndexingEpisode, IndexingDate, EpisodesPerInterval, IntervalUnit FROM Shows;";
        command.CommandTimeout = 60;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var date = reader.GetDateTime(4);
            shows.Add(new Show(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                new DateOnly(date.Year, date.Month, date.Day),
                reader.GetInt32(5),
                Enum.Parse<IntervalUnit>(reader.GetString(6))));
        }

        await connection.CloseAsync();
        return shows;
    }
}
