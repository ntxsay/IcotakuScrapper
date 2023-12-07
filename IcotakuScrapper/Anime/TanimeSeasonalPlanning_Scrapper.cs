using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace IcotakuScrapper.Anime;

public partial class TanimeSeasonalPlanning
{
    private static string? GetAnimeSeasonalPlanningUrl(WeatherSeason season)
    {
        var seasonName = season.Season switch
        {
            WeatherSeasonKind.Spring => "printemps",
            WeatherSeasonKind.Summer => "ete",
            WeatherSeasonKind.Fall => "automne",
            WeatherSeasonKind.Winter => "hiver",
            _ => null,
        };

        if (seasonName is null)
            return null;

        return $"https://anime.icotaku.com/planning/planningSaisonnier/saison/{seasonName}/annee/{season.Year}";
    }

    public static async Task<OperationState> ScrapAsync(WeatherSeason season,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var planning = await GetAnimeSeasonalPlanning(season, cancellationToken, cmd).ToArrayAsync();
        if (planning.Length == 0)
            return new OperationState(false, "Le planning est vide");

        if (isDeleteSectionRecords)
        {
            var deleteAllResult = await DeleteAllAsync(season, cancellationToken, cmd);
            if (!deleteAllResult.IsSuccess)
                return deleteAllResult;
        }

        return await InsertAsync(planning,insertMode, cancellationToken, cmd);
    }


