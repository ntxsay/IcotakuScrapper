using IcotakuScrapper.Common;

namespace IcotakuScrapper.Anime;

public class Anime : AnimeBase
{

    public List<AlternativeTitle> AlternativeTitles { get; set; } = [];

    public static async Task<Anime> GetAnimeFromId(int icotakuId)
    {
        var anime = new Anime();
        var html = await GetHtmlAsync(url);
        if (html.IsSuccess)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html.Value);
            anime = await GetAnimeAsync(doc);
        }

        return anime;
    }
}
