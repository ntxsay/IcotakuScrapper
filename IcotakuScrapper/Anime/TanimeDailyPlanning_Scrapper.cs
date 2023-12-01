using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using System.Web;

namespace IcotakuScrapper.Anime;

public partial class TanimeDailyPlanning
{
    private static string GetAnimeMonthPlanningUrl(DateOnly date)
        => $"https://anime.icotaku.com/planning/calendrierDiffusion/date_debut/{date:yyyy-MM-dd}";

    public static async Task<TanimeDailyPlanning[]> GetAnimePlanningAsync(HashSet<DateOnly> dates, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (dates is null || dates.Count == 0)
            return [];

        var plannings = await GetAnimeDaysPlanningAsync(dates, cancellationToken, cmd).Where(w => dates.Contains(w.ReleaseDate)).ToArrayAsync();
        return plannings;
    }

    private static async IAsyncEnumerable<TanimeDailyPlanning> GetAnimeDaysPlanningAsync(HashSet<DateOnly> dates, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (dates is null || dates.Count == 0)
            yield break;

        foreach (var date in dates)
        {
            await foreach (var animePlanning in GetAnimePlanning(date, cancellationToken, cmd))
                yield return animePlanning;
        }
    }

    public static async Task<TanimeDailyPlanning[]> GetAnimePlanningAsync(string minDate, string maxDate, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!DateOnly.TryParse(minDate, out var min) || !DateOnly.TryParse(maxDate, out var max))
            return [];

        var plannings = await GetAnimeMonthRangePlanningAsync(min, max, cancellationToken, cmd).Where(w => w.ReleaseDate >= min && w.ReleaseDate <= max).ToArrayAsync();
        return plannings;
    }

    public static async Task<TanimeDailyPlanning[]> GetAnimePlanningAsync(DateOnly minDate, DateOnly maxDate, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var plannings = await GetAnimeMonthRangePlanningAsync(minDate, maxDate, cancellationToken, cmd).Where(w => w.ReleaseDate >= minDate && w.ReleaseDate <= maxDate).ToArrayAsync();
        return plannings;
    }

    private static async IAsyncEnumerable<TanimeDailyPlanning> GetAnimeMonthRangePlanningAsync(DateOnly minDate, DateOnly maxDate, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        int value = DateTime.Compare(minDate.ToDateTime(default), maxDate.ToDateTime(default));
        if (value == 0) //minDate == maxDate
        {
            await foreach (var animePlanning in GetAnimePlanning(minDate, maxDate, cancellationToken, cmd))
                yield return animePlanning;
        }
        else if (value < 0)
        {
            var dateCourante = minDate;
            while (dateCourante <= maxDate)
            {
                await foreach (var animePlanning in GetAnimePlanning(dateCourante, maxDate, cancellationToken, cmd))
                    yield return animePlanning;

                dateCourante = new DateOnly(dateCourante.Year, dateCourante.Month, 1).AddMonths(1);
            }
        }
    }

    internal static async IAsyncEnumerable<TanimeDailyPlanning> GetAnimePlanning(DateOnly date, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
            var dayNode = htmlNode.SelectSingleNode(".//th/b/following-sibling::text()");
            if (dayNode is null)
                continue;

            var numberDayText = dayNode.InnerText.Trim();
            if (numberDayText == null || numberDayText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            if (byte.TryParse(numberDayText, out byte numberDay))
                continue;

            var releaseDate = new DateOnly(date.Year, date.Month, numberDay);
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


                var idAnime = await Tanime.GetIdOfAsync(animeSheetId.Value, cancellationToken, cmd);

                yield return new TanimeDailyPlanning()
                {
                    SheetId = animeSheetId.Value,
                    AnimeName = animeName,
                    Url = animeSheetUri.ToString(),
                    EpisodeNumber = episodeNumber,
                    EpisodeName = $"Episode {episodeNumber}",
                    ReleaseDate = releaseDate,
                    Day = releaseDate.DayOfWeek,
                    IdAnime = idAnime,
                };

            }
        }
    }

    internal static async IAsyncEnumerable<TanimeDailyPlanning> GetAnimePlanning(DateOnly minDate, DateOnly maxDate, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        int value = DateTime.Compare(minDate.ToDateTime(default), maxDate.ToDateTime(default));
        if (value > 0)
            yield break;
        
        var url = GetAnimeMonthPlanningUrl(minDate);
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
            var dayNode = htmlNode.SelectSingleNode(".//th/b/following-sibling::text()");
            if (dayNode is null)
                continue;

            var numberDayText = dayNode.InnerText.Trim();
            if (numberDayText == null || numberDayText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            if (!byte.TryParse(numberDayText, out byte numberDay))
                continue;

            var releaseDate = new DateOnly(minDate.Year, minDate.Month, numberDay);
            if (releaseDate > maxDate)
                yield break;

            var noDay = releaseDate.DayOfWeek;

            var animeOnDay_TdNodes = htmlNode.SelectNodes(".//tr/td/div[@id='div_info']/parent::td")?.ToArray();
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


                var idAnime = await Tanime.GetIdOfAsync(animeSheetId.Value, cancellationToken, cmd);

                yield return new TanimeDailyPlanning()
                {
                    SheetId = animeSheetId.Value,
                    AnimeName = animeName,
                    Url = animeSheetUri.ToString(),
                    EpisodeNumber = episodeNumber,
                    EpisodeName = $"Episode {episodeNumber}",
                    ReleaseDate = releaseDate,
                    Day = releaseDate.DayOfWeek,
                    IdAnime = idAnime,
                };

            }
        }

    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex GetEpisodeNumberRegex();

    [GeneratedRegex(@"\b\d{2}/\d{2}/\d{4}\b")]
    private static partial Regex GetReleaseDateRegex();
}