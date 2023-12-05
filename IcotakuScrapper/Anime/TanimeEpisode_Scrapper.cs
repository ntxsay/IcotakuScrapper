using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;


namespace IcotakuScrapper.Anime;

public partial class TanimeEpisode
{
    private static string GetAnimeEpisodesUrl(int sheetId)
        => $"https://anime.icotaku.com/anime/episodes/{sheetId}.html";

    internal static IEnumerable<TanimeEpisode> GetAnimeEpisode(int sheetId)
    {
        var url = GetAnimeEpisodesUrl(sheetId);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            yield break;

        HtmlWeb web = new();
        var htmlDocument = web.Load(uri.ToString());
        var htmlNodes = htmlDocument.DocumentNode
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
            var ReleaseDateMatch = releaseDateRegex.Match(diffusedAt);
            if (!ReleaseDateMatch.Success)
                continue;

            var releaseDate = DateHelpers.GetNullableDateOnly(ReleaseDateMatch.Value, "dd/MM/yyyy");
            if (releaseDate is null)
                continue;

            yield return new TanimeEpisode()
            {
                EpisodeNumber = episodeNumber,
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