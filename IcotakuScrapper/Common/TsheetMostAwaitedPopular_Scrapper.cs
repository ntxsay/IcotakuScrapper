using System.Collections.Frozen;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Common;

public partial class TsheetMostAwaitedPopular
{
    [GeneratedRegex(@"\d+(\.\d+)?(?=/10)")]
    internal static partial Regex GetNoteRegex();

    [GeneratedRegex(@"(\d+)")]
    internal static partial Regex GetVoteCountRegex();

    public static async Task<TsheetMostAwaitedPopular[]> ScrapPageAsync(uint length, IcotakuSection section, IcotakuListType listType)
    {
        //récupère l'url de la page
        var url= listType switch
        {
            IcotakuListType.MostPopular => IcotakuWebHelpers.GetMostPopularUrl(section, 1),
            IcotakuListType.MostAwaited => IcotakuWebHelpers.GetMostAwaitedUrl(section, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(listType), listType, "La valeur spécifiée est invalide")
        };
        
        //Charge la page (la première page)
        HtmlWeb htmlWeb = new();
        var htmlDocument = await htmlWeb.LoadFromWebAsync(url);
        
        //prépare les tâches
        List<Task<TsheetMostAwaitedPopular[]>> tasks = [];
        
        //récupère le nombre de pages
        var pages = GetSearchMinAndMaxPage(htmlDocument.DocumentNode, section);
        
        //si il y a plus d'une page, on scrap les autres pages en parallèle
        if (pages.maxPage > 1)
            for (var i = pages.minPage + 1; i <= pages.maxPage; i++)
            {
                await Task.Delay(100);
                tasks.Add(ScrapPageAsync(section, listType, i));
            }

        //prépare la liste des résultats
        List<TsheetMostAwaitedPopular> sheets = [];
        
        //scrap la première page et ajoute les résultats à la liste
        sheets.AddRange(ScrapPage(htmlDocument.DocumentNode, section, listType));

        //attends que toutes les tâches soient terminées
        var results = await Task.WhenAll(tasks);
        
        //ajoute les résultats des autres pages à la liste
        foreach (var sheetArray in results.Where(s => s.Length > 0))
            sheets.AddRange(sheetArray);
        
        //libère la mémoire
        tasks.ForEach(f => f.Dispose());
        tasks.Clear();
        
        //retourne les résultats
        return sheets.ToArray();
    }

    private static async Task<TsheetMostAwaitedPopular[]> ScrapPageAsync(IcotakuSection section, IcotakuListType listType, uint page)
    {
        return await ScrapPage(section, listType, page).ToArrayAsync();
    }
    
    private static async IAsyncEnumerable<TsheetMostAwaitedPopular> ScrapPage(IcotakuSection section, IcotakuListType listType, uint page)
    {
        var url = listType switch
        {
            IcotakuListType.MostPopular => IcotakuWebHelpers.GetMostPopularUrl(section, page),
            IcotakuListType.MostAwaited => IcotakuWebHelpers.GetMostAwaitedUrl(section, page),
            _ => throw new ArgumentOutOfRangeException(nameof(listType), listType, "La valeur spécifiée est invalide")
        };
        
        HtmlWeb htmlWeb = new();
        var htmlDocument = await htmlWeb.LoadFromWebAsync(url);

        var documentNode = htmlDocument.DocumentNode;

        foreach (var sheet in ScrapPage(documentNode, section, listType))
        {
            yield return sheet;
        }
    }
    
