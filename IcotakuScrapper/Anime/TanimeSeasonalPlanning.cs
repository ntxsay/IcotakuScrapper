using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Anime;

public enum SeasonalAnimePlanningSortBy : byte
{
    Id,
    SheetId,
    OrigineAdaptation,
    Season,
    ReleaseMonth,
    AnimeName,
    GroupName
}

public enum SeasonalAnimePlanningGroupBy : byte
{
    Default,
    OrigineAdaptation,
    Season,
    ReleaseMonth,
    GroupName,
    Category,
    Letter
}

public enum SeasonalAnimeSelectionMode : byte
{
    None,
    OrigineAdaptation,
    Season,
    ReleaseMonth,
    GroupName,
    Category,
    Letter
}

/// <summary>
/// Représente brièvement les informations d'un animé d'une saison.
/// </summary>
public partial class TanimeSeasonalPlanning
{
    private const string ReleaseDateFormat = "yyyy-MM-dd";
    public int Id { get; protected set; }
    public TorigineAdaptation? OrigineAdaptation { get; set; }
    public Tseason Season { get; set; } = new();
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
    public string GroupName { get; set; } = string.Empty;
    public uint ReleaseMonth { get; set; }
    public string? ReleaseMonthLiteral => DateHelpers.GetYearMonthLiteral(ReleaseMonth);
    public string? MinimalReleaseMonthLiteral => DateHelpers.GetYearMonthLiteral(ReleaseMonth, "MMM yyyy");
    public ushort ReleaseYear => DateHelpers.GetYear(ReleaseMonth);
    public HashSet<string> Studios { get; protected set; } = [];
    public HashSet<string> Distributors { get; protected set; } = [];
    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public TanimeSeasonalPlanning()
    {
    }

    public TanimeSeasonalPlanning(int id)
    {
        Id = id;
    }

    public void Copy(TanimeSeasonalPlanning model)
    {
        Id = model.Id;
        OrigineAdaptation = model.OrigineAdaptation;
        Season = model.Season;
        SheetId = model.SheetId;
        Url = model.Url;
        IsAdultContent = model.IsAdultContent;
        IsExplicitContent = model.IsExplicitContent;
        AnimeName = model.AnimeName;
        Anime = model.Anime;
        GroupName = model.GroupName;
        ReleaseMonth = model.ReleaseMonth;
        Studios = model.Studios;
        Distributors = model.Distributors;
        Description = model.Description;
        ThumbnailUrl = model.ThumbnailUrl;
    }

    /// <summary>
    /// Retourne le chemin d'accès vers l'affiche de l'anime.
    /// </summary>
    /// <returns></returns>
    public async Task<string?> GetThumbnailPathAsync(CancellationToken? cancellationToken = null)
    {
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;
        return await TanimeBase.GetThumbnailPathAsync(uri, cancellationToken);
    }
    
