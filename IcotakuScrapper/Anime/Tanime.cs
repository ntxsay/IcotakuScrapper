﻿using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;
using IcotakuScrapper.Objects.Exceptions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public enum AnimeSortBy
{
    Id,
    Name,
    SheetId,
    EpisodesCount,
    ReleaseMonth,
    EndDate,
    Duration,
    Format,
    Target,
    OrigineAdaptation,
    Season,
}

public enum AnimeGroupBy
{
    Default,
    ReleaseMonth,
    Format,
    Target,
    Letter,
    OrigineAdaptation,
    Season,
    Categories
}

public enum AnimeSelectionMode : byte
{
    None,
    OrigineAdaptation,
    Season,
    ReleaseMonth,
    Format,
    Category,
    Letter
}

public partial class Tanime : TanimeBase
{
    /// <summary>
    /// Obtient ou définit la liste des épisodes de l'anime.
    /// </summary>
    public HashSet<Tepisode> Episodes { get; } = [];
    

    public Tanime()
    {
        
    }

    public Tanime(int id)
    {
        Id = id;
    }

    public override string ToString() => $"{Name} ({Id}/{SheetId})";

    public void Copy(Tanime value)
    {
        Id = value.Id;
        Guid = value.Guid;
        Name = value.Name;
        Url = value.Url;
        IsAdultContent = value.IsAdultContent;
        IsExplicitContent = value.IsExplicitContent;
        VoteCount = value.VoteCount;
        SheetId = value.SheetId;
        Duration = value.Duration;
        DiffusionState = value.DiffusionState;
        ReleaseDate = value.ReleaseDate;
        ReleaseMonth = value.ReleaseMonth;
        EndDate = value.EndDate;
        EpisodesCount = value.EpisodesCount;
        Note = value.Note;
        Description = value.Description;
        Remark = value.Remark;
        ThumbnailUrl = value.ThumbnailUrl;
        Format = value.Format?.Clone();
        Target = value.Target?.Clone();
        OrigineAdaptation = value.OrigineAdaptation?.Clone();
        Season = value.Season;
        Statistic = value.Statistic?.Clone();
        AlternativeTitles.ToObservableCollection(value.AlternativeTitles, true);
        Websites.ToObservableCollection(value.Websites, true);
        Categories.ToObservableCollection(value.Categories, true);
        Studios.ToObservableCollection(value.Studios, true);
        Licenses.ToObservableCollection(value.Licenses, true);
        Staffs.ToObservableCollection(value.Staffs, true);
        Episodes.ToObservableCollection(value.Episodes, true);
    }
    
    public new Tanime Clone()
    {
        var clone = new Tanime();
        clone.Copy(this);
        return clone;
    }

    #region Scrap

    #region Scrap from sheetId

