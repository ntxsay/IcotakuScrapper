using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    #region Preparation scrapping

    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(string htmlContent, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(htmlContent);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken);
    }

    /// <summary>
    /// Scrap la fiche anime à partir de son url
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken);
    }

    /// <summary>
    /// Récupère les informations de la fiche anime à partir de son url
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(HtmlNode documentNode, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null)
    {
        try
        {
            var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
            
            var episodesTask = options.HasFlag(AnimeScrapingOptions.Episodes) ? Tepisode.ScrapEpisode(sheetId).ToArrayAsync() : ValueTask.FromResult<Tepisode[]>([]);
            
            var animeBaseResult = await ScrapAnimeBaseAsync(documentNode, sheetUri, options, cancellationToken);
            if (!animeBaseResult.IsSuccess || animeBaseResult.Data == null)
                return new OperationState<Tanime?>(false, animeBaseResult.Message);

            var anime = animeBaseResult.Data.ToFullAnime();

            //Episodes
            if (options.HasFlag(AnimeScrapingOptions.Episodes))
            {
                while (!episodesTask.IsCompleted)
                    await Task.Delay(100);

                var episodes = episodesTask.Result;
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