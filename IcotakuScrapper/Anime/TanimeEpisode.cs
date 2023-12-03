using System.Diagnostics;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public enum AnimeEpisodeSortBy : byte
{
    Id,
    IdAnime,
    ReleaseDate,
    EpisodeNumber,
    EpisodeName,
    Day
}

public partial class TanimeEpisode
{
    public const string ReleaseDateFormat = "yyyy-MM-dd";

    public int Id { get; set; }
    public int IdAnime { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public ushort EpisodeNumber { get; set; }
    public string? EpisodeName { get; set; }
    public DayOfWeek Day { get; set; }
    public string? Description { get; set; }

    public TanimeEpisode()
    {
    }

    public TanimeEpisode(int id)
    {
        Id = id;
    }

    public TanimeEpisode(int id, int idAnime)
    {
        Id = id;
        IdAnime = idAnime;
    }

    public TanimeEpisode(int id, int idAnime, DateOnly releaseDate, ushort episodeNumber, string episodeName,
        DayOfWeek day, string? description)
    {
        Id = id;
        IdAnime = idAnime;
        ReleaseDate = releaseDate;
        EpisodeNumber = episodeNumber;
        EpisodeName = episodeName;
        Day = day;
        Description = description;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeEpisode
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeEpisode";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeEpisode ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, SelectCountIdIdAnimeKind columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = columnSelect switch
        {
            SelectCountIdIdAnimeKind.Id => "SELECT COUNT(Id) FROM TanimeEpisode WHERE Id = $Id",
            SelectCountIdIdAnimeKind.IdAnime => "SELECT COUNT(Id) FROM TanimeEpisode WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect,
                "La valeur spécifiée est invalide")
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeEpisode
    /// </summary>
    /// <param name="releaseDate"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeEpisode WHERE ReleaseDate = $ReleaseDate";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeEpisode
    /// </summary>
    /// <param name="noEpisode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, ushort noEpisode, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeEpisode WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeEpisode WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, SelectCountIdIdAnimeKind columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, noEpisode, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeEpisode[]> SelectAsync(AnimeEpisodeSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + sortBy switch
        {
            AnimeEpisodeSortBy.Id => $"ORDER BY TanimeEpisode.Id {orderBy}",
            AnimeEpisodeSortBy.IdAnime => $"ORDER BY TanimeEpisode.IdAnime {orderBy}",
            AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY TanimeEpisode.ReleaseDate {orderBy}",
            AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY TanimeEpisode.NoEpisode {orderBy}",
            AnimeEpisodeSortBy.EpisodeName => $"ORDER BY TanimeEpisode.EpisodeName {orderBy}",
            AnimeEpisodeSortBy.Day => $"ORDER BY TanimeEpisode.NoDay {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "La valeur spécifiée est invalide")
        };
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeEpisode>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeEpisode[]> SelectAsync(int idAnime, AnimeEpisodeSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE IdAnime = $IdAnime" + Environment.NewLine +
                              sortBy switch
                              {
                                  AnimeEpisodeSortBy.Id => $"ORDER BY TanimeEpisode.Id {orderBy}",
                                  AnimeEpisodeSortBy.IdAnime => $"ORDER BY TanimeEpisode.IdAnime {orderBy}",
                                  AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY TanimeEpisode.ReleaseDate {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY TanimeEpisode.NoEpisode {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeName => $"ORDER BY TanimeEpisode.EpisodeName {orderBy}",
                                  AnimeEpisodeSortBy.Day => $"ORDER BY TanimeEpisode.NoDay {orderBy}",
                                  _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy,
                                      "La valeur spécifiée est invalide")
                              };

        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeEpisode>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeEpisode[]> SelectAsync(DateOnly minDate, DateOnly maxDate,
        AnimeEpisodeSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE ReleaseDate BETWEEN $MinDate AND $MaxDate" + Environment.NewLine + sortBy switch
                              {
                                  AnimeEpisodeSortBy.Id => $"ORDER BY TanimeEpisode.Id {orderBy}",
                                  AnimeEpisodeSortBy.IdAnime => $"ORDER BY TanimeEpisode.IdAnime {orderBy}",
                                  AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY TanimeEpisode.ReleaseDate {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY TanimeEpisode.NoEpisode {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeName => $"ORDER BY TanimeEpisode.EpisodeName {orderBy}",
                                  AnimeEpisodeSortBy.Day => $"ORDER BY TanimeEpisode.NoDay {orderBy}",
                                  _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy,
                                      "La valeur spécifiée est invalide")
                              };

        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$MinDate", minDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$MaxDate", maxDate.ToString(ReleaseDateFormat));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeEpisode>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeEpisode?> SingleAsync(int id,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeEpisode?> SingleAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (EpisodeNumber <= 0)
            return new OperationState<int>(false, "Le numéro de l'épisode ne peut pas être inférieur ou égal à 0");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "L'identifiant de l'anime est invalide");

        if (await ExistsAsync(IdAnime, EpisodeNumber, cancellationToken, command))
            return new OperationState<int>(false, "L'épisode existe déjà");

        command.CommandText =
            """
            INSERT INTO TanimeEpisode
                (IdAnime, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description)
            VALUES
                ($IdAnime, $ReleaseDate, $NoEpisode, $EpisodeName, $NoDay, $Description)
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", EpisodeNumber);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result <= 0)
                return new OperationState<int>(false, "L'insertion n'a pas été effectuée");

            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "L'insertion a été effectuée", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public static async Task<OperationState> InsertAsync(int idAnime, IReadOnlyCollection<TanimeEpisode> values,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (idAnime <= 0 || !await Tanime.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");

        if (values.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        List<OperationState<int>> results =  [];
        command.CommandText =
            "INSERT OR REPLACE INTO TanimeEpisode (IdAnime, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description) VALUES";
        command.Parameters.Clear();

        for (var i = 0; i < values.Count; i++)
        {
            var episode = values.ElementAt(i);
            if (episode.EpisodeNumber <= 0)
            {
                results.Add(
                    new OperationState<int>(false, $"Un des épisode sélectionnés n'est pas valide ({episode})."));
                continue;
            }

            command.CommandText += Environment.NewLine +
                                   $"($IdAnime, $ReleaseDate{i}, $NoEpisode{i}, $EpisodeName{i}, $NoDay{i}, $Description{i})";

            command.Parameters.AddWithValue($"$ReleaseDate{i}", episode.ReleaseDate.ToString(ReleaseDateFormat));
            command.Parameters.AddWithValue($"$NoEpisode{i}", episode.EpisodeNumber);
            command.Parameters.AddWithValue($"$EpisodeName{i}", episode.EpisodeName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$NoDay{i}", (byte)episode.Day);
            command.Parameters.AddWithValue($"$Description{i}", episode.Description ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun épisode n'est à ajouter.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState(false, "Une erreur est survenue lors de l'insertion");

            if (results.All(a => a.IsSuccess))
                return new OperationState(true, "Insertion réussie");
            if (results.All(a => !a.IsSuccess))
                return new OperationState(false, "Aucun épisode n'a été inséré");
            return new OperationState(true, "Certains épisodes n'ont pas été insérés");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (EpisodeNumber <= 0)
            return new OperationState(false, "Le numéro de l'épisode ne peut pas être inférieur ou égal à 0");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (Id <= 0 || !await ExistsAsync(Id, SelectCountIdIdAnimeKind.Id, cancellationToken, command))
            return new OperationState(false, "L'identifiant de l'épisode est invalide");

        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'identifiant de l'anime est invalide");

        var existingId = await GetIdOfAsync(IdAnime, EpisodeNumber, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "L'épisode existe déjà");

        command.CommandText =
            """
            UPDATE TanimeEpisode SET
                IdAnime = $IdAnime,
                ReleaseDate = $ReleaseDate,
                NoEpisode = $NoEpisode,
                EpisodeName = $EpisodeName,
                NoDay = $NoDay,
                Description = $Description
            WHERE Id = $Id
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", EpisodeNumber);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Id", Id);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "La mise à jour n'a pas été effectuée")
                : new OperationState(true, "La mise à jour a été effectuée");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (id <= 0 || !await ExistsAsync(id, SelectCountIdIdAnimeKind.Id, cancellationToken, command))
            return new OperationState(false, "L'identifiant de l'épisode est invalide");

        command.CommandText = "DELETE FROM TanimeEpisode WHERE Id = $Id";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} ligne(s) ont été supprimée(s)");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    #endregion

    private static async IAsyncEnumerable<TanimeEpisode> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TanimeEpisode()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdAnime = reader.GetInt32(reader.GetOrdinal("IdAnime")),
                ReleaseDate = DateHelpers.GetDateOnly(reader.GetString(reader.GetOrdinal("ReleaseDate")), ReleaseDateFormat),
                EpisodeNumber = (ushort)reader.GetInt16(reader.GetOrdinal("NoEpisode")),
                EpisodeName = reader.IsDBNull(reader.GetOrdinal("EpisodeName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("EpisodeName")),
                Day = (DayOfWeek)reader.GetByte(reader.GetOrdinal("NoDay")),
                Description = reader.IsDBNull(reader.GetOrdinal("BaseDescription"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("BaseDescription")),
            };
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeEpisode.Id AS BaseId,
            TanimeEpisode.IdAnime,
            TanimeEpisode.ReleaseDate,
            TanimeEpisode.NoEpisode,
            TanimeEpisode.EpisodeName,
            TanimeEpisode.NoDay,
            TanimeEpisode.Description AS BaseDescription

        FROM
            TanimeEpisode
        """;
}