    /// <summary>
    /// Télécharge l'affiche de l'anime et/ou retourne le chemin d'accès local à l'affiche.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string?> GetOrDownloadThumbnailAsync(CancellationToken? cancellationToken = null)
    {
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;
        return await TanimeBase.GetOrDownloadThumbnailAsync(uri, cancellationToken);
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeSeasonalPlanning
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeSeasonalPlanning";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeSeasonalPlanning ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var iscolumnValidated = command.IsIntColumnValidated(columnSelect, [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
            IntColumnSelect.IdSeason,
            IntColumnSelect.IdOrigine,
            IntColumnSelect.SeasonNumber,
        ]);

        if (!iscolumnValidated)
            return 0;
        
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE SheetId = $Id",
            IntColumnSelect.IdSeason => "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE IdSeason = $Id",
            IntColumnSelect.IdOrigine => "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE IdOrigine = $Id",
            IntColumnSelect.SeasonNumber => "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE IdSeason = (SELECT Id FROM Tseason WHERE SeasonNumber = $Id)",
            _ => null
        };

        if (command.CommandText.IsStringNullOrEmptyOrWhiteSpace())
        {
            Debug.WriteLine($"La valeur de {nameof(columnSelect)} est invalide : {columnSelect}");
            return 0;
        }

        

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE Url = $Url COLLATE NOCASE";
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int idSeason, Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE IdSeason = $IdSeason AND Url = $Url COLLATE NOCASE";
        command.Parameters.AddWithValue("$IdSeason", idSeason);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int idSeason, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeSeasonalPlanning WHERE IdSeason = $IdSeason AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$IdSeason", idSeason);
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(WeatherSeason season,
        CancellationToken? cancellationToken = null)
    {
        var intSeason = season.ToIntSeason();
        if (intSeason == 0)
            return 0;

        return await CountAsync((int)intSeason, IntColumnSelect.SeasonNumber, cancellationToken);
    }

    public static async Task<int?> GetIdOfAsync(Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeSeasonalPlanning WHERE Url = $Url COLLATE NOCASE";
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(int idSeason, Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeSeasonalPlanning WHERE IdSeason = $IdSeason AND Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$IdSeason", idSeason);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(int idSeason, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeSeasonalPlanning WHERE IdSeason = $IdSeason AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$IdSeason", idSeason);
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeSeasonalPlanning WHERE SheetId = $SheetId";
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(Uri sheetUri,
        CancellationToken? cancellationToken = null)
        => await CountAsync(sheetUri, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idSeason, Uri sheetUri,
        CancellationToken? cancellationToken = null)
        => await CountAsync(idSeason, sheetUri, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idSeason, int sheetId,
        CancellationToken? cancellationToken = null)
        => await CountAsync(idSeason, sheetId, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(WeatherSeason season,
        CancellationToken? cancellationToken = null)
        => await CountAsync(season, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TanimeSeasonalPlanning[]> SelectAsync(bool? isAdultContent, bool? isExplicitContent, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript;

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "WHERE TanimeSeasonalPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
        {
            if (isAdultContent.HasValue)
                command.CommandText += Environment.NewLine + "AND TanimeSeasonalPlanning.IsExplicitContent = $IsExplicitContent";
            else
                command.CommandText += Environment.NewLine + "WHERE TanimeSeasonalPlanning.IsExplicitContent = $IsExplicitContent";
        }

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        
            

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeSeasonalPlanning[]> SelectAsync(WeatherSeason season, bool? isAdultContent, bool? isExplicitContent, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        var intSeason = season.ToIntSeason();
        if (intSeason == 0)
            return [];

        return await SelectAsync(intSeason, isAdultContent, isExplicitContent, sortBy, orderBy, limit, offset, cancellationToken);
    }

    public static async Task<TanimeSeasonalPlanning[]> SelectAsync(uint seasonNumber, bool? isAdultContent, bool? isExplicitContent, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Tseason.SeasonNumber = $SeasonNumber";

        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeSeasonalPlanning.IsAdultContent = $IsAdultContent";

        if (isExplicitContent.HasValue)
            command.CommandText += Environment.NewLine + "AND TanimeSeasonalPlanning.IsExplicitContent = $IsExplicitContent";

        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        
            

        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);

        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);

        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeSeasonalPlanning[]> SelectAsync(int idSeason, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE IdSeason = $IdSeason";
        command.AddOrderSort(sortBy, orderBy);
        command.AddLimitOffset(limit, offset);

        
            

        command.Parameters.AddWithValue("$IdSeason", idSeason);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    public static async Task<TanimeSeasonalPlanning[]> SelectAsync(DateOnly date, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE ReleaseMonth = $Date";

        command.AddOrderSort(sortBy, orderBy);

        command.AddLimitOffset(limit, offset);

        
            

        command.Parameters.AddWithValue("$Date", DateHelpers.GetYearMonthInt(date));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync();
    }

    #endregion

    #region Paginate




    #endregion

    #region Single

    public static async Task<TanimeSeasonalPlanning?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Id = $Id",
            IntColumnSelect.SheetId => "WHERE SheetId = $Id",
            _ => null
        };

        if (command.CommandText.IsStringNullOrEmptyOrWhiteSpace())
        {
            Debug.WriteLine($"La valeur de {nameof(columnSelect)} est invalide : {columnSelect}");
            return null;
        }

        
            

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
    }
    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'URL de la fiche est invalide");

        if (AnimeName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime est invalide");

        if (GroupName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom du groupe est invalide");

        if (Season.Id <= 0 || (!disableVerification && !await Tseason.ExistsAsync(Season.Id, cancellationToken)))
            return new OperationState<int>(false, "La saison est invalide");

        if (!disableVerification && await ExistsAsync(uri, cancellationToken))
            return new OperationState<int>(false, "Cet animé existe déjà");

        await using var command = Main.Connection.CreateCommand(); 
        command.CommandText =
            """
            INSERT INTO TanimeSeasonalPlanning
                (SheetId, Url, IdSeason, IdOrigine, IsAdultContent, IsExplicitContent, AnimeName, GroupName, Studios, Distributors, ReleaseMonth, Description, ThumbnailUrl)
            VALUES
                ($SheetId, $Url, $IdSeason, $IdOrigine, $IsAdultContent, $IsExplicitContent, $AnimeName, $GroupName, $Studios, $Distributors, $ReleaseMonth, $Description, $ThumbnailUrl)
            """;

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
        command.Parameters.AddWithValue("$GroupName", GroupName);
        command.Parameters.AddWithValue("$IdSeason", Season.Id);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Studios", Studios.Count > 0 ? string.Join(',', Studios) : (object)DBNull.Value);
        command.Parameters.AddWithValue("$Distributors", Distributors.Count > 0 ? string.Join(',', Distributors) : (object)DBNull.Value);
        command.Parameters.AddWithValue("$ReleaseMonth", ReleaseMonth);
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

    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TanimeSeasonalPlanning> values, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        await using var command = Main.Connection.CreateCommand();

        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeSeasonalPlanning (SheetId, Url, IdSeason, IdOrigine, IsAdultContent, IsExplicitContent, AnimeName, GroupName, Studios, Distributors, ReleaseMonth, Description, ThumbnailUrl) VALUES";
        

        for (var i = 0; i < values.Count; i++)
        {
            var planning = values.ElementAt(i);
            if (planning.AnimeName.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom de l'animé n'est pas valide ({planning}).");
                continue;
            }

            if (planning.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(planning.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            {
                LogServices.LogDebug($"L'URL de la fiche est invalide ({planning}).");
                continue;
            }

            command.CommandText += Environment.NewLine +
                                   $"($SheetId{i}, $Url{i}, $IdSeason{i}, $IdOrigine{i}, $IsAdultContent{i}, $IsExplicitContent{i}, $AnimeName{i}, $GroupName{i}, $Studios{i}, $Distributors{i}, $ReleaseMonth{i}, $Description{i}, $ThumbnailUrl{i})";

            command.Parameters.AddWithValue($"$SheetId{i}", planning.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", uri.ToString());
            command.Parameters.AddWithValue($"$IsAdultContent{i}", planning.IsAdultContent ? 1 : 0);
            command.Parameters.AddWithValue($"$IsExplicitContent{i}", planning.IsExplicitContent ? 1 : 0);
            command.Parameters.AddWithValue($"$AnimeName{i}", planning.AnimeName);
            command.Parameters.AddWithValue($"$GroupName{i}", planning.GroupName);
            command.Parameters.AddWithValue($"$IdSeason{i}", planning.Season.Id);
            command.Parameters.AddWithValue($"$IdOrigine{i}", planning.OrigineAdaptation?.Id ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$Studios{i}", planning.Studios.Count > 0 ? string.Join(',', planning.Studios) : (object)DBNull.Value);
            command.Parameters.AddWithValue($"$Distributors{i}", planning.Distributors.Count > 0 ? string.Join(',', planning.Distributors) : (object)DBNull.Value);
            command.Parameters.AddWithValue($"$ReleaseMonth{i}", planning.ReleaseMonth);
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

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'URL de la fiche est invalide");

        if (AnimeName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'anime est invalide");

        if (GroupName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom du groupe est invalide");

        if (Season.Id <= 0 || (!disableVerification && !await Tseason.ExistsAsync(Season.Id, cancellationToken)))
            return new OperationState(false, "La saison est invalide");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(uri, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "Cet animé existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TanimeSeasonalPlanning SET
                SheetId = $SheetId,
                Url = $Url,
                IsAdultContent = $IsAdultContent,
                IsExplicitContent = $IsExplicitContent,
                AnimeName = $AnimeName,
                GroupName = $GroupName,
                IdSeason = $IdSeason,
                IdOrigine = $IdOrigine,
                Studios = $Studios,
                Distributors = $Distributors,
                ReleaseMonth = $ReleaseMonth,
                Description = $Description,
                ThumbnailUrl = $ThumbnailUrl
            WHERE Id = $Id
            """;

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent ? 1 : 0);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent ? 1 : 0);
        command.Parameters.AddWithValue("$AnimeName", AnimeName);
        command.Parameters.AddWithValue("$GroupName", GroupName);
        command.Parameters.AddWithValue("$IdSeason", Season.Id);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Studios", Studios.Count > 0 ? string.Join(',', Studios) : (object)DBNull.Value);
        command.Parameters.AddWithValue("$Distributors", Distributors.Count > 0 ? string.Join(',', Distributors) : (object)DBNull.Value);
        command.Parameters.AddWithValue("$ReleaseMonth", ReleaseMonth);
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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        if (id <= 0 || !await ExistsAsync(id, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'identifiant de l'épisode est invalide");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeSeasonalPlanning WHERE Id = $Id";
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

    public static async Task<OperationState> DeleteAllAsync(WeatherSeason season, CancellationToken? cancellationToken = null)
    {
        var intSeason = season.ToIntSeason();
        if (intSeason == 0)
            return new OperationState(false, "La saison est invalide");

        return await DeleteAllAsync(intSeason, cancellationToken);
    }

    public static async Task<OperationState> DeleteAllAsync(uint intSeason, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var idSeason = await Tseason.GetIdOfAsync(intSeason, cancellationToken);
        if (!idSeason.HasValue || idSeason <= 0)
            return new OperationState(false, "La saison est invalide");

        command.CommandText = "DELETE FROM TanimeSeasonalPlanning WHERE IdSeason = $IdSeason";

        
            

        command.Parameters.AddWithValue("$IdSeason", idSeason);

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

    private static async IAsyncEnumerable<TanimeSeasonalPlanning> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<int> sheetIdArray = [];

        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idPlanning = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var animeSheetId = reader.GetInt32(reader.GetOrdinal("BaseSheetId"));

            if (sheetIdArray.Contains(animeSheetId))
                continue;
            
            var record = new TanimeSeasonalPlanning()
            {
                Id = idPlanning,
                SheetId = animeSheetId,
                Url = reader.GetString(reader.GetOrdinal("BaseUrl")),
                AnimeName = reader.GetString(reader.GetOrdinal("BaseAnimeName")),
                IsAdultContent = reader.GetBoolean(reader.GetOrdinal("IsAdultContent")),
                IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("IsExplicitContent")),
                ReleaseMonth = (uint)reader.GetInt32(reader.GetOrdinal("ReleaseMonth")),
                GroupName = reader.GetString(reader.GetOrdinal("GroupName")),
                Studios = reader.IsDBNull(reader.GetOrdinal("Studios"))
                        ? []
                        : reader.GetString(reader.GetOrdinal("Studios")).Split(',').ToHashSet(),
                Distributors = reader.IsDBNull(reader.GetOrdinal("Distributors"))
                        ? []
                        : reader.GetString(reader.GetOrdinal("Distributors")).Split(',').ToHashSet(),
                Description = reader.IsDBNull(reader.GetOrdinal("BaseDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BaseDescription")),
                ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("BaseThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BaseThumbnailUrl")),
                OrigineAdaptation = reader.IsDBNull(reader.GetOrdinal("IdOrigine"))
                        ? null
                        : TorigineAdaptation.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdOrigine"),
                            sectionIndex: reader.GetOrdinal("OrigineAdaptationSection"),
                            nameIndex: reader.GetOrdinal("OrigineAdaptationName"),
                            descriptionIndex: reader.GetOrdinal("OrigineAdaptationDescription")),
                Season = Tseason.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdSeason"),
                            displayNameIndex: reader.GetOrdinal("SeasonDisplayName"),
                            seasonNumberIndex: reader.GetOrdinal("SeasonNumber")),

            };

            var animeBase = await TanimeBase.SingleAsync(animeSheetId, IntColumnSelect.SheetId, cancellationToken);
            if (animeBase is not null)
                record.Anime = animeBase;

            yield return record;
            
            sheetIdArray.Add(animeSheetId);
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            DISTINCT TanimeSeasonalPlanning.Id AS BaseId,
            TanimeSeasonalPlanning.IdSeason AS IdSeason,
            TanimeSeasonalPlanning.IdOrigine AS IdOrigine,
            TanimeSeasonalPlanning.SheetId AS BaseSheetId,
            TanimeSeasonalPlanning.Url AS BaseUrl,
            TanimeSeasonalPlanning.AnimeName AS BaseAnimeName,
            TanimeSeasonalPlanning.IsAdultContent,
            TanimeSeasonalPlanning.IsExplicitContent,
            TanimeSeasonalPlanning.ReleaseMonth,
            TanimeSeasonalPlanning.GroupName,
            TanimeSeasonalPlanning.Studios,
            TanimeSeasonalPlanning.Distributors,
            TanimeSeasonalPlanning.Description AS BaseDescription,
            TanimeSeasonalPlanning.ThumbnailUrl AS BaseThumbnailUrl,
            
            TorigineAdaptation.Name as OrigineAdaptationName,
            TorigineAdaptation.Section as OrigineAdaptationSection,
            TorigineAdaptation.Description as OrigineAdaptationDescription,
            
            Tseason.DisplayName as SeasonDisplayName,
            Tseason.SeasonNumber as SeasonNumber
        FROM
            TanimeSeasonalPlanning
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = TanimeSeasonalPlanning.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = TanimeSeasonalPlanning.IdSeason
        LEFT JOIN main.Tanime on Tanime.SheetId = TanimeSeasonalPlanning.SheetId
        LEFT JOIN main.TanimeCategory on TanimeCategory.IdAnime = Tanime.Id
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        
        """;

    private const string SqlCountScript =
        """
        SELECT
            COUNT(DISTINCT TanimeSeasonalPlanning.Id)
        FROM
            TanimeSeasonalPlanning
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = TanimeSeasonalPlanning.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = TanimeSeasonalPlanning.IdSeason
        LEFT JOIN main.Tanime on Tanime.SheetId = TanimeSeasonalPlanning.SheetId
        LEFT JOIN main.TanimeCategory on TanimeCategory.IdAnime = Tanime.Id
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        
        """;
}