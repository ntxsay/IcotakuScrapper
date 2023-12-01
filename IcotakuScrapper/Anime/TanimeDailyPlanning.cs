using System.Diagnostics;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public enum AnimePlanningSortBy : byte
{
    Id,
    IdAnime,
    ReleaseDate,
    EpisodeNumber,
    EpisodeName,
    Day,
    AnimeName
}

public partial class TanimeDailyPlanning
{
    private const string ReleaseDateFormat = "yyyy-MM-dd";
    public int Id { get; protected set; }
    public int? IdAnime { get; set; }
    public int SheetId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string AnimeName { get; set; } = string.Empty;
    public TanimeBase? Anime { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public ushort EpisodeNumber { get; set; }
    public string? EpisodeName { get; set; }
    public DayOfWeek Day { get; set; }
    public string? Description { get; set; }

    public TanimeDailyPlanning()
    {
    }

    public TanimeDailyPlanning(int id)
    {
        Id = id;
    }

    public TanimeDailyPlanning(int id, int idAnime)
    {
        Id = id;
        IdAnime = idAnime;
    }

    public TanimeDailyPlanning(int id, int idAnime, DateOnly releaseDate, ushort episodeNumber, string episodeName,
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
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeDailyPlanning";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, SelectCountIdIdAnimeSheetIdKind columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = columnSelect switch
        {
            SelectCountIdIdAnimeSheetIdKind.Id => "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE Id = $Id",
            SelectCountIdIdAnimeSheetIdKind.IdAnime => "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE IdAnime = $Id",
            SelectCountIdIdAnimeSheetIdKind.SheetId => "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE SheetId = $Id",
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
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning
    /// </summary>
    /// <param name="releaseDate"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(DateOnly releaseDate, int sheetId,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND SheetId = $SheetId";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(DateOnly releaseDate, int? idAnime,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = idAnime.HasValue
            ? "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND IdAnime = $IdAnime"
            : "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND IdAnime IS NULL";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));
        if (idAnime.HasValue)
            command.Parameters.AddWithValue("$IdAnime", idAnime.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(DateOnly releaseDate, int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND SheetId = $SheetId AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(DateOnly releaseDate, int? idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = idAnime.HasValue
            ? "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND IdAnime = $IdAnime AND NoEpisode = $NoEpisode"
            : "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND IdAnime IS NULL AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);
        if (idAnime.HasValue)
            command.Parameters.AddWithValue("$IdAnime", idAnime.Value);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning
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
            "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

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
            "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(DateOnly releaseDate, int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate AND SheetId = $SheetId AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, SelectCountIdIdAnimeSheetIdKind columnSelect,
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
    
    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, sheetId, noEpisode, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int? idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, idAnime, noEpisode, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int? idAnime,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, idAnime, cancellationToken, cmd) > 0;   
    
    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int sheetId,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, sheetId, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeDailyPlanning[]> SelectAsync(AnimePlanningSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + sortBy switch
        {
            AnimePlanningSortBy.Id => $"ORDER BY TanimeDailyPlanning.Id {orderBy}",
            AnimePlanningSortBy.IdAnime => $"ORDER BY TanimeDailyPlanning.IdAnime {orderBy}",
            AnimePlanningSortBy.ReleaseDate => $"ORDER BY TanimeDailyPlanning.ReleaseDate {orderBy}",
            AnimePlanningSortBy.EpisodeNumber => $"ORDER BY TanimeDailyPlanning.NoEpisode {orderBy}",
            AnimePlanningSortBy.EpisodeName => $"ORDER BY TanimeDailyPlanning.EpisodeName {orderBy}",
            AnimePlanningSortBy.Day => $"ORDER BY TanimeDailyPlanning.NoDay {orderBy}",
            AnimePlanningSortBy.AnimeName => $"ORDER BY Tanime.Name {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "La valeur spécifiée est invalide")
        };
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeDailyPlanning>();

        return await GetRecords(reader, cancellationToken);
    }

    public static async Task<TanimeDailyPlanning[]> SelectAsync(int idAnime, AnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE IdAnime = $IdAnime" + Environment.NewLine +
                              sortBy switch
                              {
                                  AnimePlanningSortBy.Id => $"ORDER BY TanimeDailyPlanning.Id {orderBy}",
                                  AnimePlanningSortBy.IdAnime => $"ORDER BY TanimeDailyPlanning.IdAnime {orderBy}",
                                  AnimePlanningSortBy.ReleaseDate => $"ORDER BY TanimeDailyPlanning.ReleaseDate {orderBy}",
                                  AnimePlanningSortBy.EpisodeNumber => $"ORDER BY TanimeDailyPlanning.NoEpisode {orderBy}",
                                  AnimePlanningSortBy.EpisodeName => $"ORDER BY TanimeDailyPlanning.EpisodeName {orderBy}",
                                  AnimePlanningSortBy.Day => $"ORDER BY TanimeDailyPlanning.NoDay {orderBy}",
                                  AnimePlanningSortBy.AnimeName => $"ORDER BY Tanime.Name {orderBy}",
                                  _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy,
                                      "La valeur spécifiée est invalide")
                              };

        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeDailyPlanning>();

        return await GetRecords(reader, cancellationToken);
    }

    public static async Task<TanimeDailyPlanning[]> SelectAsync(DateOnly minDate, DateOnly maxDate,
        AnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE ReleaseDate BETWEEN $MinDate AND $MaxDate" + Environment.NewLine + sortBy switch
                              {
                                  AnimePlanningSortBy.Id => $"ORDER BY TanimeDailyPlanning.Id {orderBy}",
                                  AnimePlanningSortBy.IdAnime => $"ORDER BY TanimeDailyPlanning.IdAnime {orderBy}",
                                  AnimePlanningSortBy.ReleaseDate => $"ORDER BY TanimeDailyPlanning.ReleaseDate {orderBy}",
                                  AnimePlanningSortBy.EpisodeNumber => $"ORDER BY TanimeDailyPlanning.NoEpisode {orderBy}",
                                  AnimePlanningSortBy.EpisodeName => $"ORDER BY TanimeDailyPlanning.EpisodeName {orderBy}",
                                  AnimePlanningSortBy.Day => $"ORDER BY TanimeDailyPlanning.NoDay {orderBy}",
                                  AnimePlanningSortBy.AnimeName => $"ORDER BY Tanime.Name {orderBy}",
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
            return Array.Empty<TanimeDailyPlanning>();

        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Single

    public static async Task<TanimeDailyPlanning?> SingleAsync(int id,
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

        var records = await GetRecords(reader, cancellationToken);
        return records.Length > 0 ? records[0] : null;
    }

    public static async Task<TanimeDailyPlanning?> SingleAsync(int idAnime, ushort noEpisode,
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

        var records = await GetRecords(reader, cancellationToken);
        return records.Length > 0 ? records[0] : null;
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (EpisodeNumber <= 0)
            return new OperationState<int>(false, "Le numéro de l'épisode ne peut pas être inférieur ou égal à 0");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var animeId = await Tanime.GetIdOfAsync(SheetId, cancellationToken, command);
        if (animeId.HasValue)
            IdAnime = animeId;
        
        if (await ExistsAsync(new DateOnly(ReleaseDate.Year, ReleaseDate.Month, ReleaseDate.Day), SheetId, EpisodeNumber, cancellationToken, command))
            return new OperationState<int>(false, "L'épisode existe déjà");

        command.CommandText =
            """
            INSERT INTO TanimeDailyPlanning
                (IdAnime, SheetId, Url, AnimeName, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description)
            VALUES
                ($IdAnime, $SheetId, $Url, $AnimeName, $ReleaseDate, $NoEpisode, $EpisodeName, $NoDay, $Description)
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
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

    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TanimeDailyPlanning> values,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText =
            "INSERT OR REPLACE INTO TanimeDailyPlanning (SheetId, Url, AnimeName, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description) VALUES";
        command.Parameters.Clear();

        List<OperationState<int>> results = [];
        for (var i = 0; i < values.Count; i++)
        {
            var planning = values.ElementAt(i);
            if (planning.EpisodeNumber <= 0)
            {
                results.Add(
                    new OperationState<int>(false, $"Un des épisode sélectionnés n'est pas valide ({planning})."));
                continue;
            }

            command.CommandText += Environment.NewLine +
                                   $"($SheetId{i}, $Url{i}, $AnimeName{i}, $ReleaseDate{i}, $NoEpisode{i}, $EpisodeName{i}, $NoDay{i}, $Description{i})";

            command.Parameters.AddWithValue($"$SheetId{i}", planning.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", planning.Url);
            command.Parameters.AddWithValue($"$AnimeName{i}", planning.AnimeName);
            command.Parameters.AddWithValue($"$ReleaseDate{i}", planning.ReleaseDate.ToString(ReleaseDateFormat));
            command.Parameters.AddWithValue($"$NoEpisode{i}", planning.EpisodeNumber);
            command.Parameters.AddWithValue($"$EpisodeName{i}", planning.EpisodeName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$NoDay{i}", (byte)planning.Day);
            command.Parameters.AddWithValue($"$Description{i}", planning.Description ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun épisode n'est à ajouter.");

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
        var animeId = await Tanime.GetIdOfAsync(SheetId, cancellationToken, command);
        if (animeId.HasValue)
            IdAnime = animeId;
        
        var existingId = await GetIdOfAsync(new DateOnly(ReleaseDate.Year, ReleaseDate.Month, ReleaseDate.Day), SheetId, EpisodeNumber, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "L'épisode existe déjà");

        command.CommandText =
            """
            UPDATE TanimeDailyPlanning SET
                IdAnime = $IdAnime,
                SheetId = $SheetId,
                Url = $Url,
                AnimeName = $AnimeName,
                ReleaseDate = $ReleaseDate,
                NoEpisode = $NoEpisode,
                EpisodeName = $EpisodeName,
                NoDay = $NoDay,
                Description = $Description
            WHERE Id = $Id
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
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
        if (id <= 0 || !await ExistsAsync(id, SelectCountIdIdAnimeSheetIdKind.Id, cancellationToken, command))
            return new OperationState(false, "L'identifiant de l'épisode est invalide");

        command.CommandText = "DELETE FROM TanimeDailyPlanning WHERE Id = $Id";

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

    private static async Task<TanimeDailyPlanning[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<TanimeDailyPlanning> records = new();
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idPlanning = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var record = records.Find(f => f.Id == idPlanning);
            if (record == null)
            {
                record = new TanimeDailyPlanning()
                {
                    Id = idPlanning,
                    IdAnime = reader.IsDBNull(reader.GetOrdinal("IdAnime"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("IdAnime")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("BaseSheetId")),
                    Url = reader.GetString(reader.GetOrdinal("BaseUrl")),
                    AnimeName = reader.GetString(reader.GetOrdinal("BaseAnimeName")),
                    ReleaseDate = DateHelpers.GetDateOnly(reader.GetString(reader.GetOrdinal("ReleaseDate")), ReleaseDateFormat),
                    EpisodeNumber = (ushort)reader.GetInt16(reader.GetOrdinal("NoEpisode")),
                    EpisodeName = reader.IsDBNull(reader.GetOrdinal("EpisodeName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("EpisodeName")),
                    Day = (DayOfWeek)reader.GetByte(reader.GetOrdinal("NoDay")),
                    Description = reader.IsDBNull(reader.GetOrdinal("BaseDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BaseDescription")),
                    Anime = reader.IsDBNull(reader.GetOrdinal("IdAnime")) 
                    ? null
                    : new TanimeBase(reader.GetInt32(reader.GetOrdinal("IdAnime")))
                        {
                            Name = reader.GetString(reader.GetOrdinal("AnimeName")),
                            Url = reader.GetString(reader.GetOrdinal("AnimeUrl")),
                            SheetId = reader.GetInt32(reader.GetOrdinal("AnimeSheetId")),
                            DiffusionState = (DiffusionStateKind)reader.GetByte( reader.GetOrdinal("DiffusionState")),
                            EpisodesCount = (ushort)reader.GetInt16(reader.GetOrdinal("EpisodeCount")),
                            ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("AnimeThumbnailUrl"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("AnimeThumbnailUrl")),
                            Description = reader.IsDBNull(reader.GetOrdinal("AnimeDescription"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("AnimeDescription")),
                            Format = reader.IsDBNull(reader.GetOrdinal("IdFormat"))
                                ? null
                                : Tformat.GetRecord(reader,
                                    idIndex: reader.GetOrdinal("IdFormat"),
                                    nameIndex: reader.GetOrdinal("FormatName"),
                                    descriptionIndex: reader.GetOrdinal("FormatDescription")),
                            Target = reader.IsDBNull(reader.GetOrdinal("IdTarget"))
                                ? null
                                : Ttarget.GetRecord(reader,
                                    idIndex: reader.GetOrdinal("IdTarget"),
                                    nameIndex: reader.GetOrdinal("TargetName"),
                                    descriptionIndex: reader.GetOrdinal("TargetDescription")),
                            OrigineAdaptation = reader.IsDBNull(reader.GetOrdinal("IdOrigine"))
                                ? null
                                : TorigineAdaptation.GetRecord(reader,
                                    idIndex: reader.GetOrdinal("IdOrigine"),
                                    nameIndex: reader.GetOrdinal("OrigineAdaptationName"),
                                    descriptionIndex: reader.GetOrdinal("OrigineAdaptationDescription")),
                            Season = reader.IsDBNull(reader.GetOrdinal("IdSeason"))
                                ? null
                                : Tseason.GetRecord(reader,
                                    idIndex: reader.GetOrdinal("IdSeason"),
                                    displayNameIndex: reader.GetOrdinal("SeasonDisplayName"),
                                    seasonNumberIndex: reader.GetOrdinal("SeasonNumber")),
                        },
                };
                
                
                records.Add(record);
            }
            
            if (record.Anime == null)
                continue;

            
            if (!reader.IsDBNull(reader.GetOrdinal("CategoryId")))
            {
                var categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                var category = record.Anime.Categories.FirstOrDefault(x => x.Id == categoryId);
                if (category == null)
                {
                    category = Tcategory.GetRecord(reader,
                        idIndex: reader.GetOrdinal("CategoryId"),
                        sheetIdIndex: reader.GetOrdinal("CategorySheetId"),
                        typeIndex: reader.GetOrdinal("CategoryType"),
                        urlIndex: reader.GetOrdinal("CategoryUrl"),
                        sectionIndex: reader.GetOrdinal("CategorySection"),
                        nameIndex: reader.GetOrdinal("CategoryName"),
                        descriptionIndex: reader.GetOrdinal("CategoryDescription"));
                    record.Anime.Categories.Add(category);
                }
            }
        }

        return records.ToArray();
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeDailyPlanning.Id AS BaseId,
            TanimeDailyPlanning.IdAnime,
            TanimeDailyPlanning.SheetId AS BaseSheetId,
            TanimeDailyPlanning.Url AS BaseUrl,
            TanimeDailyPlanning.AnimeName AS BaseAnimeName,
            TanimeDailyPlanning.ReleaseDate,
            TanimeDailyPlanning.NoEpisode,
            TanimeDailyPlanning.EpisodeName,
            TanimeDailyPlanning.NoDay,
            TanimeDailyPlanning.Description AS BaseDescription,
            
            Tanime.Name AS AnimeName,
            Tanime.Url AS AnimeUrl,
            Tanime.SheetId AS AnimeSheetId,
            Tanime.DiffusionState,
            Tanime.ThumbnailUrl AS AnimeThumbnailUrl,
            Tanime.Description AS AnimeDescription,
            
            Tformat.Name as FormatName,
            Tformat.Description as FormatDescription,
            
            Ttarget.Name as TargetName,
            Ttarget.Description as TargetDescription,
            
            TorigineAdaptation.Name as OrigineAdaptationName,
            TorigineAdaptation.Description as OrigineAdaptationDescription,
            
            Tseason.DisplayName as SeasonDisplayName,
            Tseason.SeasonNumber as SeasonNumber,
            
            TanimeCategory.IdCategory AS CategoryId,
            Tcategory.SheetId AS CategorySheetId,
            Tcategory.Type AS CategoryType,
            Tcategory.Url AS CategoryUrl,
            Tcategory.Section AS CategorySection,
            Tcategory.Name AS CategoryName,
            Tcategory.Description AS CategoryDescription

        FROM
            TanimeDailyPlanning
        LEFT JOIN main.Tanime on Tanime.Id = TanimeDailyPlanning.IdAnime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        
        
        """;
}