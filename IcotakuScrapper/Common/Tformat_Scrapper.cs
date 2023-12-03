using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;

namespace IcotakuScrapper.Common;

public partial class Tformat
{
    /// <summary>
    /// Retourne l'url de la page de format depuis icotaku.com
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public static string? GetFormatUrl(IcotakuSection section)
    {
        var baseUrl = IcotakuWebHelpers.GetBaseUrl(section);
        if (baseUrl.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        return baseUrl + "/recherche-avancee.html";
    }

    
    /// <summary>
    /// Scrape les formats depuis icotaku.com
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> ScrapAsync(HashSet<IcotakuSection> sections,
         DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
    {
        if (sections.Count == 0)
            return new OperationState(false, "Aucune section n'a été spécifiée");

        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

        List<Tformat> values =  [];

        foreach (var section in sections)
        {
            if (isDeleteSectionRecords)
            {
                var deleteResult = await DeleteAllAsync(section, cancellationToken, command);
                if (!deleteResult.IsSuccess)
                    return deleteResult;
            }

            var tformats = ScrapFromFormatArrayPage(section);
            if (tformats.Length > 0)
                values.AddRange(tformats);
        }

        if (values.Count == 0)
            return new OperationState(false, "Aucun format n'a été trouvé");


        return await InsertOrReplaceAsync(values, insertMode, cancellationToken, command);
    }

    private static Tformat[] ScrapFromFormatArrayPage(IcotakuSection section)
    {
        var pageUrl = GetFormatUrl(section);
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);

        return htmlDocument.DocumentNode.SelectNodes("//select[@id='categorie']//option[@value!='']")
            ?.Where(w => !w.InnerText.IsStringNullOrEmptyOrWhiteSpace()).Select(s => new Tformat()
            {
                Name = HttpUtility.HtmlDecode(s.InnerText.Trim()).Trim(),
                Section = section
            }).ToArray() ?? Array.Empty<Tformat>();
    }

}