    /// <summary>
    /// Récupère les informations de l'anime via l'id Icotaku de la fiche
    /// </summary>
    /// <param name="sheetId">Id Icotaku de la fiche</param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromSheetIdAsync(int sheetId, AnimeScrapingOptions options = AnimeScrapingOptions.Default,
        CancellationToken? cancellationToken = null)
    {
        //Récupère l'url de l'animé depuis le dictionnaire
        var index = await TsheetIndex.SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);
        if (index == null)
            return new OperationState<int>(false, "L'index permettant de récupérer l'url de la fiche de l'anime n'a pas été trouvé dans la base de données.");

        if (!Uri.TryCreate(index.Url, UriKind.Absolute, out var sheetUri) || !sheetUri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime est invalide.");

        return await ScrapFromUrlAsync(sheetUri, options, cancellationToken);
    }

    public static async Task<Tanime?> ScrapAndGetFromSheetIdAsync(int sheetId, AnimeScrapingOptions options = AnimeScrapingOptions.Default,
        CancellationToken? cancellationToken = null)
    {
        var operationResult = await ScrapFromSheetIdAsync(sheetId, options, cancellationToken);
        if (!operationResult.IsSuccess)
            return null;

        var anime = await SingleByIdAsync(operationResult.Data, cancellationToken);
        return anime;
    }
    #endregion

    #region Scrap from Url

    /// <summary>
    /// Récupère les informations de l'anime via l'url de la fiche
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="userName"></param>
    /// <param name="passWord"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, string userName, string passWord, AnimeScrapingOptions options = AnimeScrapingOptions.Default,
        CancellationToken? cancellationToken = null)
    {
        var htmlContent = await IcotakuWebHelpers.GetRestrictedHtmlAsync(IcotakuSection.Anime, sheetUri, userName, passWord);
        if (htmlContent == null || htmlContent.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le contenu de la fiche est introuvable.");

        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        var animeResult = await ScrapAnimeAsync(htmlContent, sheetUri, options, cancellationToken);
        
        //Si le scraping a échoué alors on sort de la méthode en retournant le message d'erreur
        if (!animeResult.IsSuccess || animeResult.Data == null)
            return new OperationState<int>(false, animeResult.Message);
        
        return await animeResult.Data.AddOrUpdateAsync(cancellationToken);
    }

    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, AnimeScrapingOptions options = AnimeScrapingOptions.All,
        CancellationToken? cancellationToken = null)
    {
        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        var animeResult = await ScrapAnimeAsync(sheetUri, options, cancellationToken);
        
        //Si le scraping a échoué alors on sort de la méthode en retournant le message d'erreur
        if (!animeResult.IsSuccess || animeResult.Data == null)
            return new OperationState<int>(false, animeResult.Message);
        
        return await animeResult.Data.AddOrUpdateAsync(cancellationToken);
    }

    /// <summary>
    /// Scrappe la fiche anime à partir de son url
    /// </summary>
    /// <param name="sheetUri">Url absolue de la fiche</param>
    /// <param name="options">Options de scraping</param>
    /// <param name="cancellationToken"></param>
    /// <returns>L'objet Tanime sinon null</returns>
    public static async Task<Tanime?> ScrapAndGetFromUrlAsync(Uri sheetUri, AnimeScrapingOptions options = AnimeScrapingOptions.All,
        CancellationToken? cancellationToken = null)
    {
        //Scrappe la fiche anime
        var operationResult = await ScrapFromUrlAsync(sheetUri, options, cancellationToken);
        
        //Si le scraping a échoué alors on sort de la méthode en retournant null
        if (!operationResult.IsSuccess)
            return null;

        //Si le scraping a réussi alors on retourne l'anime depuis la base de données via son id
        var anime = await SingleByIdAsync(operationResult.Data, cancellationToken);
        return anime;
    }
    #endregion

    #endregion

    #region Select

    public static async Task<Tanime[]> SelectAsync(bool? isAdultContent, bool? isExplicitContent, AnimeSortBy sortBy, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript;

        command.AddExplicitContentFilter(DbStartFilterMode.Where, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", isAdultContent, isExplicitContent);

        command.AddOrderSort(sortBy, orderBy);
        
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
    /// <returns></returns>
    public new static async Task<Tanime?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await SingleAsync(id, IntColumnSelect.Id, cancellationToken);
    
    /// <summary>
    /// Retournes un anime via son id de fiche icotaku.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public new static async Task<Tanime?> SingleBySheetIdAsync(int id, CancellationToken? cancellationToken = null)
        => await SingleAsync(id, IntColumnSelect.SheetId, cancellationToken);
    
    /// <summary>
    /// Retournes un anime
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public new static async Task<Tanime?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        IntColumnSelectException.ThrowNotSupportedException(columnSelect, nameof(columnSelect),
            [IntColumnSelect.Id, IntColumnSelect.SheetId]);
        
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Tanime.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tanime.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

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
    /// <returns></returns>
    public new static async Task<Tanime?> SingleAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Tanime.Name = $Name COLLATE NOCASE";

        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", null, null);
        command.Parameters.AddWithValue("$Name", name);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public static new async Task<Tanime?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Tanime.Url = $Url COLLATE NOCASE";

        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", null, null);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        var insertBaseResult = await InsertAsync(disableVerification, cancellationToken);
        if (!insertBaseResult.IsSuccess || insertBaseResult.Data <= 0)
            return insertBaseResult;
        
        await this.AddOrReplaceEpisodesAsync(cancellationToken);
        return new OperationState<int>(true, "L'anime a été ajouté avec succès", insertBaseResult.Data);
    }
    
    #endregion

    #region Update

    public new async Task<OperationState> UpdateAsync(bool disableExistenceVerification = false, CancellationToken? cancellationToken = null)
    {
        var updateBaseResult = await base.UpdateAsync(disableExistenceVerification, cancellationToken);
        if (!updateBaseResult.IsSuccess)
            return updateBaseResult;
        
        await this.UpdateAlternativeTitlesAsync(cancellationToken);
        await this.UpdateWebsitesAsync(cancellationToken);
        await this.UpdateStudiosAsync(cancellationToken);
        await this.UpdateCategoriesAsync(cancellationToken);
        await this.UpdateEpisodesAsync(cancellationToken);
        await this.UpdateLicensesAsync(cancellationToken);
        await this.UpdateStaffsAsync(cancellationToken);
            
        return new OperationState(true, "L'anime a été mis à jour avec succès") ;
    }

    #endregion

    #region Add or Update or Single

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);
    public static async Task<OperationState<int>> AddOrUpdateAsync(Tanime value,
        CancellationToken? cancellationToken = null)
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
        var existingId = await GetIdOfAsync(uri, value.SheetId, cancellationToken);
        
        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState<int>(false, "Le nom de l'item existe déjà");
            
            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;
            return (await value.UpdateAsync(true, cancellationToken)).ToGenericState(value.Id);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAync(true, cancellationToken);
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
                    ReleaseMonth = MonthDate.FromNumberedDate((uint)reader.GetInt64(reader.GetOrdinal("ReleaseMonth"))),
                    IsFullyLoaded = reader.GetBoolean(reader.GetOrdinal("AnimeIsFullyLoaded")),
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

                var episodes = await Tepisode.SelectAsync(animeId, IcotakuSection.Anime, AnimeEpisodeSortBy.EpisodeNumber, OrderBy.Asc);
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
                        descriptionIndex: reader.GetOrdinal("CategoryDescription"),
                        isFullyScrapedIndex: reader.GetOrdinal("CategoryIsFullyScraped"));
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
            
            if (!reader.IsDBNull(reader.GetOrdinal("IdStatistic")))
            {
                var statisticId = reader.GetInt32(reader.GetOrdinal("IdStatistic"));
                var statistic = await TsheetStatistic.SingleAsync(statisticId, cancellationToken);
                if (statistic != null)
                    anime.Statistic = statistic;
            }
        }
        
        return [.. animeList];
    }

    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Tanime.Id AS AnimeId,
            Tanime.Guid AS AnimeGuid,
            Tanime.IdStatistic,
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
            Tanime.ReleaseMonth,
            Tanime.ReleaseDate,
            Tanime.EndDate,
            Tanime.DiffusionState,
            Tanime.Description AS AnimeDescription,
            Tanime.ThumbnailUrl,
            Tanime.Remark AS AnimeRemark,
            Tanime.IsFullyLoaded AS AnimeIsFullyLoaded,
            
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
            Tcategory.Description AS CategoryDescription,
            Tcategory.IsFullyScraped AS CategoryIsFullyScraped
        
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