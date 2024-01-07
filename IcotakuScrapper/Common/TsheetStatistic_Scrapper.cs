using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using System.Text.RegularExpressions;
using System.Web;

namespace IcotakuScrapper.Common;

/// <summary>
/// Classe représentant les statistiques d'une fiche
/// </summary>
public partial class TsheetStatistic
{
    /// <summary>
    /// Pattern pour rechercher une date au format dd/MM/yyyy à HH:mm
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\b\d{2}/\d{2}/\d{4} à \d{2}:\d{2}\b")]
    private static partial Regex GetFrenchFullDate();

    /// <summary>
    /// Pattern pour rechercher l'âge au format "xx ans"
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\b(\d{1,2}(\.\d)?)\s+(an|ans)?\b")]
    private static partial Regex GetFrenchAge();

    /// <summary>
    /// Scrap les statistiques d'une fiche et les enregistre dans la base de données. Retourne l'identifiant de la fiche si l'opération s'est bien déroulée
    /// </summary>
    /// <param name="section"></param>
    /// <param name="sheetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapAndSaveAsync(IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        var statistic = await ScrapStatisticAsync(section, sheetId);
        if (statistic == null)
            return new OperationState<int>(false, "Impossible de récupérer les statistiques de la fiche");

        return await statistic.AddOrUpdateAsync(cancellationToken);
    }

    /// <summary>
    /// Scrap les statistiques d'une fiche
    /// </summary>
    /// <param name="sheetId">Id de la fiche</param>
    /// <param name="section">Section de la fiche</param>
    /// <returns></returns>
    public static async Task<TsheetStatistic?> ScrapStatisticAsync(IcotakuSection section, int sheetId)
    {
        var url = IcotakuWebHelpers.GetSheetStatisticUrl(section, sheetId);
        if (url == null || url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        HtmlWeb htmlWeb = new HtmlWeb();
        var htmlDocument = await htmlWeb.LoadFromWebAsync(url);

        var contentNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'contenu')]/div[1]");
        if (contentNode == null)
            return null;

        var (creatingDate, createdBy) = ScrapCreatingDate(ref contentNode);
        var (updatedDate, updatedBy) = ScrapUpdatedDate(ref contentNode);
        var inWatchListAverageAge = ScrapInWatchListAverageAge(ref contentNode);
        var (visitorCount, lastVisitorName) = ScrapVisitorCount(ref contentNode);

        return new TsheetStatistic
        {
            SheetId = sheetId,
            Section = section,
            CreatingDate = creatingDate,
            LastUpdatedDate = updatedDate,
            CreatedBy = createdBy,
            LastUpdatedBy = updatedBy,
            InWatchListAverageAge = inWatchListAverageAge,
            VisitCount = visitorCount,
            LastVisitedBy = lastVisitorName,
        };
    }

    /// <summary>
    /// Scrap la date de création et le nom du créateur de la fiche
    /// </summary>
    /// <param name="contentNode"></param>
    /// <returns></returns>
    private static (DateTime? CreatingDate, string? CreatedBy) ScrapCreatingDate(ref HtmlNode contentNode)
    {
        var creatingDateAndCreatorNameNode = contentNode.SelectSingleNode("./p[1]");
        if (creatingDateAndCreatorNameNode == null)
            return (null, null);

        var creatingDateAndCreatorNameText = HttpUtility.HtmlDecode(creatingDateAndCreatorNameNode.InnerText)?.Trim();
        if (creatingDateAndCreatorNameText == null || creatingDateAndCreatorNameText.IsStringNullOrEmptyOrWhiteSpace())
            return (null, null);

        DateTime? creatingDate = null;
        var fullDateMatch = GetFrenchFullDate().Match(creatingDateAndCreatorNameText);
        if (fullDateMatch.Success && !fullDateMatch.Value.IsStringNullOrEmptyOrWhiteSpace())
            if (DateTime.TryParseExact(fullDateMatch.Value, "dd/MM/yyyy à HH:mm", null, System.Globalization.DateTimeStyles.None, out var date))
                creatingDate = date;

        var indexOfPar = creatingDateAndCreatorNameText.IndexOf("par", StringComparison.OrdinalIgnoreCase);
        if (indexOfPar == -1)
            return (creatingDate, null);

        var creatorName = creatingDateAndCreatorNameText.Substring(indexOfPar + 3).Trim().TrimEnd('.');
        if (creatorName.IsStringNullOrEmptyOrWhiteSpace())
            return (creatingDate, null);

        return (creatingDate, creatorName);
    }

    /// <summary>
    /// Scrap la date de la dernière mise à jour et le nom du dernier membre à l'avoir mise à jour
    /// </summary>
    /// <param name="contentNode"></param>
    /// <returns></returns>
    private static (DateTime? UpdatedDate, string? UpdatedBy) ScrapUpdatedDate(ref HtmlNode contentNode)
    {
        var updatedDateAndUpdaterNameNode = contentNode.SelectSingleNode("./p[2]");
        if (updatedDateAndUpdaterNameNode == null)
            return (null, null);

        var updatedDateAndUpdaterNameText = HttpUtility.HtmlDecode(updatedDateAndUpdaterNameNode.InnerText)?.Trim();
        if (updatedDateAndUpdaterNameText == null || updatedDateAndUpdaterNameText.IsStringNullOrEmptyOrWhiteSpace())
            return (null, null);

        DateTime? updatedDate = null;
        var fullDateMatch = GetFrenchFullDate().Match(updatedDateAndUpdaterNameText);
        if (fullDateMatch.Success && !fullDateMatch.Value.IsStringNullOrEmptyOrWhiteSpace())
            if (DateTime.TryParseExact(fullDateMatch.Value, "dd/MM/yyyy à HH:mm", null, System.Globalization.DateTimeStyles.None, out var date))
                updatedDate = date;

        var indexOfPar = updatedDateAndUpdaterNameText.IndexOf("par", StringComparison.OrdinalIgnoreCase);
        if (indexOfPar == -1)
            return (updatedDate, null);

        var updaterName = updatedDateAndUpdaterNameText.Substring(indexOfPar + 3).Trim().TrimEnd('.');
        if (updaterName.IsStringNullOrEmptyOrWhiteSpace())
            return (updatedDate, null);

        return (updatedDate, updaterName);
    }

    /// <summary>
    /// Scrap l'âge moyen des membres ayant cet animé dans leur watchlist
    /// </summary>
    /// <param name="contentNode"></param>
    /// <returns></returns>
    private static float? ScrapInWatchListAverageAge(ref HtmlNode contentNode)
    {
        var inWatchListAverageAgeNode = contentNode.SelectSingleNode("./p[3]");
        if (inWatchListAverageAgeNode == null)
            return null;

        var inWatchListAverageAgeText = HttpUtility.HtmlDecode(inWatchListAverageAgeNode.InnerText).Trim();
        if (inWatchListAverageAgeText.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var ageMatch = GetFrenchAge().Match(inWatchListAverageAgeText);
        if (ageMatch.Success && !ageMatch.Value.IsStringNullOrEmptyOrWhiteSpace())
        {
            var ageString = ageMatch.Groups[1].Value;
            if (!ageString.IsStringNullOrEmptyOrWhiteSpace() && ageString.Contains('.'))
                ageString = ageString.Replace('.', ',');
            if (float.TryParse(ageString, out float age))
                return age;
        }

        return null;
    }

    /// <summary>
    /// Scrap le nombre de visite qu'a eu cette fiche jusqu'à présent et le nom du dernier membre à l'avoir visité
    /// </summary>
    /// <param name="contentNode"></param>
    /// <returns></returns>
    private static (uint VisitorCount, string? LastVisitorName) ScrapVisitorCount(ref HtmlNode contentNode)
    {
        uint visitorCount = 0;
        string? lastVisitorName = null;

        var visitorCountNode = contentNode.SelectSingleNode("./p[4]/text()[1]");
        if (visitorCountNode != null)
        {
            var visitorCountText = HttpUtility.HtmlDecode(visitorCountNode.InnerText).Trim();
            if (!visitorCountText.IsStringNullOrEmptyOrWhiteSpace())
            {
                var split = visitorCountText.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    var visitorCountString = split[1];
                    if (uint.TryParse(visitorCountString, out var count))
                        visitorCount = count;
                }
            }
        }

        var lastVisitorNameNode = contentNode.SelectSingleNode("./p[4]/text()[2]");
        if (lastVisitorNameNode != null)
        {
            var lastVisitorNameText = HttpUtility.HtmlDecode(lastVisitorNameNode.InnerText).Trim();
            if (!lastVisitorNameText.IsStringNullOrEmptyOrWhiteSpace())
            {
                var split = lastVisitorNameText.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (split.Length == 2)
                {
                    var lastVisitorNameString = split[1];
                    if (!lastVisitorNameString.IsStringNullOrEmptyOrWhiteSpace())
                        lastVisitorName = lastVisitorNameString;
                }
            }
        }

        return (visitorCount, lastVisitorName);
    }
}