using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    #region Preparation scrapping

    /// <summary>
    /// Récupère l'anime depuis l'url de la fiche icotaku.
    /// </summary>
    /// <param name="animeResult">Résultat du scraping</param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async Task<OperationState<int>> ScrapAndAnimeFromUrlAsync(OperationState<Tanime?> animeResult, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        //Si le scraping a échoué alors on sort de la méthode en retournant le message d'erreur
        if (!animeResult.IsSuccess)
            return new OperationState<int>(false, animeResult.Message);

        var anime = animeResult.Data;
        if (anime == null)
            return new OperationState<int>(false, "Une erreur est survenue lors de la récupération de l'anime");

        if (anime.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime est introuvable.");

        _ = await CreateIndexAsync(anime.Name, anime.Url, anime.SheetId, cancellationToken, cmd);

        return await anime.AddOrUpdateAsync(cancellationToken, cmd);
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
            var animeBaseResult = await ScrapAnimeBaseAsync(documentNode, sheetUri, options, cancellationToken, cmd);
            if (!animeBaseResult.IsSuccess || animeBaseResult.Data == null)
                return new OperationState<Tanime?>(false, animeBaseResult.Message);

            var anime = new Tanime(animeBaseResult.Data);

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