    private static IEnumerable<TsheetMostAwaitedPopular> ScrapPage(HtmlNode documentNode, IcotakuSection section, IcotakuListType listType)
    {
        var tableNode = documentNode.SelectSingleNode("//table[contains(@class, 'table_classement')]");
        if (tableNode == null)
            yield break;
        
        var trNodes = tableNode.SelectNodes(".//tr")?.ToArray();
        if (trNodes == null || trNodes.Length == 0)
            yield break;

        foreach (var trNode in trNodes)
        {
            var rankNode = trNode.SelectSingleNode(".//td[contains(@class, 'td_rank ')]/span[contains(@class, 'number')]/text()");
            if (rankNode == null || rankNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
                continue;
            if (!int.TryParse(rankNode.InnerText, out var rank)) 
                continue;
            
            var sheetNode = trNode.SelectSingleNode(".//td[contains(@class, 'td_apercufiche')]/div[1]/a");
            if (sheetNode == null)
                continue;
                
            var title = HttpUtility.HtmlDecode(sheetNode.InnerText.Trim());
            var href = IcotakuWebHelpers.GetFullHrefFromHtmlNode(sheetNode, section);
            if (href == null)
                continue;
            
            var sheetId = IcotakuWebHelpers.GetSheetId(href);
            if (sheetId == 0)
                continue;

            var sheet = new TsheetMostAwaitedPopular()
            {
                Rank = rank,
                SheetName = title,
                Url = href.ToString(),
                SheetId = sheetId,
                ListType = listType,
                Section = section,
            };
            
            if (listType == IcotakuListType.MostPopular)
            {
                var scoreNode = trNode.SelectSingleNode(".//td[contains(@class, 'td_note')]/p[contains(@class, 'note')]/text()");
                if (scoreNode == null || scoreNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
                {
                    yield return sheet;
                    continue;
                }

                if (!double.TryParse(scoreNode.InnerText.Replace('.', ','), out var score))
                {
                    yield return sheet;
                    continue;
                }   
                
                var voteCountNode = trNode.SelectSingleNode(".//td[contains(@class, 'td_note')]/p[contains(@class, 'note_par')]/text()");
                if (voteCountNode == null || voteCountNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
                {
                    yield return sheet;
                    continue;
                }
                
                var match = GetVoteCountRegex().Match(voteCountNode.InnerText.Trim());
                if (!match.Success)
                {
                    yield return sheet;
                    continue;
                }

                var value = match.Groups[1].Value;
                if (value.IsStringNullOrEmptyOrWhiteSpace())
                {
                    yield return sheet;
                    continue;
                }

                if (!int.TryParse(value, out var result))
                {
                    yield return sheet;
                    continue;
                }
                
                sheet.VoteCount = result;
            }
            else if (listType == IcotakuListType.MostAwaited)
            {
                var voteCountNode = trNode.SelectSingleNode(".//td[contains(@class, 'td_note')]/p[contains(@class, 'note')]/text()");
                if (voteCountNode == null || voteCountNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
                {
                    yield return sheet;
                    continue;
                }

                var match = GetVoteCountRegex().Match(voteCountNode.InnerText.Trim());
                if (!match.Success)
                {
                    yield return sheet;
                    continue;
                }

                var value = match.Groups[1].Value;
                if (value.IsStringNullOrEmptyOrWhiteSpace())
                {
                    yield return sheet;
                    continue;
                }

                if (!int.TryParse(value, out var result))
                {
                    yield return sheet;
                    continue;
                }
                
                sheet.VoteCount = result;        
            }
            
            yield return sheet;
        }
        
    }
    
    internal static (uint minPage, uint maxPage) GetSearchMinAndMaxPage(HtmlNode documentNode, IcotakuSection section)
    {
        var minPageNode =
            documentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[1]");
        var maxPageNode =
            documentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[last()]");

        if (minPageNode is null || maxPageNode is null)
            return (1, 1);

        var minPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(minPageNode, section);
        var maxPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(maxPageNode, section);
        if (minPageUri == null || maxPageUri == null)
            return (1, 1);

        uint minPage = 1;
        if (uint.TryParse(minPageUri.Segments.FirstOrDefault(f => f.EndsWith(".html"))?.Replace(".html", ""), out var minpage))
            minPage = minpage;
        
        uint maxPage = 1;
        if (uint.TryParse(maxPageUri.Segments.FirstOrDefault(f => f.EndsWith(".html"))?.Replace(".html", ""), out var maxpage))
            maxPage = maxpage;

        return (minPage, maxPage);
    }
}