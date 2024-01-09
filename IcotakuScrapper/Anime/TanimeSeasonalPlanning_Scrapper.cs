using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Web;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Anime;

public partial class TanimeSeasonalPlanning
{
    private static readonly HashSet<AnimeAdditionalInfosStruct> AdditionalContentList = [];

    /// <summary>
    /// Scrappe le planning saisonnier d'animé
    /// </summary>
    /// <param name="season">Saison à scrapper</param>
    /// <param name="insertMode"></param>
    /// <param name="isDeleteSectionRecords"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> ScrapAsync(WeatherSeason season,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
    {
        var seasonalPlannings = await ScrapAnimeSeasonalPlanning(season, cancellationToken).ToArrayAsync();
        if (seasonalPlannings.Length == 0)
            return new OperationState(false, "Le planning est vide");

        if (isDeleteSectionRecords)
        {
            var deleteAllResult = await DeleteAllAsync(season, cancellationToken);
            if (!deleteAllResult.IsSuccess)
                return deleteAllResult;
        }

        return await InsertAsync(seasonalPlannings, insertMode, cancellationToken);
    }

    public static async Task<OperationState> ScrapAndAddOrUpdateAsync(WeatherSeason season, CancellationToken? cancellationToken = null)
    {
        var seasonalPlannings = await ScrapAnimeSeasonalPlanning(season, cancellationToken).ToArrayAsync();
        if (seasonalPlannings.Length == 0)
            return new OperationState(false, "Le planning est vide");

        List<OperationState> results = [];
        foreach (var seasonalPlanning in seasonalPlannings)
        {
            results.Add((await seasonalPlanning.AddOrUpdateAsync()).ToBaseState());
        }
        
        if (results.All(a => a.IsSuccess))
            return new OperationState(true, "Tous les animés ont été ajoutés ou mis à jour");
        return results.All(a => !a.IsSuccess) 
            ? new OperationState(false, "Aucun animé n'a été ajouté ou mis à jour") 
            : new OperationState(true, "Certains animés ont été ajoutés ou mis à jour");
    }

