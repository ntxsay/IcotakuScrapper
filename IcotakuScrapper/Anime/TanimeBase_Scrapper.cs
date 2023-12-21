using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    [GeneratedRegex(@"\d+(\.\d+)?(?=/10)")]
    protected static partial Regex GetNoteRegex();

    [GeneratedRegex(@"(\d+)")]
    protected static partial Regex GetVoteCountRegex();
    
    public static async Task<TanimeBase[]> FindAsync(AnimeFinderParameterStruct finderParameter, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await ScrapAnimeSearchAsync(finderParameter, cancellationToken, cmd).ToArrayAsync();
    
    protected static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(string htmlContent, Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(htmlContent);

        return await ScrapAnimeBaseAsync(htmlDocument.DocumentNode, sheetUri, cancellationToken, cmd);
    }
    
    internal static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);

        return await ScrapAnimeBaseAsync(htmlDocument.DocumentNode, sheetUri, cancellationToken, cmd);
    }
    
    
    protected static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(HtmlNode documentNode, Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        try
        {
            var isAdultContent = ScrapIsAdultContent(documentNode);
            if (Main.IsAccessingToAdultContent == false && isAdultContent)
                return new OperationState<TanimeBase?>(false, "L'anime est considéré comme étant un contenu adulte (Hentai, Yuri, Yaoi).");

            var isExplicitContent = ScrapIsExplicitContent(documentNode);
            if (Main.IsAccessingToExplicitContent == false && isExplicitContent)
                return new OperationState<TanimeBase?>(false, "L'anime est considéré comme étant un contenu explicite (Violence ou nudité explicite).");

            var mainName = ScrapMainName(documentNode);
            if (mainName == null || mainName.IsStringNullOrEmptyOrWhiteSpace())
                throw new Exception("Le nom de l'anime n'a pas été trouvé");

            var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

            var anime = new TanimeBase()
            {
                Name = mainName,
                SheetId = sheetId,
                Url = sheetUri.ToString(),
                IsAdultContent = isAdultContent,
                IsExplicitContent = isExplicitContent,
                Note = ScrapNote(documentNode),
                VoteCount = ScrapVoteCount(documentNode),
                DiffusionState = ScrapDiffusionState(documentNode),
                EpisodesCount = ScrapTotalEpisodes(documentNode),
                Duration = ScrapDuration(documentNode),
                Target = await ScrapTargetAsync(documentNode, cancellationToken, command),
                OrigineAdaptation = await ScrapOrigineAdaptationAsync(documentNode, cancellationToken, command),
                Format = await ScrapFormatAsync(documentNode, cancellationToken, command),
                Season = await ScrapSeason(documentNode, cancellationToken, command),
                ReleaseDate = ScrapBeginDate(documentNode),
                Description = ScrapDescription(documentNode),
                ThumbnailUrl = ScrapFullThumbnail(documentNode),
            };

            //Genres et themes
            var genres = await GetCategoriesAsync(documentNode, CategoryType.Genre, cancellationToken, command).ToArrayAsync();
            var themes = await GetCategoriesAsync(documentNode, CategoryType.Theme, cancellationToken, command).ToArrayAsync();
            List<Tcategory> categories = [.. genres, .. themes];
            if (categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    if (!anime.Categories.Any(a => string.Equals(a.Name, category.Name, StringComparison.OrdinalIgnoreCase) && a.Type == category.Type))
                        anime.Categories.Add(category);
                }
            }

            return new OperationState<TanimeBase?>
            {
                IsSuccess = true,
                Data = anime,
            };
        }
        catch (Exception ex)
        {
            LogServices.LogDebug(ex);
            return new OperationState<TanimeBase?>(false, ex.Message);
        }
    }

    
     #region General
    /// <summary>
    /// Obtient le nom principal de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static string? ScrapMainName(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[@id='fiche_entete']//h1/text()");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var decodedString = HttpUtility.HtmlDecode(text).Trim();
        return decodedString;
    }

    /// <summary>
    /// Retourne la description complète de l'animé
    /// </summary>
    /// <param name="htmlNode">Noeud à partir duquel commencer la recherche</param>
    /// <returns></returns>
    protected static string? ScrapDescription(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Histoire')]/parent::div/p");
        return HttpUtility.HtmlDecode(node?.InnerText?.Trim());
    }

    /// <summary>
    /// Retourne les titres alternatifs de l'animé
    /// </summary>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    protected static IEnumerable<TanimeAlternativeTitle> ScrapAlternativeTitles(HtmlNode documentNode)
    {
        var nodes = documentNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Titre ')]")?.ToArray();

        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var description = HttpUtility.HtmlDecode(node.InnerText.Trim().TrimEnd(':').Trim());
            var title = HttpUtility.HtmlDecode(node.NextSibling.InnerText.Trim());
            if (title.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var splitTitle = title.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitTitle.Length == 0)
                continue;

            foreach (var splittedTitle in splitTitle)
            {
                yield return new TanimeAlternativeTitle()
                {
                    Title = splittedTitle,
                    Description = description,
                };
            }
        }
    }


    /// <summary>
    /// Retourne les sites web de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static IEnumerable<TanimeWebSite> ScrapWebsites(HtmlNode htmlNode)
    {
        var nodes = htmlNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Site ')]")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var description = HttpUtility.HtmlDecode(node.InnerText.Trim());
            var aNodes = node.ParentNode?.SelectNodes("./a")?.ToArray();
            if (aNodes == null || aNodes.Length == 0)
                continue;

            foreach (var node2 in aNodes)
            {
                var url = node2.Attributes["href"]?.Value;
                if (url == null || url.IsStringNullOrEmptyOrWhiteSpace())
                    continue;

                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    yield return new TanimeWebSite()
                    {
                        Url = uri.ToString(),
                        Description = description?.TrimEnd(':').Trim(),
                    };
            }
        }
    }

    protected static double? ScrapNote(HtmlNode documentNode)
    {
        var htmlNode = documentNode.SelectSingleNode("//div[contains(@class, 'contenu')]/div[contains(@class, 'complements')]/p[contains(@class, 'note')]/text()[1]");
        if (htmlNode == null)
            return null;

        var text = htmlNode.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        text = text.Replace("/10", string.Empty).Trim();
        if (!double.TryParse(text, out var result))
            return null;

        return result;
    }

    protected static uint ScrapVoteCount(HtmlNode documentNode)
    {
        var htmlNode = documentNode.SelectSingleNode("//div[contains(@class, 'contenu')]/div[contains(@class, 'complements')]/p[contains(@class, 'note')]/span[contains(@class, 'note_par')]/text()");
        if (htmlNode == null)
            return 0;

        var text = htmlNode.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        var match = GetVoteCountRegex().Match(text);
        if (!match.Success)
            return 0;

        var value = match.Groups[1].Value;
        if (value.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        if (!uint.TryParse(value, out var result))
            return 0;

        return result;

    }

    #endregion

    #region Format
    /// <summary>
    /// Retourne le nombre total d'épisodes de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static ushort ScrapTotalEpisodes(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Nombre d')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        if (ushort.TryParse(text, out ushort result))
            return result;

        return 0;
    }

    /// <summary>
    /// Retourne la durée d'un épisode en moyenne et en minutes
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static TimeSpan ScrapDuration(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Durée d')]/following-sibling::text()[1]");

        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace() || text == "?")
            return TimeSpan.Zero;

        text = Regex.Replace(text, @"[^\d]", "").Trim();
        if (text.IsStringNullOrEmptyOrWhiteSpace())
            return TimeSpan.Zero;

        return ushort.TryParse(text, out ushort result) ? TimeSpan.FromMinutes(result) : TimeSpan.Zero;
    }

    /// <summary>
    /// Retourne l'origine de l'adaptation de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected static async Task<TorigineAdaptation?> ScrapOrigineAdaptationAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Origine :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        TorigineAdaptation record = new()
        {
            Name = text,
            Section = IcotakuSection.Anime,
        };

        return await TorigineAdaptation.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }


    /// <summary>
    /// Retourne le format de l'animé (série, film, oav, etc...)
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected static async Task<Tformat?> ScrapFormatAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Catégorie :')]/following-sibling::text()[1]");
        var text = HttpUtility.HtmlDecode(node?.InnerText?.Trim());
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        Tformat record = new()
        {
            Name = text,
            Section = IcotakuSection.Anime,
        };

        return await Tformat.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }
    #endregion

    #region Diffusion
    /// <summary>
    /// Retourne l'état de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static DiffusionStateKind ScrapDiffusionState(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Diffusion :')]/following-sibling::text()[1]");

        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return DiffusionStateKind.Unknown;

        return text switch
        {
            "Bientôt" => DiffusionStateKind.UpComing,
            "En cours" => DiffusionStateKind.InProgress,
            "En pause" => DiffusionStateKind.Paused,
            "Terminée" => DiffusionStateKind.Completed,
            "Arrêtée" => DiffusionStateKind.Stopped,
            _ => DiffusionStateKind.Unknown,
        };
    }

    /// <summary>
    /// Retourne la saison de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected static async Task<Tseason?> ScrapSeason(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var year = ScrapYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var seasonNumber = ScrapSeasonNumber(htmlNode);
        if (seasonNumber == null)
            return null;

        var intSeason = DateHelpers.GetIntSeason(seasonNumber.Value, year.Value);
        if (intSeason == 0)
            return null;

        var seasonLiteral = DateHelpers.GetSeasonLiteral((WeatherSeasonKind)seasonNumber.Value, year.Value);
        if (seasonLiteral == null)
            return null;

        var record = new Tseason
        {
            SeasonNumber = seasonNumber.Value,
            DisplayName = seasonLiteral,
        };

        return await Tseason.SingleOrCreateAsync(record, false ,cancellationToken, cmd);
    }

    /// <summary>
    /// Retourne le numéro de la saison de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static byte? ScrapSeasonNumber(HtmlNode htmlNode)
    {
        var year = ScrapYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var seasonNode = htmlNode.SelectSingleNode(
            "//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Saison :')]/following-sibling::text()[1]");

        var text = seasonNode?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var number = DateHelpers.GetSeasonNumber(text);
        return number == 0 ? null : number;
    }

    /// <summary>
    /// Retourne la date de début de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static string? ScrapBeginDate(HtmlNode htmlNode)
    {
        var year = ScrapYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var month = ScrapMonthDiffusion(htmlNode);
        if (month == null)
            return $"{year.Value}-00-00";

        return $"{year.Value}-{month.Value:00}-00";
    }

    /// <summary>
    /// Retourne l'année de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static ushort? ScrapYearDiffusion(HtmlNode htmlNode)
    {
        var yearNode = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Année de diffusion :')]/following-sibling::text()[1]");
        if (yearNode == null || yearNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        var yearText = yearNode.InnerText.Trim();
        return ushort.TryParse(yearText, out ushort year) ? year : null;
    }

    /// <summary>
    /// Retourne le mois de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    protected static byte? ScrapMonthDiffusion(HtmlNode htmlNode)
    {
        var monthNode = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Mois de début de diffusion :')]/following-sibling::text()[1]");
        if (monthNode == null || monthNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var monthText = monthNode.InnerText.Trim();

        return DateHelpers.GetMonthNumber(monthText);
    }

    #endregion

    #region Classification
    /// <summary>
    /// Retourne le public visé de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected static async Task<Ttarget?> ScrapTargetAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Public visé :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        Ttarget record = new()
        {
            Name = text,
            Section = IcotakuSection.Anime,
        };

        return await Ttarget.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }

    internal static bool ScrapIsAdultContent(Uri animeSheetUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(animeSheetUri.ToString());
        return ScrapIsAdultContent(htmlDocument.DocumentNode);
    }

    internal static bool ScrapIsAdultContent(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[@id='divFicheHentai']");
        return node != null;
    }

    internal static bool ScrapIsExplicitContent(Uri animeSheetUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(animeSheetUri.ToString());
        return ScrapIsExplicitContent(htmlDocument.DocumentNode);
    }

    internal static bool ScrapIsExplicitContent(HtmlNode htmlNode)
    {
        if (ScrapIsAdultContent(htmlNode))
            return true;
        var node = htmlNode.SelectSingleNode("//div[@id='divFicheError']");
        return node != null;
    }
    #endregion

    #region Categories
    /// <summary>
    /// Retourne les catégories de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="categoryType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    internal static async IAsyncEnumerable<Tcategory> GetCategoriesAsync(HtmlNode htmlNode, CategoryType categoryType, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlNode[]? nodes = categoryType switch
        {
            CategoryType.Genre => htmlNode.SelectNodes("//span[@id='id_genre']/a[contains(@href, '/genre/')]")?.ToArray(),
            CategoryType.Theme => htmlNode.SelectNodes("//span[@id='id_theme']/a[contains(@href, '/theme/')]")?.ToArray(),
            _ => null
        };

        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var name = HttpUtility.HtmlDecode(node.InnerText?.Trim())?.Trim();
            if (name == null || name.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Récupère l'url de la fiche du thème ou du genre
            var uri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(node, IcotakuSection.Anime);
            if (uri == null)
                continue;

            var category = Tcategory.ScrapCategoryFromSheetPage(uri, IcotakuSection.Anime, categoryType);
            if (category == null)
                continue;

            category = await Tcategory.SingleOrCreateAsync(category, true, cancellationToken, cmd);
            if (category == null)
                continue;

            yield return category;
        }
    }
    #endregion

    #region Thumbnail

    /// <summary>
    /// Récupère l'url de la vignette de l'animé
    /// </summary>
    /// <param name="ficheAnimeUri"><see cref="Uri"/> de la fiche anime</param>
    /// <returns>Le lien url de la vignette originale de la fiche anime</returns>
    public static string? GetFullThumbnail(Uri ficheAnimeUri)
    {
        //Charge la page web (fiche anime icotaku)
        HtmlWeb web = new();
        var htmlDocument = web.Load(ficheAnimeUri.OriginalString);

        return ScrapFullThumbnail(htmlDocument.DocumentNode);
    }

    public static string? ScrapFullThumbnail(HtmlNode htmlNode)
    {

        //Récupère le noeud de l'image de la vignette
        var imgNode = htmlNode.SelectSingleNode("//div[contains(@class, 'contenu')]/div[contains(@class, 'complements')]/p/img[contains(@src, '/uploads/animes/')]");
        if (imgNode == null)
            return null;

        //Récupère l'url de l'image de la vignette
        var src = imgNode.Attributes["src"]?.Value;

        //Si l'url est valide, on retourne l'url de l'image de la vignette
        if (src == null)
            return null;

        //Sinon on retourne null
        var uri = IcotakuWebHelpers.GetImageFromSrc(IcotakuSection.Anime, src);
        return uri?.ToString();
    }

    

    #endregion

    
    #region finder
    protected static async IAsyncEnumerable<TanimeBase> ScrapAnimeSearchAsync(AnimeFinderParameterStruct finderParameter, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var uri = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, finderParameter);
        if (uri == null)
            yield break;
        
        HtmlWeb htmlWeb = new();
        var htmlDocument = htmlWeb.Load(uri);
        
        var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[contains(@class, 'table_apercufiche')]");
        if (tableNode == null)
            yield break;
        
        await foreach (var anime in ScrapSearchResult(tableNode, cancellationToken, cmd))
        {
            yield return anime;
        }
        
        var (minPage, maxPage) = GetSearchMinAndMaxPage(uri);
        if (minPage < 1 || maxPage < 2) 
            yield break;
        
        for (var i = minPage + 1; i <= maxPage; i++)
        {
            var pageUri = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, finderParameter, i);
            if (pageUri == null)
                continue;
                
            htmlDocument = htmlWeb.Load(pageUri);
            tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[contains(@class, 'table_apercufiche')]");
            if (tableNode == null)
                continue;
                
            await foreach (var anime in ScrapSearchResult(tableNode, cancellationToken, cmd))
            {
                yield return anime;
            }
        }
    }


    /// <summary>
    /// Retourne le nombre de pages de la liste des animes
    /// </summary>
    /// <returns></returns>
    internal static (uint minPage, uint maxPage) GetSearchMinAndMaxPage(Uri searchResultUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(searchResultUri);

        var minPageNode =
            htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[1]");
        var maxPageNode =
            htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[last()]");

        if (minPageNode is null || maxPageNode is null)
            return (1, 1);
        
        var minPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(minPageNode, IcotakuSection.Anime);
        var maxPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(maxPageNode, IcotakuSection.Anime);
        if (minPageUri == null || maxPageUri == null)
            return (1, 1);

        
        var minPage = HttpUtility.ParseQueryString(minPageUri.Query).Get("page");
        var maxPage = HttpUtility.ParseQueryString(maxPageUri.Query).Get("page");
        if (minPage is null || maxPage is null)
            return (1, 1);


        if (uint.TryParse(minPage, out var minPageInt) && uint.TryParse(maxPage, out var maxPageInt))
            return (minPageInt, maxPageInt);

        return (1, 1);
    }

    internal static (uint minPage, uint maxPage) GetSearchMinAndMaxPage(HtmlNode documentNode)
    {
        var minPageNode =
            documentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[1]");
        var maxPageNode =
            documentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[last()]");

        if (minPageNode is null || maxPageNode is null)
            return (1, 1);
        
        var minPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(minPageNode, IcotakuSection.Anime);
        var maxPageUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(maxPageNode, IcotakuSection.Anime);
        if (minPageUri == null || maxPageUri == null)
            return (1, 1);

        
        var minPage = HttpUtility.ParseQueryString(minPageUri.Query).Get("page");
        var maxPage = HttpUtility.ParseQueryString(maxPageUri.Query).Get("page");
        if (minPage is null || maxPage is null)
            return (1, 1);


        if (uint.TryParse(minPage, out var minPageInt) && uint.TryParse(maxPage, out var maxPageInt))
            return (minPageInt, maxPageInt);

        return (1, 1);
    }


    
    protected static async  IAsyncEnumerable<TanimeBase> ScrapSearchResult(HtmlNode documentNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var tableNode = documentNode.SelectSingleNode("//table[contains(@class, 'table_apercufiche')]");

        var nodes = tableNode?.SelectNodes(".//div[contains(@class, 'td_apercufiche')]/a[1]")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var href = node.Attributes["href"]?.Value;
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var uri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(node, IcotakuSection.Anime);
            if (uri == null)
                continue;
            
            var animeBaseResult = await ScrapAnimeBaseAsync(uri);
            if (!animeBaseResult.IsSuccess || animeBaseResult.Data == null)
                continue;
            
            yield return animeBaseResult.Data;
        }
    }
    
    /// <summary>
    /// Récupère les liens des fiches animes à partir du noeud de la page de recherche
    /// </summary>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    internal static IEnumerable<Uri> ScrapSearchResultUri(HtmlNode documentNode)
    {
        var tableNode = documentNode.SelectSingleNode("//table[contains(@class, 'table_apercufiche')]");

        var nodes = tableNode?.SelectNodes(".//div[contains(@class, 'td_apercufiche')]/a[1]")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var href = node.Attributes["href"]?.Value;
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var uri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(node, IcotakuSection.Anime);
            if (uri == null)
                continue;
            
            yield return uri;
        }
    }

    #endregion
}