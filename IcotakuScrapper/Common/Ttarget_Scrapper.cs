using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Common;

public partial class Ttarget
{
    /// <summary>
    /// Retourne l'url de la page des Publics visés depuis icotaku.com
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public static string? GetTargetsUrl(IcotakuSection section)
    {
        var baseUrl = Main.GetBaseUrl(section);
        if (baseUrl.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        return baseUrl + "/recherche-avancee.html";
    }


    /// <summary>
    /// Scrape les Publics visés depuis icotaku.com
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

        List<Ttarget> values =  [];

        foreach (var section in sections)
        {
            if (isDeleteSectionRecords)
            {
                var deleteAllResult = await DeleteAllAsync(section, cancellationToken, command);
                if (!deleteAllResult.IsSuccess)
                    continue;
            }
            
            var tvalues = ScrapFromTargetArrayPage(section);
            if (tvalues.Length > 0)
                values.AddRange(tvalues);
        }

        if (values.Count == 0)
            return new OperationState(false, "Aucune origine n'a été trouvé");


        return await InsertOrReplaceAsync(values, insertMode, cancellationToken, command);
    }

    private static Ttarget[] ScrapFromTargetArrayPage(IcotakuSection section)
    {
        var pageUrl = GetTargetsUrl(section);
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);

        return htmlDocument.DocumentNode.SelectNodes("//select[@id='origine']//option[@value!='']")
            ?.Where(w => !w.InnerText.IsStringNullOrEmptyOrWhiteSpace()).Select(s => new Ttarget()
            {
                Name = HttpUtility.HtmlDecode(s.InnerText.Trim()).Trim(),
                Section = section
            }).ToArray() ?? Array.Empty<Ttarget>();
    }
}