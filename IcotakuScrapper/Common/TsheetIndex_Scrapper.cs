using System.Diagnostics;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Common;

public partial class TsheetIndex
{
    /// <summary>
    /// Crée les index de toutes les fiches de la liste des animes
    /// </summary>
    /// <param name="contentSection"></param>
    /// <param name="sheetType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> CreateIndexesAsync(IcotakuSection contentSection, IcotakuSheetType sheetType,
        CancellationToken? cancellationToken = null)
    {
        var (minPage, maxPage) = GetMinAndMaxPage(contentSection);
        if (minPage == 0 || maxPage == 0)
            return new OperationState(false, "Impossible de récupérer le nombre de pages de la liste des animes.");

        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();
        await DeleteAllAsync(contentSection, sheetType, cancellationToken, command);

        List<OperationState> results = [];
        for (var i = (uint)minPage; i <= maxPage; i++)
        {
            var pageResults = GetSheetIndexes(contentSection, sheetType, i).ToArray();
            if (pageResults.Length == 0)
                continue;
            var result = await InsertAsync(pageResults, DbInsertMode.InsertOrReplace, cancellationToken, command);
            Debug.WriteLine(
                $"Page {i} :: Nombre: {pageResults.Length}, Succès: {result.IsSuccess}, Message: {result.Message}");
            results.Add(result);
        }

        return results.All(a => a.IsSuccess)
            ? new OperationState(true, "Tous les index ont été créés avec succès.")
            : new OperationState(false, "Une ou plusieurs erreurs sont survenues lors de la création des index.");
    }


    private static IEnumerable<TsheetIndex> GetSheetIndexes(IcotakuSection contentSection, IcotakuSheetType sheetType, uint currentPage = 1)
    {
        //url de la page en cours contenant le tableau des fiches
        var pageUrl = IcotakuWebHelpers.GetIcotakuFilterUrl(contentSection, sheetType, currentPage);
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);

        //Obtient la liste des urls des fiches
        var sheetUrlsNode = htmlDocument.DocumentNode
            .SelectNodes("//div[@id='page']/table[@class='table_apercufiche']//div[@class='td_apercufiche']/a[2]")?
            .ToArray();
        if (sheetUrlsNode == null || sheetUrlsNode.Length == 0)
            yield break;

        foreach (var htmlNode in sheetUrlsNode)
        {
            if (htmlNode == null)
                continue;

            var sheetIndex = GetSheetIndex(htmlNode, contentSection, sheetType, currentPage);
            if (sheetIndex != null)
                yield return sheetIndex;
        }
    }

    /// <summary>
    /// Crée un index à partir d'un noeud HTML
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="contentSection"></param>
    /// <param name="sheetType"></param>
    /// <param name="currentPage"></param>
    /// <returns></returns>
    private static TsheetIndex? GetSheetIndex(HtmlNode htmlNode, IcotakuSection contentSection, IcotakuSheetType sheetType, uint currentPage)
    {
        var sheetHref = htmlNode.GetAttributeValue("href", string.Empty);
        if (sheetHref.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var split = sheetHref.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
            return null;
        var sheetId = split.FirstOrDefault(f =>
            !f.Any(a =>
                char.IsLetter(a) || a == '-' || a == '_'));

        if (sheetId.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (!int.TryParse(sheetId, out var sheetIdInt))
            return null;

        sheetHref = "https://anime.icotaku.com" + sheetHref;
        if (!Uri.TryCreate(sheetHref, UriKind.Absolute, out var sheetUri))
            return null;

        var sheetName = HttpUtility.HtmlDecode(htmlNode.InnerText.Trim());

        return new TsheetIndex()
        {
            SheetId = sheetIdInt,
            Section = contentSection,
            SheetType = sheetType,
            Url = sheetUri.ToString(),
            SheetName = sheetName,
            FoundedPage = currentPage
        };
    }

    /// <summary>
    /// Retourne le nombre de pages de la liste des animes
    /// </summary>
    /// <returns></returns>
    private static (int minPage, int maxPage) GetMinAndMaxPage(IcotakuSection contentSection)
    {
        HtmlWeb web = new();

        var url = contentSection switch
        {
            IcotakuSection.Anime => "https://anime.icotaku.com/animes.html?filter=all",
            _ => throw new ArgumentOutOfRangeException(nameof(contentSection), contentSection, null)
        };

        var htmlDocument = web.Load(url);

        var minPageNode =
            htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[1]");
        var maxPageNode =
            htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[last()]");

        if (minPageNode is null || maxPageNode is null)
            return (0, 0);

        var minPageHref = minPageNode.GetAttributeValue("href", string.Empty);
        var maxPageHref = maxPageNode.GetAttributeValue("href", string.Empty);
        if (minPageHref.IsStringNullOrEmptyOrWhiteSpace() || maxPageHref.IsStringNullOrEmptyOrWhiteSpace())
            return (0, 0);

        minPageHref = HttpUtility.UrlDecode("https://anime.icotaku.com" + minPageHref).Replace("&amp;", "&");
        maxPageHref = HttpUtility.UrlDecode("https://anime.icotaku.com" + maxPageHref).Replace("&amp;", "&");

        if (!Uri.TryCreate(minPageHref, UriKind.Absolute, out var minPageUri) ||
            !Uri.TryCreate(maxPageHref, UriKind.Absolute, out var maxPageUri))
            return (0, 0);

        var minPage = HttpUtility.ParseQueryString(minPageUri.Query).Get("page");
        var maxPage = HttpUtility.ParseQueryString(maxPageUri.Query).Get("page");
        if (minPage is null || maxPage is null)
            return (0, 0);

        if (int.TryParse(minPage, out var minPageInt) && int.TryParse(maxPage, out var maxPageInt))
            return (minPageInt, maxPageInt);

        return (0, 0);
    }
}