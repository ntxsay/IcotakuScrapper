using System.Diagnostics;
using System.Globalization;
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
    /// Obtient ou définit la liste des épisodes de l'anime.
    /// </summary>
    public HashSet<TanimeEpisode> Episodes { get; } = [];
    

    public Tanime()
    {
        
    }
    
    public Tanime(TanimeBase animeBase)
    {
        Id = animeBase.Id;
        Guid = animeBase.Guid;
        Name = animeBase.Name;
        Url = animeBase.Url;
        IsAdultContent = animeBase.IsAdultContent;
        IsExplicitContent = animeBase.IsExplicitContent;
        VoteCount = animeBase.VoteCount;
        SheetId = animeBase.SheetId;
        Duration = animeBase.Duration;
        DiffusionState = animeBase.DiffusionState;
        ReleaseDate = animeBase.ReleaseDate;
        EndDate = animeBase.EndDate;
        EpisodesCount = animeBase.EpisodesCount;
        Note = animeBase.Note;
        Description = animeBase.Description;
        Remark = animeBase.Remark;
        ThumbnailUrl = animeBase.ThumbnailUrl;
        Format = animeBase.Format;
        Target = animeBase.Target;
        OrigineAdaptation = animeBase.OrigineAdaptation;
        Season = animeBase.Season;
        AlternativeTitles.ToObservableCollection(animeBase.AlternativeTitles, true);
        Websites.ToObservableCollection(animeBase.Websites, true);
    }

    public Tanime(int id)
    {
        Id = id;
    }

    public override string ToString() => $"{Name} ({Id}/{SheetId})";

    

    private static async Task<OperationState<int>> CreateIndexAsync(string animeName, string animeUrl, int animeSheetId, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await TsheetIndex.InsertAsync(IcotakuSection.Anime, IcotakuSheetType.Anime, animeName, animeUrl, animeSheetId, 0, cancellationToken, cmd);
    


    #region Select

    public static async Task<Tanime[]> SelectAsync(bool? isAdultContent, bool? isExplicitContent, AnimeSortBy sortBy, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript;
        command.Parameters.Clear();

        command.AddExplicitContentFilter(DbStartFilterMode.Where, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", isAdultContent, isExplicitContent);

        AddSortOrderBy(command, sortBy, orderBy);
        
        command.AddLimitOffset(limit, skip);

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Single

    /// <summary>
    /// Retournes un anime via son id SQLite.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tanime?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await SingleAsync(id, IntColumnSelect.Id, cancellationToken, cmd);
    
    /// <summary>
    /// Retournes un anime via son id de fiche icotaku.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tanime?> SingleBySheetIdAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await SingleAsync(id, IntColumnSelect.SheetId, cancellationToken, cmd);
    
    /// <summary>
    /// Retournes un anime
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public new static async Task<Tanime?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);

        if (!isColumnSelectValid)
        {
            return null;
        }

        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Tanime.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tanime.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", null, null);
        
        command.Parameters.AddWithValue("$Id", id);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).SingleOrDefault();
    }
    
    /// <summary>
    /// Retournes un anime via son nom.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public new static async Task<Tanime?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Tanime.Name = $Name COLLATE NOCASE";
        command.Parameters.Clear();

        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", null, null);
        command.Parameters.AddWithValue("$Name", name);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public new static async Task<Tanime?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Tanime.Url = $Url COLLATE NOCASE";
        command.Parameters.Clear();

        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", null, null);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    #endregion

    #region Insert

    public new async Task<OperationState<int>> InsertAync(bool disableExistenceVerification = false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var insertBaseResult = await base.InsertAync(disableExistenceVerification, cancellationToken, cmd);
        if (!insertBaseResult.IsSuccess || insertBaseResult.Data <= 0)
            return insertBaseResult;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        await this.AddOrReplaceAlternativeTitlesAsync(cancellationToken, command);
        await this.AddOrReplaceWebsitesAsync(cancellationToken, command);
        await this.AddOrReplaceStudiosAsync(cancellationToken, command);
        await this.AddOrReplaceCategoriesAsync(cancellationToken, command);
        await this.AddOrReplaceEpisodesAsync(cancellationToken, command);
        await this.AddOrReplaceLicensesAsync(cancellationToken, command);
        await this.AddOrReplaceStaffsAsync(cancellationToken, command);
            
        return new OperationState<int>(true, "L'anime a été ajouté avec succès", insertBaseResult.Data);
    }
    
    #endregion

    #region Update

    public new async Task<OperationState> UpdateAsync(bool disableExistenceVerification = false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var updateBaseResult = await base.UpdateAsync(disableExistenceVerification, cancellationToken, cmd);
        if (!updateBaseResult.IsSuccess)
            return updateBaseResult;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        await this.UpdateAlternativeTitlesAsync(cancellationToken, command);
        await this.UpdateWebsitesAsync(cancellationToken, command);
        await this.UpdateStudiosAsync(cancellationToken, command);
        await this.UpdateCategoriesAsync(cancellationToken, command);
        await this.UpdateEpisodesAsync(cancellationToken, command);
        await this.UpdateLicensesAsync(cancellationToken, command);
        await this.UpdateStaffsAsync(cancellationToken, command);
            
        return new OperationState(true, "L'anime a été mis à jour avec succès") ;
    }

    #endregion

    #region Add or Update or Single

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await AddOrUpdateAsync(this, cancellationToken, cmd);
    public static async Task<OperationState<int>> AddOrUpdateAsync(Tanime value,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime ne peut pas être vide");
        
        if (value.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");
        
        if (value.SheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");
        
        if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.Name, value.SheetId, uri, cancellationToken, cmd);
        
        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState<int>(false, "Le nom de l'item existe déjà");
            
            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;
            return (await value.UpdateAsync(true, cancellationToken, cmd)).ToGenericState(value.Id);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAync(true, cancellationToken, cmd);
        if (addResult.IsSuccess)
            value.Id = addResult.Data;
        
        return addResult;
    }

    #endregion
    
    private static async Task<Tanime[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<Tanime> animeList = [];
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var animeId = reader.GetInt32(reader.GetOrdinal("AnimeId"));
            var anime = animeList.FirstOrDefault(x => x.Id == animeId);
            if (anime == null)
            {
                anime = new Tanime(animeId)
                {
                    Guid = DbHelpers.ConvertStringSqliteToGuid(reader.GetString(reader.GetOrdinal("AnimeGuid"))),
                    Name = reader.GetString(reader.GetOrdinal("AnimeName")),
                    Url = reader.GetString(reader.GetOrdinal("AnimeUrl")),
                    IsAdultContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsAdultContent")),
                    IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsExplicitContent")),
                    VoteCount = (uint)reader.GetInt32(reader.GetOrdinal("AnimeVoteCount")),
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
                    Note = reader.IsDBNull(reader.GetOrdinal("AnimeNote"))
                        ? null
                        : reader.GetDouble(reader.GetOrdinal("AnimeNote")),
                    Description = reader.IsDBNull(reader.GetOrdinal("AnimeDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeDescription")),
                    Remark = reader.IsDBNull(reader.GetOrdinal("AnimeRemark"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeRemark")),
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
                var webSite = anime.Websites.FirstOrDefault(x => x.Id == webSiteId);
                if (webSite == null)
                {
                    webSite = new TanimeWebSite(webSiteId, animeId)
                    {
                        Url = reader.GetString(reader.GetOrdinal("WebSiteUrl")),
                        Description = reader.IsDBNull(reader.GetOrdinal("WebSiteDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("WebSiteDescription"))
                    };
                    anime.Websites.Add(webSite);
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
                    studio = await TcontactBase.SingleAsync(studioId, IntColumnSelect.Id, cancellationToken);
                    if (studio != null)
                        anime.Studios.Add(studio);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("LicenseId")))
            {
                var licenseId = reader.GetInt32(reader.GetOrdinal("LicenseId"));
                var license = anime.Licenses.FirstOrDefault(x => x.Id == licenseId);
                if (license == null)
                {
                    license = await TanimeLicense.SingleAsync(licenseId, cancellationToken);
                    if (license != null)
                        anime.Licenses.Add(license);
                }
            }

            if (!reader.IsDBNull(reader.GetOrdinal("StaffId")))
            {
                var staffId = reader.GetInt32(reader.GetOrdinal("StaffId"));
                var staff = anime.Staffs.FirstOrDefault(x => x.Id == staffId);
                if (staff == null)
                {
                    staff = await TanimeStaff.SingleAsync(staffId, cancellationToken);
                    if (staff != null)
                        anime.Staffs.Add(staff);
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
            Tanime.Guid AS AnimeGuid,
            Tanime.IdTarget,
            Tanime.IdFormat,
            Tanime.IdOrigine,
            Tanime.IdSeason,
            Tanime.SheetId AS AnimeSheetId,
            Tanime.Url AS AnimeUrl,
            Tanime.IsAdultContent AS AnimeIsAdultContent,
            Tanime.IsExplicitContent AS AnimeIsExplicitContent,
            Tanime.Note AS AnimeNote,
            Tanime.VoteCount AS AnimeVoteCount,
            Tanime.Name AS AnimeName,
            Tanime.EpisodeCount,
            Tanime.EpisodeDuration,
            Tanime.ReleaseDate,
            Tanime.EndDate,
            Tanime.DiffusionState,
            Tanime.Description AS AnimeDescription,
            Tanime.ThumbnailUrl,
            Tanime.Remark AS AnimeRemark,
            
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
            TanimeAlternativeTitle.Title AS AlternativeTitle,
            TanimeAlternativeTitle.Description AS AlternativeTitleDescription,
            
            TanimeWebSite.Id AS WebSiteId,
            TanimeWebSite.Url AS WebSiteUrl,
            TanimeWebSite.Description AS WebSiteDescription,
            
            TanimeStudio.IdStudio AS StudioId,
            TanimeLicense.Id AS LicenseId,
            TanimeStaff.Id AS StaffId,
            
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
        LEFT JOIN main.TanimeLicense on Tanime.Id = TanimeLicense.IdAnime
        LEFT JOIN main.TanimeStaff on Tanime.Id = TanimeStaff.IdAnime
        """;
}