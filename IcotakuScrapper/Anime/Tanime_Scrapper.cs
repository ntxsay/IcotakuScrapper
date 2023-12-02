using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    public static async Task<OperationState<int>> ScrapAnimeFromSheetId(int sheetId,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        
        var index = await TsheetIndex.SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken, cmd);
        if (index == null)
            return new OperationState<int>(false, "L'index permettant de récupérer l'url de la fiche de l'anime n'a pas été trouvé dans la base de données.");
        
        return await ScrapAnimeFromUrl(index.Url, cancellationToken, cmd);
    }
    
    /// <summary>
    /// Récupère l'anime depuis l'url de la fiche icotaku.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapAnimeFromUrl(string url, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");
        
        if (!uri.Host.StartsWith("anime.icotaku.com"))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var animeResult = await ScrapAnimeAsync(uri, cancellationToken, command);

        if (!animeResult.IsSuccess)
            return new OperationState<int>(false, animeResult.Message);

        var anime = animeResult.Data;
        if (anime == null)
            return new OperationState<int>(false, "Une erreur est survenue lors de la récupération de l'anime");

        if (!anime.Url.IsStringNullOrEmptyOrWhiteSpace())
        {
            _ = await CreateIndexAsync(anime.Name, anime.Url, anime.SheetId, cancellationToken, command);
        }
        anime.Url = uri.ToString();

        return await anime.InsertAync(cancellationToken, command);
    }
    
    /// <summary>
    /// Récupère les informations de la fiche anime à partir de son url
    /// </summary>
    /// <param name="uri">Url de la fiche de l'animé</param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(Uri uri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        try
        {
            HtmlWeb web = new();
            var htmlDocument = web.Load(uri.OriginalString);

            var mainName = GetMainName(htmlDocument.DocumentNode);
            if (mainName == null || mainName.IsStringNullOrEmptyOrWhiteSpace())
                throw new Exception("Le nom de l'anime n'a pas été trouvé");

            var sheetId = Main.GetSheetId(uri) ?? throw new Exception("L'id de la fiche anime n'a pas été trouvé.");
            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

            var anime = new Tanime()
            {
                Name = mainName,
                SheetId = sheetId,
                Url = uri.ToString(),
                IsAdultContent = GetIsAdultContent(htmlDocument.DocumentNode),
                IsExplicitContent = GetIsExplicitContent(htmlDocument.DocumentNode),
                DiffusionState = GetDiffusionState(htmlDocument.DocumentNode),
                EpisodesCount = GetTotalEpisodes(htmlDocument.DocumentNode),
                Duration = GetDuration(htmlDocument.DocumentNode),
                Target = await GetTargetAsync(htmlDocument.DocumentNode, cancellationToken, command),
                OrigineAdaptation = await GetOrigineAdaptationAsync(htmlDocument.DocumentNode, cancellationToken, command),
                Format = await GetFormatAsync(htmlDocument.DocumentNode, cancellationToken, command),
                Season = await GetSeason(htmlDocument.DocumentNode, cancellationToken, command),
                ReleaseDate = GetBeginDate(htmlDocument.DocumentNode),
                Description = GetDescription(htmlDocument.DocumentNode),
                ThumbnailUrl = GetFullThumbnail(htmlDocument.DocumentNode),
            };

            //Titres alternatifs
            var alternativeNames = GetAlternativeTitles(htmlDocument.DocumentNode).ToArray();
            if (alternativeNames.Length > 0)
            {
                foreach (var title in alternativeNames)
                {
                    if (!anime.AlternativeTitles.Any(a => string.Equals(a.Title, title.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        anime.AlternativeTitles.Add(title);
                    }
                }
            }

            //Websites
            var websites = GetWebsites(htmlDocument.DocumentNode).ToArray();
            if (websites.Length > 0)
            {
                foreach (var url in websites)
                {
                    if (!anime.WebSites.Any(a => string.Equals(a.Url, url.Url, StringComparison.OrdinalIgnoreCase)))
                    {
                        anime.WebSites.Add(url);
                    }
                }
            }

            //Genres et themes
            var genres = await GetCategoriesAsync(htmlDocument.DocumentNode, CategoryType.Genre, cancellationToken, command).ToArrayAsync();
            var themes = await GetCategoriesAsync(htmlDocument.DocumentNode, CategoryType.Theme, cancellationToken, command).ToArrayAsync();
            List<Tcategory> categories = [.. genres, .. themes];
            if (categories.Count > 0)
            {
                foreach (var category in categories)
                {
                    if (!anime.Categories.Any(a => string.Equals(a.Name, category.Name, StringComparison.OrdinalIgnoreCase) && a.Type == category.Type))
                        anime.Categories.Add(category);
                }
            }

            //Studios
            var studios = await GetStudioAsync(htmlDocument.DocumentNode, cancellationToken, command).ToArrayAsync();
            if (studios.Length > 0)
            {
                foreach (var studio in studios)
                {
                    if (!anime.Studios.Any(a => string.Equals(a.DisplayName, studio.DisplayName, StringComparison.OrdinalIgnoreCase) && string.Equals(a.Url, studio.Url, StringComparison.OrdinalIgnoreCase)))
                        anime.Studios.Add(studio);
                }
            }

            //Episodes
            var episodes = TanimeEpisode.GetAnimeEpisode(anime.SheetId).ToArray();
            if (episodes.Length > 0)
                foreach (var episode in episodes)
                    anime.Episodes.Add(episode);

            return new OperationState<Tanime?>
            {
                IsSuccess = true,
                Data = anime,
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new OperationState<Tanime?>(false, ex.Message);
        }
    }

    /// <summary>
    /// Obtient le nom principal de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static string? GetMainName(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[@id='fiche_entete']//h1/text()");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var decodedString = HttpUtility.HtmlDecode(text)?.Trim();
        return decodedString;
    }

    /// <summary>
    /// Retourne la description complète de l'animé
    /// </summary>
    /// <param name="htmlNode">Noeud à partir duquel commencer la recherche</param>
    /// <returns></returns>
    private static string? GetDescription(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Histoire')]/parent::div/p");
        return HttpUtility.HtmlDecode(node?.InnerText?.Trim());
    }

    /// <summary>
    /// Retourne les titres alternatifs de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static TanimeAlternativeTitle[] GetAlternativeTitles(HtmlNode htmlNode)
    {
        var nodes = htmlNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Titre ')]")?.ToArray();

        if (nodes == null || nodes.Length == 0)
            return [];

        return nodes.Select(s => new TanimeAlternativeTitle()
        {
            Title = HttpUtility.HtmlDecode(s.NextSibling.InnerText.Trim()),
            Description = HttpUtility.HtmlDecode(s.InnerText.Trim()?.TrimEnd(':')?.Trim()),
        }).Where(w => !w.Title.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
    }


    /// <summary>
    /// Retourne les sites web de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static IEnumerable<TanimeWebSite> GetWebsites(HtmlNode htmlNode)
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
                        Description = description?.TrimEnd(':')?.Trim(),
                    };
            }
        }
    }

    /// <summary>
    /// Retourne les catégories de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="categoryType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<Tcategory> GetCategoriesAsync(HtmlNode htmlNode, CategoryType categoryType, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
            var uri = Main.GetFullHrefFromHtmlNode(node, IcotakuSection.Anime);
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

    /// <summary>
    /// Retourne le nombre total d'épisodes de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static ushort GetTotalEpisodes(HtmlNode htmlNode)
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
    private static TimeSpan GetDuration(HtmlNode htmlNode)
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
    private static async Task<TorigineAdaptation?> GetOrigineAdaptationAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Origine :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        TorigineAdaptation? record = new()
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
    private static async Task<Tformat?> GetFormatAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Catégorie :')]/following-sibling::text()[1]");
        var text = HttpUtility.HtmlDecode(node?.InnerText?.Trim());
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        Tformat? record = new()
        {
            Name = text,
            Section = IcotakuSection.Anime,
        };

        return await Tformat.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }

    /// <summary>
    /// Retourne le public visé de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async Task<Ttarget?> GetTargetAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Public visé :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        Ttarget? record = new()
        {
            Name = text,
            Section = IcotakuSection.Anime,
        };

        return await Ttarget.SingleOrCreateAsync(record, true, cancellationToken, cmd);
    }

    /// <summary>
    /// Retourne les studios d'aimation de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<Tcontact> GetStudioAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var _node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Studio')]");
        if (_node == null)
            yield break;

        var nodes = _node.ParentNode?.SelectNodes("./a")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            //Récupère le nom d'affichage du studio
            var displayName = node?.InnerText?.Trim();
            if (displayName == null || displayName.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Récupère l'url de la fiche du studio
            var href = node?.Attributes["href"]?.Value;
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Créer l'url de la fiche du studio
            href = Main.GetBaseUrl(IcotakuSection.Anime) + href;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                continue;

            //Récupère l'id de la fiche du studio
            var sheetId = Main.GetSheetId(uri);
            if (sheetId == null)
                continue;

            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
            var record = await Tcontact.SingleAsync(uri, cancellationToken, command);
            if (record != null)
            {
                yield return record;
                continue;
            }

            record = new Tcontact(sheetId.Value, ContactType.Studio, uri, displayName);
            var result = await record.InsertAync(cancellationToken, command);
            if (!result.IsSuccess)
                continue;

            yield return record;
        }
    }

    /// <summary>
    /// Retourne l'état de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static DiffusionStateKind GetDiffusionState(HtmlNode htmlNode)
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
    private static async Task<Tseason?> GetSeason(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var year = GetYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var seasonNumber = GetSeasonNumber(htmlNode);
        if (seasonNumber == null)
            return null;

        var intSeason = DateHelpers.GetIntSeason(seasonNumber.Value, year.Value);
        if (intSeason == 0)
            return null;

        var record = await Tseason.SingleAsync(intSeason, cancellationToken, cmd);
        if (record != null)
            return record;

        var seasonLiteral = DateHelpers.GetSeasonLiteral((FourSeasonsKind)seasonNumber.Value, year.Value);
        if (seasonLiteral == null)
            return null;

        record = new Tseason
        {
            SeasonNumber = seasonNumber.Value,
            DisplayName = seasonLiteral,
        };

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var result = await record.InsertAsync(cancellationToken, command);
        return result.IsSuccess ? record : null;
    }

    /// <summary>
    /// Retourne le numéro de la saison de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static byte? GetSeasonNumber(HtmlNode htmlNode)
    {
        var year = GetYearDiffusion(htmlNode);
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
    private static string? GetBeginDate(HtmlNode htmlNode)
    {
        var year = GetYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var month = GetMonthDiffusion(htmlNode);
        if (month == null)
            return $"{year.Value}-00-00";

        return $"{year.Value}-{month.Value:00}-00";
    }

    /// <summary>
    /// Retourne l'année de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static ushort? GetYearDiffusion(HtmlNode htmlNode)
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
    private static byte? GetMonthDiffusion(HtmlNode htmlNode)
    {
        var monthNode = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Mois de début de diffusion :')]/following-sibling::text()[1]");
        if (monthNode == null || monthNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var monthText = monthNode.InnerText.Trim();

        return DateHelpers.GetMonthNumber(monthText);
    }


    internal static bool GetIsAdultContent(Uri animeSheetUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(animeSheetUri.ToString());
        return GetIsAdultContent(htmlDocument.DocumentNode);
    }

    internal static bool GetIsAdultContent(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[@id='divFicheHentai']");
        return node != null;
    }

    internal static bool GetIsExplicitContent(Uri animeSheetUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(animeSheetUri.ToString());
        return GetIsExplicitContent(htmlDocument.DocumentNode);
    }

    internal static bool GetIsExplicitContent(HtmlNode htmlNode)
    {
        if (GetIsAdultContent(htmlNode))
            return true;
        var node = htmlNode.SelectSingleNode("//div[@id='divFicheError']");
        return node != null;
    }

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

        return GetFullThumbnail(htmlDocument.DocumentNode);
    }

    public static string? GetFullThumbnail(HtmlNode htmlNode)
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
        var uri = Main.GetImageFromSrc(IcotakuSection.Anime, src);
        return uri == null ? null : uri.ToString();
    }

    #endregion
}