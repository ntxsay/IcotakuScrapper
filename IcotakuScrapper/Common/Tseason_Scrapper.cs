using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Common;

public partial class Tseason
{
    public static IEnumerable<Tseason> ScrapSeasons(IcotakuSection section)
    {
        //url de la page en cours contenant le tableau des fiches
        var pageUrl = IcotakuWebHelpers.GetSeasonalPlanningUrl(section);
        if (pageUrl == null || pageUrl.IsStringNullOrEmptyOrWhiteSpace())
            yield break;
        
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);

        var yearValueNodes = htmlDocument.DocumentNode.SelectNodes("//select[@id='annee']/option/@value")?.ToArray();
        if (yearValueNodes == null || yearValueNodes.Length == 0)
            yield break;

        foreach (var node in yearValueNodes)
        {
            var yearText = node.GetAttributeValue("value", null);
            if (yearText == null || yearText.IsStringNullOrEmptyOrWhiteSpace() || !ushort.TryParse(yearText, out var year))
                continue;
            
            for (byte i = 1; i <= 4; i++)
            {
                if (!uint.TryParse($"{year}{i:00}", out var numberedSeason))
                    continue;

                yield return new Tseason()
                {
                    SeasonNumber = numberedSeason,
                    DisplayName = DateHelpers.GetSeasonLiteral(numberedSeason) ?? numberedSeason.ToString()
                };
            }
        }
    }
}