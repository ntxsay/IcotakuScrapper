using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;

namespace IcotakuScrapper.Common;

public partial class TorigineAdaptation
{
    /// <summary>
    /// Retourne l'url de la page des origines depuis icotaku.com
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public static string? GetOriginesUrl(IcotakuSection section)
    {
        var baseUrl = IcotakuWebHelpers.GetBaseUrl(section);
        if (baseUrl.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        return baseUrl + "/recherche-avancee.html";
    }


    /// <summary>
    /// Scrape les origines depuis icotaku.com
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="insertMode"></param>
    /// <param name="isDeleteSectionRecords">Indique s'il faut supprimer les enregistrements existants concernant cette section</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> ScrapAsync(HashSet<IcotakuSection> sections, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
    {
        if (sections.Count == 0)
            return new OperationState(false, "Aucune section n'a été spécifiée");

        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

        List<TorigineAdaptation> values =  [];

        foreach (var section in sections)
        {
            if (isDeleteSectionRecords)
            {
                var deleteAllResult = await DeleteAllAsync(section, cancellationToken, command);
                if (!deleteAllResult.IsSuccess)
                    continue;
            }
            
            var tvalues = ScrapFromOrigineArrayPage(section);
            if (tvalues.Length > 0)
                values.AddRange(tvalues);
        }

        if (values.Count == 0)
            return new OperationState(false, "Aucune origine n'a été trouvé");


        return await InsertOrReplaceAsync(values, insertMode, cancellationToken, command);
    }

    private static TorigineAdaptation[] ScrapFromOrigineArrayPage(IcotakuSection section)
    {
        var pageUrl = GetOriginesUrl(section);
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);

        return htmlDocument.DocumentNode.SelectNodes("//select[@id='origine']//option[@value!='']")
            ?.Where(w => !w.InnerText.IsStringNullOrEmptyOrWhiteSpace()).Select(s => new TorigineAdaptation()
            {
                Name = HttpUtility.HtmlDecode(s.InnerText.Trim()).Trim(),
                Section = section
            }).ToArray() ?? Array.Empty<TorigineAdaptation>();
    }

}