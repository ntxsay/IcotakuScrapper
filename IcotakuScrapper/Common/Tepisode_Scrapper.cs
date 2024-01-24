using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Common;

public partial class Tepisode
{

    internal static async IAsyncEnumerable<Tepisode> ScrapEpisode(int sheetId)
    {
        //Obtient l'url de la page contenant la liste des épisodes
        var url = IcotakuWebHelpers.GetEpisodesUrl(sheetId, IcotakuSection.Anime);
        
        //Si l'url est invalide alors on sort de la méthode
        if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            yield break;

        HtmlWeb web = new();
        var htmlDocument = web.Load(uri.ToString());

        var (minpage, maxpage) =
            IcotakuHelpers.Scrapper.GetMinAndMaxPage(htmlDocument.DocumentNode, IcotakuSection.Anime, false, null);

        //Initialisation de la liste des noeuds de la page des épisodes à scrapper
        List<HtmlNode> episodesDocumentNodesList = [];
        
        //Ajout du noeud de la page actuelle
        episodesDocumentNodesList.Add(htmlDocument.DocumentNode);
        
        for (var i = 1; i < maxpage + 1; i++)
        {
            /*
             * Charge de manière asynchrone la page suivante si elle n'est pas supérieure à la page maximale
             * Si la page est supérieure à la page maximale alors on retourne null
             */
            using var loadEpisodesDocumentNodeTask = i > maxpage 
                ? Task.FromResult<HtmlNode?>(null) 
                : LoadUrlAsync(sheetId, (uint)i + 1);
            
            //Scrappe les épisodes de la page précédente
            foreach (var episode in ScrapEpisodesPage(episodesDocumentNodesList[i - 1]))
                yield return episode;

            //Attends que la tâche de chargement de la page suivante soit terminée
            while (!loadEpisodesDocumentNodeTask.IsCompleted)
                await Task.Delay(100);

            //Ajout du noeud de la page suivante si elle n'est pas null
            var loadEpisodesDocumentNode = loadEpisodesDocumentNodeTask.Result;
            if (loadEpisodesDocumentNode != null)
                episodesDocumentNodesList.Add(loadEpisodesDocumentNode);

            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Charge de manière asynchrone la page contenant la liste des épisodes
    /// </summary>
    /// <param name="sheetId"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    private static async Task<HtmlNode?> LoadUrlAsync(int sheetId, uint page)
    {
        var url = IcotakuWebHelpers.GetEpisodesUrl(sheetId, IcotakuSection.Anime, page);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;

        HtmlWeb web = new();
        var htmlDocument = await web.LoadFromWebAsync(uri.AbsoluteUri);
        return htmlDocument.DocumentNode;
    }

    
    /// <summary>
    /// Retourne la liste des épisodes à partir d'un noeud HTML
    /// </summary>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    private static IEnumerable<Tepisode> ScrapEpisodesPage(HtmlNode documentNode)
    {
        var htmlNodes = documentNode
            .SelectNodes(
                "//div[@id='page']/div[contains(@class, 'contenu')]/div[contains(@class, 'liste_episodes')]/div/h2/parent::div")
            ?.ToArray();
        if (htmlNodes is null || htmlNodes.Length == 0)
            yield break;

        foreach (var htmlNode in htmlNodes)
        {
            var episodeNameRaw = htmlNode.SelectSingleNode("./h2/text()")?.InnerText.Trim();
            var splitEpisodeName = episodeNameRaw?.Split(':',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (splitEpisodeName == null || splitEpisodeName.Length == 0)
                continue;

            string episodeName = "";

            // Création d'une instance Regex
            var episodeNumberRegex = GetEpisodeNumberRegex();

            // Recherche du numéro de l'épisode
            var matchEpisodeNumber = episodeNumberRegex.Match(splitEpisodeName[0]);
            if (!matchEpisodeNumber.Success)
                continue;

            if (!ushort.TryParse(matchEpisodeNumber.Value, out ushort episodeNumber))
                continue;

            if (splitEpisodeName.Length >= 2)
                episodeName = !splitEpisodeName[1].IsStringNullOrEmptyOrWhiteSpace()
                    ? HttpUtility.HtmlDecode(splitEpisodeName[1]).Trim()
                    : $"Episode {episodeNumber}";

            var diffusedAt = htmlNode.SelectSingleNode("./div[contains(@class, 'screenshot')]/br[1]/following-sibling::text()[1]")?.InnerText;
            if (diffusedAt is null || diffusedAt.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            // Création d'une instance Regex
            var releaseDateRegex = GetReleaseDateRegex();

            // Recherche de la date dans la chaîne
            var releaseDateMatch = releaseDateRegex.Match(diffusedAt);
            if (!releaseDateMatch.Success)
                continue;

            var releaseDate = DateHelpers.GetNullableDateOnly(releaseDateMatch.Value, "dd/MM/yyyy");
            if (releaseDate is null)
                continue;

            yield return new Tepisode()
            {
                NoEpisode = episodeNumber,
                EpisodeName = episodeName,
                ReleaseDate = releaseDate.Value,
                Day = releaseDate.Value.DayOfWeek,
            };
        }
    }
    
    [GeneratedRegex("(\\d+)")]
    private static partial Regex GetEpisodeNumberRegex();

    [GeneratedRegex(@"\b\d{2}/\d{2}/\d{4}\b")]
    private static partial Regex GetReleaseDateRegex();
}