    internal static async IAsyncEnumerable<TanimeSeasonalPlanning> ScrapAnimeSeasonalPlanning(WeatherSeason season, CancellationToken? cancellationToken = null)
    {
        AdditionalContentList.Clear();

        var url = IcotakuWebHelpers.GetAnimeSeasonalPlanningUrl(season);
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            yield break;

        HtmlWeb web = new();
        var htmlDocument = web.Load(uri.ToString());
        var htmlNodes = htmlDocument.DocumentNode.SelectNodes("//div[contains(@class, 'planning_saison')]/div[contains(@class, 'categorie')]")?.ToArray();

        if (htmlNodes == null || htmlNodes.Length == 0)
            yield break;

        var seasonRecord = await GetSeasonAsync(season, cancellationToken);
        if (seasonRecord is null)
            yield break;

        List<Task<TanimeSeasonalPlanning?>> tasks = [];
        foreach (var categoryNode in htmlNodes)
        {
            //obtient le nom de la catégorie
            var categoryName = HttpUtility.HtmlDecode(categoryNode.SelectSingleNode("./h2[1]")?.InnerText?.Trim())?.Trim();
            if (categoryName == null || categoryName.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Si le nom de la catégorie est "Hentai" et que l'accès au contenu adulte est désactivé, on passe à la catégorie suivante
            if (!Main.IsAccessingToAdultContent && categoryName.Equals("Hentai", StringComparison.OrdinalIgnoreCase))
                continue;

            var tableNodes = categoryNode.SelectNodes(".//table")?.ToArray();
            if (tableNodes == null || tableNodes.Length == 0)
                yield break;

            foreach (var tableNode in tableNodes)
            {
                tasks.Add(ScrapItemAndGetAdditionalInfosAsync(tableNode, categoryName, cancellationToken));
            }
        }

        var results = await Task.WhenAll(tasks);
        foreach (var result in results.Where(w => w != null))
        {
            if (result is null)
                continue;

            result.Season = seasonRecord;
            yield return result;
        }

        tasks.ForEach(f => f.Dispose());
        tasks.Clear();
    }

    /// <summary>
    /// Scrappe l'anime sur le planning ainsi que ses informations supplémentaires
    /// </summary>
    /// <param name="tableItemNode">Correspond au noeud de type Table qui contient des informations pour un animé</param>
    /// <param name="categoryName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<TanimeSeasonalPlanning?> ScrapItemAndGetAdditionalInfosAsync(HtmlNode tableItemNode, string categoryName, CancellationToken? cancellationToken = null)
    {
        //Si le noeud est null, on retourne null
        if (tableItemNode is null)
            return null;

        //obtient le noeud a qui contient le titre et lien de la fiche de l'animé 
        var aNode = tableItemNode.SelectSingleNode(".//th[contains(@class, 'titre')]/a");
        if (aNode == null)
            return null;

        //obtient le titre de l'animé
        var title = HttpUtility.HtmlDecode(aNode.InnerText?.Trim())?.Trim();
        if (title == null || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        //obtient l'url de la fiche de l'animé
        var animeUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(aNode, IcotakuSection.Anime);
        if (animeUri is null)
            return null;

        //obtient l'id de la fiche de l'animé
        var animeSheetId = IcotakuWebHelpers.GetSheetId(animeUri);
        if (animeSheetId < 0)
            return null;

        //Charge la fiche de l'animé
        HtmlWeb web = new();
        Task<HtmlDocument> htmlDocumentTask = web.LoadFromWebAsync(animeUri.ToString());

        TanimeSeasonalPlanning record = new()
        {
            AnimeName = title,
            SheetId = animeSheetId,
            Url = animeUri.ToString(),
            GroupName = categoryName,
        };

        //obtient le synopsis partiel de l'animé
        var descriptionNode = tableItemNode.SelectSingleNode(".//td[contains(@class, 'histoire')]/text()[1]");
        if (descriptionNode != null)
        {
            record.Description = HttpUtility.HtmlDecode(descriptionNode.InnerText?.Trim())?.Trim();
        }

        //obtient le mois et/ou l'année de sortie de l'animé
        var dateNode = tableItemNode.SelectSingleNode(".//td/span[contains(@class, 'date')]/text()");
        if (dateNode != null)
        {
            var date = HttpUtility.HtmlDecode(dateNode.InnerText?.Trim())?.Trim();
            record.ReleaseMonth = DateHelpers.GetNumberedMonthAndYear(date);
        }

        //obtient l'origine de l'animé
        var origineNode = tableItemNode.SelectSingleNode(".//span[contains(@class, 'origine')]/text()");
        if (origineNode != null)
        {
            var origine = HttpUtility.HtmlDecode(origineNode.InnerText?.Trim())?.Trim();
            record.OrigineAdaptation = GetOrigineAdaptationAsync(origine).Result;
        }

        //obtient les distributeurs de l'animé
        var distributorsNode = tableItemNode.SelectSingleNode(".//span[contains(@class, 'editeur')]/text()");
        if (distributorsNode != null)
        {
            var distributorsName = await GetContact(distributorsNode, ContactType.Distributor, cancellationToken).ToArrayAsync();
            if (distributorsName.Length > 0)
                foreach (var distributorName in distributorsName)
                    record.Distributors.Add(distributorName);
        }

        //obtient les studios de l'animé
        var studiosNode = tableItemNode.SelectSingleNode(".//span[contains(@class, 'studio')]/text()");
        if (studiosNode != null)
        {
            var studiosName = await GetContact(studiosNode, ContactType.Studio, cancellationToken).ToArrayAsync();
            if (studiosName.Length > 0)
                foreach (var studioName in studiosName)
                    record.Studios.Add(studioName);
        }

        //Si fiche de l'animé chargée, on scrappe les informations supplémentaires sinon on attend
        while (!htmlDocumentTask.IsCompleted)
            await Task.Delay(100);

        //Préparation du scrapping des informations supplémentaires
        var htmlDocument = htmlDocumentTask.Result.DocumentNode;
        htmlDocumentTask.Dispose();

        record.IsAdultContent = TanimeBase.ScrapIsAdultContent(ref htmlDocument);
        record.IsExplicitContent = record.IsAdultContent || TanimeBase.ScrapIsExplicitContent(ref htmlDocument);
        record.ThumbnailUrl = TanimeBase.ScrapFullThumbnail(ref htmlDocument);

        return record;
    }

    private static async Task<Tseason?> GetSeasonAsync(WeatherSeason season, CancellationToken? cancellationToken = null)
    {
        var intSeason = season.ToIntSeason();
        if (intSeason == 0)
            return null;

        var seasonLiteral = DateHelpers.GetSeasonLiteral(season);
        if (seasonLiteral == null || seasonLiteral.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var seasonRecord = new Tseason()
        {
            SeasonNumber = intSeason,
            DisplayName = seasonLiteral,
        };

        return await Tseason.SingleOrCreateAsync(seasonRecord, false, cancellationToken);
    }

    private static async Task<TorigineAdaptation?> GetOrigineAdaptationAsync(string? value,
        CancellationToken? cancellationToken = null)
    {
        if (value == null || value.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        TorigineAdaptation? record = new()
        {
            Name = value.Trim(),
            Section = IcotakuSection.Anime,
        };

        return await TorigineAdaptation.SingleOrCreateAsync(record, true, cancellationToken);
    }

    private static async IAsyncEnumerable<string> GetContact(HtmlNode htmlNode, ContactType contactType,
        CancellationToken? cancellationToken = null)
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

                var isExist = await TcontactBase.ExistsAsync(decodedItem, cancellationToken);
                if (!isExist)
                {
                    var contact = new Tcontact()
                    {
                        DisplayName = decodedItem,
                        Type = contactType,
                    };

                    var resultInsert = await contact.InsertAync(false, cancellationToken);
                    if (!resultInsert.IsSuccess)
                        continue;
                }

                yield return decodedItem;
            }
        }
    }
}