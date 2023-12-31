﻿using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace IcotakuScrapper.Anime;

public partial class TanimeDailyPlanning
{
    public static async Task<OperationState> ScrapAsync(DateOnly date, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
    {
        var plannings = GetAnimePlanning(date, cancellationToken);
        if (plannings is null || plannings.Length == 0)
            return new OperationState(false, "Aucun anime n'a été trouvé");

        if (isDeleteSectionRecords)
        {
            var deleteState = await DeleteAllAsync(date, cancellationToken);
            if (!deleteState.IsSuccess)
                return deleteState;
        }

        return await InsertAsync(plannings, insertMode, cancellationToken);
    }

    public static async Task<OperationState> ScrapAsync(DateOnly minDate, DateOnly maxDate, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
               bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
    {
        var plannings = GetAnimePlanning(minDate, maxDate, cancellationToken);
        if (plannings is null || plannings.Length == 0)
            return new OperationState(false, "Aucun anime n'a été trouvé");

        if (isDeleteSectionRecords)
        {
            var deleteState = await DeleteAllAsync(minDate, maxDate, cancellationToken);
            if (!deleteState.IsSuccess)
                return deleteState;
        }

        return await InsertAsync(plannings, insertMode, cancellationToken);
    }

    private static string GetAnimeMonthPlanningUrl(DateOnly date)
        => $"https://anime.icotaku.com/planning/calendrierDiffusion/date_debut/{date:yyyy-MM-dd}";

    internal static TanimeDailyPlanning[] GetAnimePlanningAsync(HashSet<DateOnly> dates, CancellationToken? cancellationToken = null)
    {
        if (dates is null || dates.Count == 0)
            return [];

        var plannings = GetAnimeDaysPlanning(dates, cancellationToken).Where(w => dates.Contains(w.ReleaseDate)).ToArray();
        return plannings;
    }

    private static IEnumerable<TanimeDailyPlanning> GetAnimeDaysPlanning(HashSet<DateOnly> dates, CancellationToken? cancellationToken = null)
    {
        if (dates is null || dates.Count == 0)
            yield break;

        HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList = [];
        foreach (var date in dates)
        {
            foreach (var animePlanning in ScrapPlanningFromIcotaku(date, date, additionalContentList, cancellationToken))
                yield return animePlanning;
        }
    }

    internal static TanimeDailyPlanning[] GetAnimePlanning(DateOnly minDate, DateOnly maxDate, CancellationToken? cancellationToken = null)
    {
        var plannings = GetAnimeMonthRangePlanning(minDate, maxDate, cancellationToken).ToArray();
        return plannings;
    }

    internal static TanimeDailyPlanning[] GetAnimePlanning(DateOnly date, CancellationToken? cancellationToken = null)
    {
        var plannings = GetAnimeMonthRangePlanning(date, date, cancellationToken).ToArray();
        return plannings;
    }

    internal static IEnumerable<TanimeDailyPlanning> GetAnimeMonthRangePlanning(DateOnly minDate, DateOnly maxDate, CancellationToken? cancellationToken = null)
    {
        HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList = [];

        int value = DateTime.Compare(minDate.ToDateTime(default), maxDate.ToDateTime(default));
        if (value == 0) //minDate == maxDate
        {
            foreach (var animePlanning in ScrapPlanningFromIcotaku(minDate, maxDate, additionalContentList, cancellationToken))
                yield return animePlanning;
        }
        else if (value < 0)
        {
            var dateCourante = minDate;
            while (dateCourante <= maxDate)
            {
                foreach (var animePlanning in ScrapPlanningFromIcotaku(dateCourante, maxDate, additionalContentList, cancellationToken))
                    yield return animePlanning;

                dateCourante = new DateOnly(dateCourante.Year, dateCourante.Month, 1).AddMonths(1);
            }
        }
    }

    private static IEnumerable<TanimeDailyPlanning> ScrapPlanningFromIcotaku(DateOnly minDate, DateOnly maxDate, HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList, CancellationToken? cancellationToken = null)
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
            else if (releaseDate < minDate)
                continue;

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

                var animeSheetUri = IcotakuWebHelpers.GetFullHrefFromRelativePath(animeSheetUrl, IcotakuSection.Anime);
                if (animeSheetUri is null)
                    continue;

                var animeSheetId = IcotakuWebHelpers.GetSheetId(animeSheetUri);
                if (animeSheetId == 0)
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

                var record = new TanimeDailyPlanning()
                {
                    SheetId = animeSheetId,
                    AnimeName = animeName,
                    Url = animeSheetUri.ToString(),
                    EpisodeNumber = episodeNumber,
                    EpisodeName = $"Episode {episodeNumber}",
                    ReleaseDate = releaseDate,
                    Day = releaseDate.DayOfWeek,
                };

                AddAdditionalInfos(record, animeSheetUri, ref additionalContentList);

                yield return record;
            }
        }

    }

    private static void AddAdditionalInfos(TanimeDailyPlanning planning, Uri animeSheetUri, ref HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList)
    {
        var additionalContent = additionalContentList.FirstOrDefault(w => w.sheetId == planning.SheetId);
        if (!additionalContent.Equals(default))
        {
            planning.IsAdultContent = additionalContent.isAdultContent;
            planning.IsExplicitContent = additionalContent.isExplicitContent;
            planning.ThumbnailUrl = additionalContent.thumbnailUrl;
            return;
        }

        HtmlWeb web = new();
        var htmlDocument = web.Load(animeSheetUri.ToString()).DocumentNode;

        planning.IsAdultContent = TanimeBase.ScrapIsAdultContent(ref htmlDocument);
        planning.IsExplicitContent = planning.IsAdultContent || TanimeBase.ScrapIsExplicitContent(ref htmlDocument);
        planning.ThumbnailUrl = TanimeBase.ScrapFullThumbnail(ref htmlDocument);

        additionalContentList.Add((planning.SheetId, planning.IsAdultContent, planning.IsExplicitContent, planning.ThumbnailUrl));
    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex GetEpisodeNumberRegex();

}