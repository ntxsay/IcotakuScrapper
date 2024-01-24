using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Services;

public static partial class IcotakuHelpers
{
    public static class Scrapper
    {
        /// <summary>
        /// Retourne le nombre de pages de la liste des animes
        /// </summary>
        /// <returns></returns>
        internal static (uint minPage, uint maxPage) GetMinAndMaxPage(HtmlNode documentNode,
            IcotakuSection section, bool hasQueryPage = true, string? pageQueryName = "page")
        {
            var stringUri =IcotakuWebHelpers.GetBaseUrl(section);
            if (stringUri == null)
                return (1, 1);
            
            var minPageNode =
                documentNode.SelectSingleNode("//div[@class='anime_pager']/a[1]");
            var maxPageNode =
                documentNode.SelectSingleNode("//div[@class='anime_pager']/a[last()]");

            if (minPageNode is null || maxPageNode is null)
                return (1, 1);

            var minPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(minPageNode, section);
            var maxPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(maxPageNode, section);
            if (minPageUri is null || maxPageUri is null)
                return (1, 1);
            
            if (hasQueryPage)
            {
                var minPageQuery = HttpUtility.ParseQueryString(minPageUri.Query).Get(pageQueryName);
                var maxPageQuery = HttpUtility.ParseQueryString(maxPageUri.Query).Get(pageQueryName);
                if (minPageQuery is null || maxPageQuery is null)
                    return (1, 1);

                if (uint.TryParse(minPageQuery, out var minPageInt) && uint.TryParse(maxPageQuery, out var maxPageInt))
                    return (minPageInt, maxPageInt);

                return (1, 1);
            }
            else
            {
                var minPage = minPageUri.Segments[^1];
                var maxPage = maxPageUri.Segments[^1];

                if (uint.TryParse(minPage, out var minPageInt) && uint.TryParse(maxPage, out var maxPageInt))
                    return (minPageInt, maxPageInt);
            }
            

            return (1, 1);
        }
    }
}