using System.Diagnostics;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public enum AnimeSortBy
{
    Id,
    Name,
    SheetId,
    EpisodesCount,
    ReleaseDate,
    EndDate,
    Duration,
    Format,
    Target,
    OrigineAdaptation
}

public partial class Tanime : TanimeBase
{
    

    /// <summary>
    /// Obtient ou définit la date de sortie de l'anime au format yyyy-MM-dd.
    /// </summary>
    public string? ReleaseDate { get; set; }

    /// <summary>
    /// Obtient ou définit la date de fin de l'anime.
    /// </summary>
    public string? EndDate { get; set; }

    /// <summary>
    /// Obtient ou définit la durée d'un épisode de l'anime (en minutes).
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    
    /// <summary>
    /// Obtient ou définit la liste des titres alternatifs de l'anime.
    /// </summary>
    public HashSet<TanimeAlternativeTitle> AlternativeTitles { get; } = new();
    
    /// <summary>
    /// Obtient ou définit la liste des sites web de l'anime.
    /// </summary>
    public HashSet<TanimeWebSite> WebSites { get; } = new();
    
    /// <summary>
    /// Obtient ou définit la liste des studios de l'anime.
    /// </summary>
    public HashSet<Tcontact> Studios { get; } = new();
    
    /// <summary>
    /// Obtient ou définit la liste des épisodes de l'anime.
    /// </summary>
    public HashSet<TanimeEpisode> Episodes { get; } = new();


    /// <summary>
    /// Retourne la date de sortie de l'anime via l'objet <see cref="DateOnly"/>.
    /// </summary>
    /// <returns></returns>
    public DateOnly? GetReleaseDate()
    {
        if (ReleaseDate == null || ReleaseDate.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var date = ReleaseDate.Split('-');
        if (date.Length != 3)
            return null;

        if (!ushort.TryParse(date[0], out var year))
            return null;

        if (!ushort.TryParse(date[1], out var month))
            return null;

        if (!ushort.TryParse(date[2], out var day))
            return null;

        if (DateOnly.TryParse($"{year}-{month}-{day}", out var result))
            return result;
        
        return null;
    }

    public Tanime()
    {
        
    }

    public Tanime(int id)
    {
        Id = id;
    }

    public override string ToString() => $"{Name} ({Id}/{SheetId})";

    

    private static async Task<OperationState<int>> CreateIndexAsync(string animeName, string animeUrl, int animeSheetId, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await TsheetIndex.InsertAsync(IcotakuSection.Anime, SheetType.Anime, animeName, animeUrl, animeSheetId, 0, cancellationToken, cmd);
    


    #region Select

    public static async Task<Tanime[]> SelectAsync(bool? isAdultContent, bool? isExplicitContent, AnimeSortBy sortBy, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript;
        
        if (isAdultContent.HasValue)
            command.CommandText += Environment.NewLine + "AND Tanime.IsAdultContent = $IsAdultContent";
        
        if (isExplicitContent.HasValue)
            command.CommandText += Environment.NewLine + "AND Tanime.IsExplicitContent = $IsExplicitContent";
        
        AddSortOrderBy(command, sortBy, orderBy);
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        if (isAdultContent.HasValue)
            command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value);
        
        if (isExplicitContent.HasValue)
            command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<Tanime>();
        
        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Single

    public new static async Task<Tanime?> SingleAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += columnSelect switch
        {
            SheetIntColumnSelect.Id => "WHERE Tanime.Id = $Id",
            SheetIntColumnSelect.SheetId => "WHERE Tanime.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public new static async Task<Tanime?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tanime.Name = $Name COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Name", name);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public new static async Task<Tanime?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tanime.Url = $Url COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime ne peut pas être vide");
        
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");
        
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");
        
        if (SheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche icotaku n'est pas valide");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        if (await ExistsAsync(Name, SheetId, uri, cancellationToken, command))
            return new OperationState<int>(false, "L'anime existe déjà");
        
        
        command.CommandText = 
            """
            INSERT INTO Tanime 
                (SheetId, Url, IsAdultContent, IsExplicitContent, Name, DiffusionState, EpisodeCount, EpisodeDuration, ReleaseDate, EndDate, Description, ThumbnailMiniUrl, ThumbnailUrl, IdFormat, IdTarget, IdOrigine, IdSeason) 
            VALUES 
                ($SheetId, $Url, $IsAdultContent, $IsExplicitContent, $Name, $DiffusionState , $EpisodeCount, $EpisodeDuration, $ReleaseDate, $EndDate, $Description, $ThumbnailMiniUrl, $ThumbnailUrl, $IdFormat, $IdTarget, $IdOrigine, $IdSeason)
            """;
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$Name", Name);
        command.Parameters.AddWithValue("$DiffusionState", (byte)DiffusionState);
        command.Parameters.AddWithValue("$EpisodeCount", EpisodesCount);
        command.Parameters.AddWithValue("$EpisodeDuration", Duration.TotalMinutes);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$EndDate", EndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailMiniUrl", ThumbnailMiniUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdFormat", Format?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdTarget", Target?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdSeason", Season?.Id ?? (object)DBNull.Value);
        
        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result <= 0)
                return new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
            Id = await command.GetLastInsertRowIdAsync();
            
            if (AlternativeTitles.Count > 0)
                foreach (var title in AlternativeTitles)
                {
                    title.IdAnime = Id;
                    _ = await title.InsertAsync(cancellationToken, command);
                }
            
            if (WebSites.Count > 0)
                foreach (var webSite in WebSites)
                {
                    webSite.IdAnime = Id;
                    _ = await webSite.InsertAsync(cancellationToken, command);
                }
            
            if (Studios.Count > 0)
                _ = await TanimeStudio.InsertAsync(Id, Studios.Select(s => s.Id).ToArray(), cancellationToken, command);

            if (Categories.Count > 0)
                _ = await TanimeCategory.InsertAsync(Id, Categories.Select(s => s.Id).ToArray(), cancellationToken, command);

            if (Episodes.Count > 0)
                _ = await TanimeEpisode.InsertAsync(Id, Episodes.Where(w => w.EpisodeNumber >= 0).DistinctBy(d => d.EpisodeNumber).ToArray(), cancellationToken, command);

            return new OperationState<int>(true, "L'anime a été ajouté avec succès", Id);

        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
        }
    }
    
    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'anime ne peut pas être vide");
        
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url de la fiche de l'anime ne peut pas être vide");
        
