using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    /// <summary>
    /// Récupère les informations de l'anime via l'id Icotaku de la fiche
    /// </summary>
    /// <param name="sheetId">Id Icotaku de la fiche</param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromSheetIdAsync(int sheetId, AnimeScrapingOptions options = AnimeScrapingOptions.None,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var index = await TsheetIndex.SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken, cmd);
        if (index == null)
            return new OperationState<int>(false, "L'index permettant de récupérer l'url de la fiche de l'anime n'a pas été trouvé dans la base de données.");

        if (!Uri.TryCreate(index.Url, UriKind.Absolute, out var sheetUri) || !sheetUri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime est invalide.");

        return await ScrapFromUrlAsync(sheetUri, options, cancellationToken, cmd);
    }

    /// <summary>
    /// Récupère les informations de l'anime via l'url de la fiche
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="userName"></param>
    /// <param name="passWord"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, string userName, string passWord, AnimeScrapingOptions options = AnimeScrapingOptions.None,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlContent = await IcotakuWebHelpers.GetRestrictedHtmlAsync(IcotakuSection.Anime, sheetUri, userName, passWord);
        if (htmlContent == null || htmlContent.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le contenu de la fiche est introuvable.");

        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var animeResult = await ScrapAnimeAsync(htmlContent, sheetUri, options, cancellationToken, command);

        return await ScrapAnimeFromUrlAsync(animeResult, cancellationToken, command);
    }

    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, AnimeScrapingOptions options = AnimeScrapingOptions.None, 
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var animeResult = await ScrapAnimeAsync(sheetUri, options, cancellationToken, command);

        return await ScrapAnimeFromUrlAsync(animeResult, cancellationToken, command);
    }

    #region Prepration scrapping

    /// <summary>
    /// Récupère l'anime depuis l'url de la fiche icotaku.
    /// </summary>
    /// <param name="animeResult"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async Task<OperationState<int>> ScrapAnimeFromUrlAsync(OperationState<Tanime?> animeResult, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!animeResult.IsSuccess)
            return new OperationState<int>(false, animeResult.Message);

        var anime = animeResult.Data;
        if (anime == null)
            return new OperationState<int>(false, "Une erreur est survenue lors de la récupération de l'anime");

        if (anime.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime est introuvable.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        _ = await CreateIndexAsync(anime.Name, anime.Url, anime.SheetId, cancellationToken, command);

        return await anime.AddOrUpdateAsync(cancellationToken, command);
    }

    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(string htmlContent, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(htmlContent);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken, cmd);
    }

    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken, cmd);
    }

    /// <summary>
    /// Récupère les informations de la fiche anime à partir de son url
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(HtmlNode documentNode, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        try
        {
            var animeBaseResult = await ScrapAnimeBaseAsync(documentNode, sheetUri, cancellationToken, cmd);
            if (!animeBaseResult.IsSuccess || animeBaseResult.Data == null)
                return new OperationState<Tanime?>(false, animeBaseResult.Message);
            
            var anime = new Tanime(animeBaseResult.Data);
            
            
            //Licenses
            var licenses = await ScrapLicensesAsync(documentNode, cancellationToken, cmd).ToArrayAsync();
            if (licenses.Length > 0)
            {
                foreach (var license in licenses)
                {
                    if (!anime.Licenses.Any(a => string.Equals(a.Distributor.Url, license.Distributor.Url, StringComparison.OrdinalIgnoreCase)))
                        anime.Licenses.Add(license);

                }
            }

            //Episodes
            if (options.HasFlag(AnimeScrapingOptions.Episodes))
            {
                var episodes = TanimeEpisode.GetAnimeEpisode(anime.SheetId).ToArray();
                if (episodes.Length > 0)
                {
                    foreach (var episode in episodes)
                        anime.Episodes.Add(episode);

                    anime.ReleaseDate = episodes.Min(m => m.ReleaseDate).ToString("yyyy-MM-dd");
                    if (anime.EpisodesCount == episodes.Length)
                    {
                        var endDate = episodes.Max(m => m.ReleaseDate);
                        anime.EndDate = endDate.ToString("yyyy-MM-dd");
                    }
                }
            }
            
            //Staff
            var staff = await ScrapStaffAsync(documentNode, cancellationToken, cmd).ToArrayAsync();
            if (staff.Length > 0)
            {
                foreach (var tanimeStaff in staff)
                {
                    if (!anime.Staffs.Any(a => a.Role.Id == tanimeStaff.Role.Id && a.Person.Id == tanimeStaff.Person.Id))
                        anime.Staffs.Add(tanimeStaff);
                }
            }

            return new OperationState<Tanime?>
            {
                IsSuccess = true,
                Data = anime,
            };
        }
        catch (Exception ex)
        {
            LogServices.LogDebug(ex);
            return new OperationState<Tanime?>(false, ex.Message);
        }
    }

    #endregion

    
   
}