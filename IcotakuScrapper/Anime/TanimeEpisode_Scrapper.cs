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

    private static string GetAnimeMonthPlanningUrl(DateOnly date)
        => $"https://anime.icotaku.com/planning/calendrierDiffusion/date_debut/{date:yyyy-MM-dd}";

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

            if(!DateTime.TryParseExact(ReleaseDateMatch.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate))
                continue;

            yield return new TanimeEpisode()
            {
                EpisodeNumber = episodeNumber,
                EpisodeName = episodeName,
                ReleaseDate = releaseDate,
                Day = releaseDate.DayOfWeek,
            };
        }
    }

    internal static IEnumerable<TanimeEpisode> GeTanimeEpisode(DateOnly date)
    {
        var url = GetAnimeMonthPlanningUrl(date);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            yield break;

        HtmlWeb web = new();
        var htmlDocument = web.Load(uri.ToString());
        var htmlNodes = htmlDocument.DocumentNode
            .SelectNodes(
                "//table[contains(@class, 'calendrier_diffusion')]")
            ?.ToArray();
        if (htmlNodes is null || htmlNodes.Length == 0)
            yield break;

        foreach (var htmlNode in htmlNodes)
        {
            var dayNode = htmlNode.SelectSingleNode(".//th/bfollowing-sibling::text()");
            if (dayNode is null)
                continue;

            var numberDayText = dayNode.InnerText.Trim();
            if (numberDayText == null || numberDayText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            if (byte.TryParse(numberDayText, out byte numberDay))
                continue;

            var releaseDate = new DateTime(date.Year, date.Month, numberDay);
            var noDay = releaseDate.DayOfWeek;

            var animeOnDay_TdNodes = htmlNode.SelectNodes(".//tr/td/div[@id='div_info')]/parent::td")?.ToArray();
            if (animeOnDay_TdNodes is null || animeOnDay_TdNodes.Length == 0)
                continue;

            foreach (var animeOnDay_TdNode in animeOnDay_TdNodes)
            {
                var anime_aNode = animeOnDay_TdNode.SelectSingleNode("./span[1]/a");
                if (anime_aNode is null)
                    continue;

                var animeName = HttpUtility.HtmlDecode(anime_aNode.InnerText.Trim());
                if (animeName == null || animeName.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                var animeSheetUrl = anime_aNode.GetAttributeValue("href", null);
                if (animeSheetUrl == null || animeSheetUrl.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                var animeSheetUri = Main.GetFullHrefFromRelativePath(animeSheetUrl, IcotakuSection.Anime);
                if (animeSheetUri is null)
                    continue;

                var animeSheetId = Main.GetSheetId(animeSheetUri);
                if (animeSheetId is null)
                    continue;

                var noEpisodeNode = animeOnDay_TdNode.SelectSingleNode("./span[2]/text()");
                if (noEpisodeNode is null)
                    continue;

                var noEpisodeText = noEpisodeNode.InnerText.Trim();
                if (noEpisodeText == null || noEpisodeText.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                // Création d'une instance Regex
                var episodeNumberRegex = GetEpisodeNumberRegex();

                // Recherche du numéro de l'épisode
                var matchEpisodeNumber = episodeNumberRegex.Match(noEpisodeText);
                if (!matchEpisodeNumber.Success)
                    continue;

                if (!ushort.TryParse(matchEpisodeNumber.Value, out ushort episodeNumber))
                    continue;

                
                yield return new TanimeEpisode()
                {
                    EpisodeNumber = episodeNumber,
                    EpisodeName = $"Episode {episodeNumber}",
                    ReleaseDate = releaseDate,
                    Day = releaseDate.DayOfWeek,
                };

            }
        }

    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex GetEpisodeNumberRegex();

    [GeneratedRegex(@"\b\d{2}/\d{2}/\d{4}\b")]
    private static partial Regex GetReleaseDateRegex();
}