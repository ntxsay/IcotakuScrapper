using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Anime;

public enum AnimeDailyPlanningSortBy : byte
{
    Id,
    ReleaseDate,
    EpisodeNumber,
    EpisodeName,
    Day,
    AnimeName,
    SheetId,
}

/// <summary>
/// Représente un épisode d'un anime dans le planning quotidien.
/// </summary>
public partial class TanimeDailyPlanning
{
    private const string ReleaseDateFormat = "yyyy-MM-dd";
    public int Id { get; protected set; }
    public int SheetId { get; set; }
    public string Url { get; set; } = string.Empty;
    /// <summary>
    /// Obtient ou définit une valeur indiquant si l'anime est réservé à un public adulte.
    /// </summary>
    public bool IsAdultContent { get; set; }

    /// <summary>
    /// Obtient ou définit une valeur indiquant si l'anime est réservé à un public averti.
    /// </summary>
    public bool IsExplicitContent { get; set; }
    public string AnimeName { get; set; } = string.Empty;
    public TanimeBase? Anime { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public string LiteralReleaseDate => ReleaseDate.ToString("dddd dd MMMM yyyy");
    public ushort EpisodeNumber { get; set; }
    public string? EpisodeName { get; set; }
    public DayOfWeek Day { get; set; }
    public string? LiteralDay => DateHelpers.GetLiteralDay(Day);
    public string? Description { get; set; }
    
    /// <summary>
    /// Obtient ou définit l'url de l'image de l'anime.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    public TanimeDailyPlanning()
    {
    }

    public TanimeDailyPlanning(int id)
    {
        Id = id;
    }

    public TanimeDailyPlanning(int id, int sheetId)
    {
        Id = id;
        SheetId = sheetId;
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
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE SheetId = $Id",
            _ => string.Empty
        };

        if ((columnSelect != IntColumnSelect.Id && columnSelect != IntColumnSelect.SheetId) || command.CommandText.IsStringNullOrEmptyOrWhiteSpace())
        {
            LogServices.LogDebug("Seules les colonnes Id et SheetId sont autorisées pour la sélection");
            return 0;
        }

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
    /// <param name="releaseDate">Date de sortie de l'épisode</param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="releaseDate">Date de sortie de l'épisode</param>
    /// <param name="sheetId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="releaseDate"></param>
    /// <param name="sheetId"></param>
    /// <param name="noEpisode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
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


    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeDailyPlanning
    /// </summary>
    /// <param name="noEpisode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="sheetId"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int sheetId, ushort noEpisode, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeDailyPlanning WHERE SheetId = $SheetId AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int sheetId, ushort noEpisode, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeDailyPlanning WHERE SheetId = $SheetId AND NoEpisode = $NoEpisode";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", sheetId);
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

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(sheetId, noEpisode, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, sheetId, noEpisode, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(DateOnly releaseDate, int sheetId,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(releaseDate, sheetId, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeDailyPlanning[]> SelectAsync(bool? isAdultContent, bool? isExplicitContent, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript;

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "WHERE TanimeDailyPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
        {
            if (isAdultContent.HasValue)
                command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsExplicitContent = $IsExplicitContent";
            else
                command.CommandText += Environment.NewLine + "WHERE TanimeDailyPlanning.IsExplicitContent = $IsExplicitContent";
        }

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeDailyPlanning[]> SelectAsync(int sheetId, bool? isAdultContent, bool? isExplicitContent, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeDailyPlanning.SheetId = $SheetId";

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsExplicitContent = $IsExplicitContent";

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", sheetId);

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeDailyPlanning[]> SelectAsync(DateOnly date,
        bool? isAdultContent, bool? isExplicitContent, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE ReleaseDate = $ReleaseDate";

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsExplicitContent = $IsExplicitContent";

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", date.ToString(ReleaseDateFormat));

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeDailyPlanning[]> SelectAsync(DateOnly minDate, DateOnly maxDate,
        bool? isAdultContent, bool? isExplicitContent,
        AnimeDailyPlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE ReleaseDate BETWEEN $MinDate AND $MaxDate";

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeDailyPlanning.IsExplicitContent = $IsExplicitContent";

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$MinDate", minDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$MaxDate", maxDate.ToString(ReleaseDateFormat));

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
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

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
    }

    public static async Task<TanimeDailyPlanning?> SingleAsync(int sheetId, ushort noEpisode,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE SheetId = $SheetId AND NoEpisode = $NoEpisode";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        if (await ExistsAsync(new DateOnly(ReleaseDate.Year, ReleaseDate.Month, ReleaseDate.Day), SheetId, EpisodeNumber, cancellationToken, command))
            return new OperationState<int>(false, "L'épisode existe déjà");

        command.CommandText =
            """
            INSERT INTO TanimeDailyPlanning
                (SheetId, Url, IsAdultContent, IsExplicitContent, AnimeName, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description, ThumbnailUrl)
            VALUES
                ($SheetId, $Url, $IsAdultContent, $IsExplicitContent, $AnimeName, $ReleaseDate, $NoEpisode, $EpisodeName, $NoDay, $Description, $ThumbnailUrl)
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", EpisodeNumber);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);

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

    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TanimeDailyPlanning> values, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeDailyPlanning (SheetId, Url, IsAdultContent, IsExplicitContent, AnimeName, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description, ThumbnailUrl) VALUES";
        command.Parameters.Clear();

        for (var i = 0; i < values.Count; i++)
        {
            var planning = values.ElementAt(i);
            
            command.CommandText += Environment.NewLine +
                                   $"($SheetId{i}, $Url{i}, $IsAdultContent{i}, $IsExplicitContent{i}, $AnimeName{i}, $ReleaseDate{i}, $NoEpisode{i}, $EpisodeName{i}, $NoDay{i}, $Description{i}, $ThumbnailUrl{i})";

            command.Parameters.AddWithValue($"$SheetId{i}", planning.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", planning.Url);
            command.Parameters.AddWithValue($"$IsAdultContent{i}", planning.IsAdultContent);
            command.Parameters.AddWithValue($"$IsExplicitContent{i}", planning.IsExplicitContent);
            command.Parameters.AddWithValue($"$AnimeName{i}", planning.AnimeName);
            command.Parameters.AddWithValue($"$ReleaseDate{i}", planning.ReleaseDate.ToString(ReleaseDateFormat));
            command.Parameters.AddWithValue($"$NoEpisode{i}", planning.EpisodeNumber);
            command.Parameters.AddWithValue($"$EpisodeName{i}", planning.EpisodeName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$NoDay{i}", (byte)planning.Day);
            command.Parameters.AddWithValue($"$Description{i}", planning.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$ThumbnailUrl{i}", planning.ThumbnailUrl ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun épisode n'est à ajouter.");

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0, $"{count} enregistrement(s) sur {values.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var existingId = await GetIdOfAsync(new DateOnly(ReleaseDate.Year, ReleaseDate.Month, ReleaseDate.Day), SheetId, EpisodeNumber, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "L'épisode existe déjà");

        command.CommandText =
            """
            UPDATE TanimeDailyPlanning SET
                SheetId = $SheetId,
                Url = $Url,
                IsAdultContent = $IsAdultContent,
                IsExplicitContent = $IsExplicitContent,
                AnimeName = $AnimeName,
                ReleaseDate = $ReleaseDate,
                NoEpisode = $NoEpisode,
                EpisodeName = $EpisodeName,
                NoDay = $NoDay,
                Description = $Description,
                ThumbnailUrl = $ThumbnailUrl
            WHERE Id = $Id
            """;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", EpisodeNumber);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);
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
        if (id <= 0 || !await ExistsAsync(id, IntColumnSelect.Id, cancellationToken, command))
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

    public static async Task<OperationState> DeleteAllAsync(DateOnly date, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "DELETE FROM TanimeDailyPlanning WHERE ReleaseDate = $ReleaseDate";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$ReleaseDate", date.ToString(ReleaseDateFormat));

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

    public static async Task<OperationState> DeleteAllAsync(DateOnly minDate, DateOnly maxDate,
               CancellationToken? cancellationToken = null,
                      SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "DELETE FROM TanimeDailyPlanning WHERE ReleaseDate BETWEEN $MinDate AND $MaxDate";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        command.Parameters.AddWithValue("$MinDate", minDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$MaxDate", maxDate.ToString(ReleaseDateFormat));

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

    private static async IAsyncEnumerable<TanimeDailyPlanning> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idPlanning = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var animeSheetId = reader.GetInt32(reader.GetOrdinal("BaseSheetId"));
            var record = new TanimeDailyPlanning()
            {
                Id = idPlanning,
                SheetId = animeSheetId,
                Url = reader.GetString(reader.GetOrdinal("BaseUrl")),
                IsAdultContent = reader.GetBoolean(reader.GetOrdinal("IsAdultContent")),
                IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("IsExplicitContent")),
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
                ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("BaseThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BaseThumbnailUrl"))
            };

            var animeBase = await TanimeBase.SingleAsync((int)animeSheetId, IntColumnSelect.SheetId, cancellationToken);
            if (animeBase is not null)
                record.Anime = animeBase;

            yield return record;
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeDailyPlanning.Id AS BaseId,
            TanimeDailyPlanning.SheetId AS BaseSheetId,
            TanimeDailyPlanning.Url AS BaseUrl,
            TanimeDailyPlanning.IsAdultContent,
            TanimeDailyPlanning.IsExplicitContent,
            TanimeDailyPlanning.AnimeName AS BaseAnimeName,
            TanimeDailyPlanning.ReleaseDate,
            TanimeDailyPlanning.NoEpisode,
            TanimeDailyPlanning.EpisodeName,
            TanimeDailyPlanning.NoDay,
            TanimeDailyPlanning.Description AS BaseDescription,
            TanimeDailyPlanning.ThumbnailUrl AS BaseThumbnailUrl
        FROM
            TanimeDailyPlanning
        """;
}