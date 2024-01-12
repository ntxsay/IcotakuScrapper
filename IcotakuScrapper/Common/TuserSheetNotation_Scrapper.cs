using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Common;

public partial class TuserSheetNotation
{
    public static async Task<OperationState<int>> ScrapAndSaveAsync(IcotakuConnexion icotakuConnexion, IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        var statistic = await ScrapAsync(icotakuConnexion, section, sheetId, cancellationToken);
        if (statistic == null)
            return new OperationState<int>(false, "Impossible de récupérer l'évaluation de la fiche");

        return await statistic.AddOrUpdateAsync(cancellationToken);
    }
    
    public static async Task<TuserSheetNotation?> ScrapAndGetAsync(IcotakuConnexion icotakuConnexion, IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        var result = await ScrapAndSaveAsync(icotakuConnexion, section, sheetId, cancellationToken);
        if (!result.IsSuccess)
            return null;
        
        return await SingleByIdAsync(result.Data, cancellationToken);
    }
    
    public static async Task<TuserSheetNotation?> ScrapAsync(IcotakuConnexion icotakuConnexion, IcotakuSection section, int sheetId, CancellationToken? cancellationToken = null)
    {
        var url = IcotakuWebHelpers.GetSheetEvaluationUrl(section, sheetId);
        if (url == null)
            return null;
        
        var htmlString = await icotakuConnexion.GetHtmlStringAsync(new Uri(url));
        if (htmlString == null)
            return null;
        
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);
        
        var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='tableau_edition']");
        if (tableNode == null)
            return null;

        var sheetNotation = new TuserSheetNotation
        {
            Section = section,
            SheetId = sheetId,
            Note = ScrapNote(ref tableNode),
            WatchStatus = ScrapWatchStatus(ref tableNode),
            PublicComment = ScrapPublicComment(ref tableNode),
            PrivateComment = ScrapPrivateComment(ref tableNode)
        };
        
        return sheetNotation;
    }
    
    /// <summary>
    /// Retourne la note qu'a eu cet oeuvre par l'utilisateur
    /// </summary>
    /// <param name="tableNode"></param>
    /// <returns></returns>
    private static float? ScrapNote(ref HtmlNode tableNode)
    {
        var noteNode = tableNode.SelectSingleNode(".//input[@id='note']");
        if (noteNode == null)
            return null;
        
        var note = noteNode.GetAttributeValue("value", string.Empty);
        if (note.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        note = note.Replace('.', ',');
        if (!float.TryParse(note, out var noteValue))
            return null;
        
        return noteValue;
    }
    
    private static WatchStatusKind ScrapWatchStatus(ref HtmlNode tableNode)
    {
        var watchStatusNode = tableNode.SelectSingleNode(".//select[@id='statut']/option[@selected='selected']");
        if (watchStatusNode == null)
            return WatchStatusKind.NotPlanned;
        
        var watchStatus = watchStatusNode.GetAttributeValue("value", string.Empty);
        if (watchStatus.IsStringNullOrEmptyOrWhiteSpace())
            return WatchStatusKind.NotPlanned;
        
        return watchStatus switch
        {
            "a_commencer" => WatchStatusKind.Planned,
            "en_cours" => WatchStatusKind.InProgress,
            "en_pause" => WatchStatusKind.Paused,
            "abandonne" => WatchStatusKind.Dropped,
            "termine" => WatchStatusKind.Completed,
            _ => WatchStatusKind.NotPlanned
        };
    }
    
    private static string? ScrapPublicComment(ref HtmlNode tableNode)
    {
        var publicCommentNode = tableNode.SelectSingleNode(".//textarea[@id='commentaire_public']");
        if (publicCommentNode == null)
            return null;
        
        var publicComment = HttpUtility.HtmlDecode(publicCommentNode.InnerText);
        if (publicComment.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        return publicComment;
    }
    
    private static string? ScrapPrivateComment(ref HtmlNode tableNode)
    {
        var privateCommentNode = tableNode.SelectSingleNode(".//textarea[@id='commentaire_prive']");
        if (privateCommentNode == null)
            return null;
        
        var privateComment = HttpUtility.HtmlDecode(privateCommentNode.InnerText);
        if (privateComment.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        return privateComment;
    }
    
    public async Task<bool> PostNotationAsync(IcotakuConnexion icotakuConnexion, CancellationToken? cancellationToken = null)
    {
        return await PostNotationAsync(icotakuConnexion, this, cancellationToken);
    }
    
    public static async Task<bool> PostNotationAsync(IcotakuConnexion icotakuConnexion, TuserSheetNotation value, CancellationToken? cancellationToken = null)
    {
        var url = IcotakuWebHelpers.GetSheetEvaluationUrl(value.Section, value.SheetId);
        if (url == null)
            return false;
        
        var htmlString = await icotakuConnexion.GetHtmlStringAsync(new Uri(url));
        if (htmlString == null)
            return false;
        
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlString);

        var formNode = htmlDocument.DocumentNode.SelectSingleNode("//form[@id='form_wl']");
        if (formNode == null)
            return false;
        
        var action = formNode.GetAttributeValue("action", string.Empty);
        if (action.IsStringNullOrEmptyOrWhiteSpace())
            return false;

        #region csrfToken

        var csrfTokenNode = formNode.SelectSingleNode(".//input[@name='_csrf_token']");
        if (csrfTokenNode == null)
            return false;

        var csrfToken = csrfTokenNode.GetAttributeValue("value", string.Empty);
        if (csrfToken.IsStringNullOrEmptyOrWhiteSpace())
            return false;

        #endregion
        
        var formData = new Dictionary<string, string>
        {
            { "_csrf_token", csrfToken },
            { "anime_id", value.SheetId.ToString() },
            { "statut", value.WatchStatus switch
                {
                    WatchStatusKind.NotPlanned => "non_planifie",
                    WatchStatusKind.Planned => "a_commencer",
                    WatchStatusKind.InProgress => "en_cours",
                    WatchStatusKind.Paused => "en_pause",
                    WatchStatusKind.Dropped => "abandonne",
                    WatchStatusKind.Completed => "termine",
                    _ => "non_planifie"
                }
            },
            { "note", value.Note?.ToString() ?? string.Empty },
            { "commentaire_public", value.PublicComment ?? string.Empty },
            { "commentaire_prive", value.PrivateComment ?? string.Empty }
        };

        var postResult = await icotakuConnexion.PostAsync(action, new FormUrlEncodedContent(formData), cancellationToken);
        return postResult.IsSucces;
    }
}