    internal static async IAsyncEnumerable<TanimeSeasonalPlanning> GetAnimeSeasonalPlanning(WeatherSeason season, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var url = GetAnimeSeasonalPlanningUrl(season);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            yield break;

        HtmlWeb web = new();
        var htmlDocument = web.Load(uri.ToString());
        var htmlNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'planning_saison')]/div[contains(@class, 'categorie')]").ToArray();

        if (htmlNodes == null || htmlNodes.Length == 0)
            yield break;

        var seasonRecord = await GetSeasonAsync(season, cancellationToken, cmd);
        if (seasonRecord is null)
            yield break;

        HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList = [];

        foreach (var categoryNode in htmlNodes)
        {
            var categoryName = HttpUtility.HtmlDecode(categoryNode.SelectSingleNode("./h2[1]")?.InnerText?.Trim())?.Trim();
            if (categoryName == null || categoryName.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var tableNodes = categoryNode.SelectNodes(".//table").ToArray();
            if (tableNodes == null || tableNodes.Length == 0)
                yield break;

            foreach (var tableNode in tableNodes)
            {
                var aNode = tableNode.SelectSingleNode(".//th[contains(@class, 'titre')]/a");
                if (aNode == null)
                    continue;

                var title = HttpUtility.HtmlDecode(aNode.InnerText?.Trim())?.Trim();
                if (title == null || title.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                if (aNode.Attributes["href"]?.Value == null || aNode.Attributes["href"].Value.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                var animeUri = IcotakuWebHelpers.GetFullHrefFromRelativePath(aNode.Attributes["href"].Value, IcotakuSection.Anime);
                if (animeUri is null)
                    continue;

                var animeSheetId = IcotakuWebHelpers.GetSheetId(animeUri);
                if (animeSheetId < 0)
                    continue;

                TanimeSeasonalPlanning record = new()
                {
                    AnimeName = title,
                    GroupName = categoryName,
                    SheetId = animeSheetId,
                    Url = animeUri.ToString(),
                    Season = seasonRecord,
                };

                AddAdditionalInfos(record, animeUri, ref additionalContentList);

                var descriptionNode = tableNode.SelectSingleNode(".//td[contains(@class, 'histoire')]/text()[1]");
                if (descriptionNode != null)
                {
                    record.Description = HttpUtility.HtmlDecode(descriptionNode.InnerText?.Trim())?.Trim();
                }

                var dateNode = tableNode.SelectSingleNode(".//td/span[contains(@class, 'date')]/text()");
                if (dateNode != null)
                {
                    var date = HttpUtility.HtmlDecode(dateNode.InnerText?.Trim())?.Trim();
                    record.ReleaseMonth = GetBeginDate(date);
                }

                var origineNode = tableNode.SelectSingleNode(".//span[contains(@class, 'origine')]/text()");
                if (origineNode != null)
                {
                    var origine = HttpUtility.HtmlDecode(origineNode.InnerText?.Trim())?.Trim();
                    record.OrigineAdaptation = await GetOrigineAdaptationAsync(origine);
                }

                var distributorsNode = tableNode.SelectSingleNode(".//span[contains(@class, 'editeur')]/text()");
                if (distributorsNode != null)
                {
                    var distributorsName = await GetContact(distributorsNode, ContactType.Distributor, cancellationToken, cmd).ToArrayAsync();
                    if (distributorsName.Length > 0)
                        foreach (var distributorName in distributorsName)
                            record.Distributors.Add(distributorName);
                }

                var studiosNode = tableNode.SelectSingleNode(".//span[contains(@class, 'studio')]/text()");
                if (studiosNode != null)
                {
                    var studiosName = await GetContact(studiosNode, ContactType.Studio, cancellationToken, cmd).ToArrayAsync();
                    if (studiosName.Length > 0)
                        foreach (var studioName in studiosName)
                            record.Studios.Add(studioName);
                }

                yield return record;
            }
        }
    }

    private static void AddAdditionalInfos(TanimeSeasonalPlanning planning, Uri animeSheetUri, ref HashSet<(int sheetId, bool isAdultContent, bool isExplicitContent, string? thumbnailUrl)> additionalContentList)
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
        var htmlDocument = web.Load(animeSheetUri.ToString());

        planning.IsAdultContent = Tanime.ScrapIsAdultContent(htmlDocument.DocumentNode);
        planning.IsExplicitContent = planning.IsAdultContent || Tanime.ScrapIsExplicitContent(htmlDocument.DocumentNode);
        planning.ThumbnailUrl = Tanime.SCrapFullThumbnail(htmlDocument.DocumentNode);

        additionalContentList.Add((planning.SheetId, planning.IsAdultContent, planning.IsExplicitContent, planning.ThumbnailUrl));
    }

    private static async Task<Tseason?> GetSeasonAsync(WeatherSeason season, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var intSeason = season.ToIntSeason();
        if (intSeason == 0)
            return null;

        var seasonRecord = await Tseason.SingleAsync(intSeason, cancellationToken, cmd);
        if (seasonRecord is null)
        {
            var seasonliteral = DateHelpers.GetSeasonLiteral(season);
            if (seasonliteral == null || seasonliteral.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            seasonRecord = new Tseason()
            {
                SeasonNumber = intSeason,
                DisplayName = seasonliteral,
            };

            var resultInsert = await seasonRecord.InsertAsync(cancellationToken, cmd);
            if (!resultInsert.IsSuccess)
                return null;
        }

        return seasonRecord;
    }

    private static async Task<TorigineAdaptation?> GetOrigineAdaptationAsync(string? value,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (value == null || value.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        TorigineAdaptation? record = new()
        {
            Name = value.Trim(),
            Section = IcotakuSection.Anime,
        };

        return await TorigineAdaptation.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }

    private static uint GetBeginDate(string? date)
    {
        if (date == null || date.IsStringNullOrEmptyOrWhiteSpace())
            return 0;
        var _date = date.Trim();

        var split = _date.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (split.Length == 1)
        {
            if (!ushort.TryParse(split[0], out ushort year))
                return uint.Parse($"{year}00");
        }
        else if (split.Length == 2)
        {
            var monthNumber = DateHelpers.GetMonthNumber(split[0]);

            if (!ushort.TryParse(split[1], out ushort year))
                return 0;

            return uint.Parse($"{year}{monthNumber:00}");
        }

        return 0;
    }

    private static async IAsyncEnumerable<string> GetContact(HtmlNode htmlNode, ContactType contactType,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (htmlNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            yield break;

        var splitContact = htmlNode.InnerText.Split("&nbsp;", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (splitContact.Length > 0)
        {
            foreach (var item in splitContact)
            {
                var decodedItem = HttpUtility.HtmlDecode(item?.Trim())?.Trim();
                if (decodedItem == null || decodedItem.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                var isExist = await TcontactBase.ExistsAsync(decodedItem, cancellationToken, cmd);
                if (!isExist)
                {
                    var contact = new Tcontact()
                    {
                        DisplayName = decodedItem,
                        Type = contactType,
                    };

                    var resultInsert = await contact.InsertAync(cancellationToken, cmd);
                    if (!resultInsert.IsSuccess)
                        continue;
                }

                yield return decodedItem;
            }
        }
    }

    [GeneratedRegex("(\\d+)")]
    private static partial Regex GetEpisodeNumberRegex();

    [GeneratedRegex(@"\b\d{2}/\d{2}/\d{4}\b")]
    private static partial Regex GetReleaseDateRegex();
}