        if (SheetId <= 0)
            return new OperationState(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");
        
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'url de la fiche de l'anime n'est pas valide");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (Id <= 0 || !await ExistsAsync(Id, SheetIntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'id de l'anime ne peut pas être inférieur ou égal à 0");
        
        var existingId = await GetIdOfAsync(Name, SheetId, uri, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "L'url de la fiche de l'anime existe déjà");
        
        command.CommandText = 
            """
            UPDATE Tanime SET 
                SheetId = $SheetId, 
                Url = $Url, 
                IsAdultContent = $IsAdultContent,
                IsExplicitContent = $IsExplicitContent,
                Name = $Name, 
                DiffusionState = $DiffusionState,
                EpisodeCount = $EpisodeCount, 
                EpisodeDuration = $EpisodeDuration, 
                ReleaseDate = $ReleaseDate, 
                EndDate = $EndDate, 
                Description = $Description, 
                ThumbnailMiniUrl = $ThumbnailMiniUrl, 
                ThumbnailUrl = $ThumbnailUrl, 
                IdFormat = $IdFormat, 
                IdTarget = $IdTarget, 
                IdOrigine = $IdOrigine,
                IdSeason = $IdSeason
            WHERE Id = $Id
            """;
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$Name", Name);
        command.Parameters.AddWithValue("$DiffusionState", (byte)DiffusionState);
        command.Parameters.AddWithValue("$EpisodeCount", EpisodesCount);
        command.Parameters.AddWithValue("$EpisodeDuration", Duration.TotalMinutes);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$EndDate", EndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailMiniUrl", ThumbnailMiniUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdFormat", Format?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdTarget", Target?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdSeason", Season?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Id", Id);
        
        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result > 0 
                ? new OperationState(true, "L'anime a été modifié avec succès") 
                : new OperationState(false, "Une erreur est survenue lors de la modification de l'anime");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la modification de l'anime");
        }
    }

    #endregion

    #region Delete
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, SheetIntColumnSelect.Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeAlternativeTitle WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM tanimeWebSite WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM tanimeStudio WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM tanimeLicence WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM tanimeStaff WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM tanimeCharacter WHERE IdAnime = $Id;";
        command.CommandText += Environment.NewLine + "DELETE FROM Tanime WHERE Id = $Id;";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        try
        {
            var countAffectedRows = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countAffectedRows} lignes affectées");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'anime");
        }
    }

    public static async Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var id = await GetIdOfAsync(uri, cancellationToken, command);
        if (!id.HasValue)
            return new OperationState(false, "L'anime n'a pas été trouvé");

        return await DeleteAsync(id.Value, SheetIntColumnSelect.Id, cancellationToken, command);
    }

    #endregion

    private static async Task<Tanime[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<Tanime> animeList = new();
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var animeId = reader.GetInt32(reader.GetOrdinal("AnimeId"));
            var anime = animeList.FirstOrDefault(x => x.Id == animeId);
            if (anime == null)
            {
                anime = new Tanime(animeId)
                {
                    Name = reader.GetString(reader.GetOrdinal("AnimeName")),
                    Url = reader.GetString(reader.GetOrdinal("AnimeUrl")),
                    IsAdultContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsAdultContent")),
                    IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsExplicitContent")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("AnimeSheetId")),
                    Duration = TimeSpan.FromMinutes(reader.GetInt32(reader.GetOrdinal("EpisodeDuration"))),
                    DiffusionState = (DiffusionStateKind)reader.GetByte( reader.GetOrdinal("DiffusionState")),
                    ReleaseDate = reader.IsDBNull(reader.GetOrdinal("ReleaseDate"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ReleaseDate")),
                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("EndDate")),
                    EpisodesCount = (ushort)reader.GetInt16(reader.GetOrdinal("EpisodeCount")),
                    Description = reader.IsDBNull(reader.GetOrdinal("AnimeDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeDescription")),
                    ThumbnailMiniUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailMiniUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ThumbnailMiniUrl")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ThumbnailUrl")),
                    Format = reader.IsDBNull(reader.GetOrdinal("IdFormat"))
                        ? null
                        : Tformat.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdFormat"),
                            sectionIndex: reader.GetOrdinal("FormatSection"),
                            nameIndex: reader.GetOrdinal("FormatName"),
                            descriptionIndex: reader.GetOrdinal("FormatDescription")),
                    Target = reader.IsDBNull(reader.GetOrdinal("IdTarget"))
                        ? null
                        : Ttarget.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdTarget"),
                            sectionIndex: reader.GetOrdinal("TargetSection"),
                            nameIndex: reader.GetOrdinal("TargetName"),
                            descriptionIndex: reader.GetOrdinal("TargetDescription")),
                    OrigineAdaptation = reader.IsDBNull(reader.GetOrdinal("IdOrigine"))
                        ? null
                        : TorigineAdaptation.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdOrigine"),
                            sectionIndex: reader.GetOrdinal("OrigineAdaptationSection"),
                            nameIndex: reader.GetOrdinal("OrigineAdaptationName"),
                            descriptionIndex: reader.GetOrdinal("OrigineAdaptationDescription")),
                    Season = reader.IsDBNull(reader.GetOrdinal("IdSeason"))
                        ? null
                        : Tseason.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdSeason"),
                            displayNameIndex: reader.GetOrdinal("SeasonDisplayName"),
                            seasonNumberIndex: reader.GetOrdinal("SeasonNumber")),
                };

                var episodes = await TanimeEpisode.SelectAsync(animeId, AnimeEpisodeSortBy.EpisodeNumber, OrderBy.Asc);
                if (episodes.Length > 0)
                    foreach (var episode in episodes)
                        anime.Episodes.Add(episode);

                animeList.Add(anime);
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("AlternativeTitleId")))
            {
                var alternativeTitleId = reader.GetInt32(reader.GetOrdinal("AlternativeTitleId"));
                var alternativeTitle = anime.AlternativeTitles.FirstOrDefault(x => x.Id == alternativeTitleId);
                if (alternativeTitle == null)
                {
                    alternativeTitle = new TanimeAlternativeTitle(alternativeTitleId, animeId)
                    {
                        Title = reader.GetString(reader.GetOrdinal("AlternativeTitle")),
                        Description = reader.IsDBNull(reader.GetOrdinal("AlternativeTitleDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("AlternativeTitleDescription"))
                    };
                    anime.AlternativeTitles.Add(alternativeTitle);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("WebSiteId")))
            {
                var webSiteId = reader.GetInt32(reader.GetOrdinal("WebSiteId"));
                var webSite = anime.WebSites.FirstOrDefault(x => x.Id == webSiteId);
                if (webSite == null)
                {
                    webSite = new TanimeWebSite(webSiteId, animeId)
                    {
                        Url = reader.GetString(reader.GetOrdinal("WebSiteUrl")),
                        Description = reader.IsDBNull(reader.GetOrdinal("WebSiteDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("WebSiteDescription"))
                    };
                    anime.WebSites.Add(webSite);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("CategoryId")))
            {
                var categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                var category = anime.Categories.FirstOrDefault(x => x.Id == categoryId);
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
                    anime.Categories.Add(category);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("StudioId")))
            {
                var studioId = reader.GetInt32(reader.GetOrdinal("StudioId"));
                var studio = anime.Studios.FirstOrDefault(x => x.Id == studioId);
                if (studio == null)
                {
                    studio = await Tcontact.SingleAsync(studioId, SheetIntColumnSelect.Id, cancellationToken);
                    if (studio != null)
                        anime.Studios.Add(studio);
                }
            }

            
        }
        
        return animeList.ToArray();
    }

    internal static void AddSortOrderBy(SqliteCommand command, AnimeSortBy sortBy, OrderBy orderBy)
    {
        command.CommandText += Environment.NewLine;
        command.CommandText += sortBy switch
        {
            AnimeSortBy.Id => $" ORDER BY Tanime.Id {orderBy}",
            AnimeSortBy.Name => $" ORDER BY Tanime.Name {orderBy}",
            AnimeSortBy.Duration => $" ORDER BY Tanime.Duration {orderBy}",
            AnimeSortBy.OrigineAdaptation => $" ORDER BY TorigineAdaptation.Name {orderBy}",
            AnimeSortBy.SheetId => $" ORDER BY Tanime.SheetId {orderBy}",
            AnimeSortBy.Target => $" ORDER BY Ttarget.Name {orderBy}",
            AnimeSortBy.EpisodesCount => $" ORDER BY Tanime.EpisodeCount {orderBy}",
            AnimeSortBy.EndDate => $" ORDER BY Tanime.EndDate {orderBy}",
            AnimeSortBy.Format => $" ORDER BY Tformat.Name {orderBy}",
            AnimeSortBy.ReleaseDate => $" ORDER BY Tanime.ReleaseDate {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    private const string SqlSelectScript =
        """
        SELECT
            Tanime.Id AS AnimeId,
            Tanime.IdTarget,
            Tanime.IdFormat,
            Tanime.IdOrigine,
            Tanime.IdSeason,
            Tanime.SheetId AS AnimeSheetId,
            Tanime.Url AS AnimeUrl,
            Tanime.IsAdultContent AS AnimeIsAdultContent,
            Tanime.IsExplicitContent AS AnimeIsExplicitContent,
            Tanime.Name AS AnimeName,
            Tanime.EpisodeCount,
            Tanime.EpisodeDuration,
            Tanime.ReleaseDate,
            Tanime.EndDate,
            Tanime.DiffusionState,
            Tanime.Description AS AnimeDescription,
            Tanime.ThumbnailMiniUrl,
            Tanime.ThumbnailUrl,
            Tanime.Remark,
            
            Tformat.Name as FormatName,
            Tformat.Section as FormatSection,
            Tformat.Description as FormatDescription,
            
            Ttarget.Name as TargetName,
            Ttarget.Section as TargetSection,
            Ttarget.Description as TargetDescription,
            
            TorigineAdaptation.Name as OrigineAdaptationName,
            TorigineAdaptation.Section as OrigineAdaptationSection,
            TorigineAdaptation.Description as OrigineAdaptationDescription,
            
            Tseason.DisplayName as SeasonDisplayName,
            Tseason.SeasonNumber as SeasonNumber,
            
            TanimeAlternativeTitle.Id AS AlternativeTitleId,
            TanimeAlternativeTitle.Name AS AlternativeTitle,
            TanimeAlternativeTitle.Description AS AlternativeTitleDescription,
            
            TanimeWebSite.Id AS WebSiteId,
            TanimeWebSite.Url AS WebSiteUrl,
            TanimeWebSite.Description AS WebSiteDescription,
            
            TanimeStudio.IdStudio AS StudioId,
            
            TanimeCategory.IdCategory AS CategoryId,
            Tcategory.SheetId AS CategorySheetId,
            Tcategory.Type AS CategoryType,
            Tcategory.Url AS CategoryUrl,
            Tcategory.Section AS CategorySection,
            Tcategory.Name AS CategoryName,
            Tcategory.Description AS CategoryDescription
        
        FROM Tanime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeAlternativeTitle TanimeAlternativeTitle on Tanime.Id = TanimeAlternativeTitle.IdAnime
        LEFT JOIN main.TanimeWebSite on Tanime.Id = TanimeWebSite.IdAnime
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        LEFT JOIN main.TanimeStudio on Tanime.Id = TanimeStudio.IdAnime
        """;
}