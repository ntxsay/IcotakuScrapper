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

        var hostname = IcotakuWebHelpers.GetHostName(IcotakuSection.Anime);
        if (hostname == null || !uri.Host.Contains(hostname, StringComparison.OrdinalIgnoreCase))
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

            var mainName = ScrapMainName(htmlDocument.DocumentNode);
            if (mainName == null || mainName.IsStringNullOrEmptyOrWhiteSpace())
                throw new Exception("Le nom de l'anime n'a pas été trouvé");

            var sheetId = IcotakuWebHelpers.GetSheetId(uri) ?? throw new Exception("L'id de la fiche anime n'a pas été trouvé.");
            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

            var anime = new Tanime()
            {
                Name = mainName,
                SheetId = sheetId,
                Url = uri.ToString(),
                IsAdultContent = ScrapIsAdultContent(htmlDocument.DocumentNode),
                IsExplicitContent = ScrapIsExplicitContent(htmlDocument.DocumentNode),
                Note = ScrapNote(htmlDocument.DocumentNode),
                VoteCount = ScrapVoteCount(htmlDocument.DocumentNode),
                DiffusionState = ScrapDiffusionState(htmlDocument.DocumentNode),
                EpisodesCount = ScrapTotalEpisodes(htmlDocument.DocumentNode),
                Duration = ScrapDuration(htmlDocument.DocumentNode),
                Target = await ScrapTargetAsync(htmlDocument.DocumentNode, cancellationToken, command),
                OrigineAdaptation = await ScrapOrigineAdaptationAsync(htmlDocument.DocumentNode, cancellationToken, command),
                Format = await ScrapFormatAsync(htmlDocument.DocumentNode, cancellationToken, command),
                Season = await ScrapSeason(htmlDocument.DocumentNode, cancellationToken, command),
                ReleaseDate = ScrapBeginDate(htmlDocument.DocumentNode),
                Description = ScrapDescription(htmlDocument.DocumentNode),
                ThumbnailUrl = SCrapFullThumbnail(htmlDocument.DocumentNode),
            };

            //Titres alternatifs
            var alternativeNames = ScrapAlternativeTitles(htmlDocument.DocumentNode).ToArray();
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
            var websites = ScrapWebsites(htmlDocument.DocumentNode).ToArray();
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
            var studios = await ScrapStudioAsync(htmlDocument.DocumentNode, cancellationToken, command).ToArrayAsync();
            if (studios.Length > 0)
            {
                foreach (var studio in studios)
                {
                    if (!anime.Studios.Any(a => string.Equals(a.DisplayName, studio.DisplayName, StringComparison.OrdinalIgnoreCase) && string.Equals(a.Url, studio.Url, StringComparison.OrdinalIgnoreCase)))
                        anime.Studios.Add(studio);
                }
            }

            //Licenses
            var licenses = await ScrapLicensesAsync(htmlDocument.DocumentNode, cancellationToken, command).ToArrayAsync();
            if (licenses.Length > 0)
            {
                foreach (var license in licenses)
                {
                    if (!anime.Licenses.Any(a => string.Equals(a.Distributor.Url, license.Distributor.Url, StringComparison.OrdinalIgnoreCase)))
                        anime.Licenses.Add(license);

                }
            }

            //Episodes
            var episodes = TanimeEpisode.GetAnimeEpisode(anime.SheetId).ToArray();
            if (episodes.Length > 0)
            {
                foreach (var episode in episodes)
                    anime.Episodes.Add(episode);

                anime.ReleaseDate = episodes.Min(m => m.ReleaseDate).ToString("yyyy-MM-dd");
            }

            return new OperationState<Tanime?>
            {
                IsSuccess = true,
                Data = anime,
            };
        }
        catch (Exception ex)
        {
            LogServices.LogDebug(ex);
            return new OperationState<Tanime?>(false, ex.Message);
        }
    }

    #region General
    /// <summary>
    /// Obtient le nom principal de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static string? ScrapMainName(HtmlNode htmlNode)
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
    private static string? ScrapDescription(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Histoire')]/parent::div/p");
        return HttpUtility.HtmlDecode(node?.InnerText?.Trim());
    }

    /// <summary>
    /// Retourne les titres alternatifs de l'animé
    /// </summary>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    private static IEnumerable<TanimeAlternativeTitle> ScrapAlternativeTitles(HtmlNode documentNode)
    {
        var nodes = documentNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Titre ')]")?.ToArray();

        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var description = HttpUtility.HtmlDecode(node.InnerText.Trim()?.TrimEnd(':')?.Trim());
            var title = HttpUtility.HtmlDecode(node.NextSibling.InnerText.Trim());
            if (title == null || title.IsStringNullOrEmptyOrWhiteSpace())
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
    private static IEnumerable<TanimeWebSite> ScrapWebsites(HtmlNode htmlNode)
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

    private static double? ScrapNote(HtmlNode documentNode)
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

    private static uint ScrapVoteCount(HtmlNode documentNode)
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
    private static ushort ScrapTotalEpisodes(HtmlNode htmlNode)
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
    private static TimeSpan ScrapDuration(HtmlNode htmlNode)
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
    private static async Task<TorigineAdaptation?> ScrapOrigineAdaptationAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
    private static async Task<Tformat?> ScrapFormatAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
    #endregion

    #region Diffusion
    /// <summary>
    /// Retourne l'état de diffusion de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <returns></returns>
    private static DiffusionStateKind ScrapDiffusionState(HtmlNode htmlNode)
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
    private static async Task<Tseason?> ScrapSeason(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
    private static byte? ScrapSeasonNumber(HtmlNode htmlNode)
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
    private static string? ScrapBeginDate(HtmlNode htmlNode)
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
    private static ushort? ScrapYearDiffusion(HtmlNode htmlNode)
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
    private static byte? ScrapMonthDiffusion(HtmlNode htmlNode)
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
    private static async Task<Ttarget?> ScrapTargetAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

    #region Studios
    /// <summary>
    /// Retourne les studios d'aimation de l'animé
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<Tcontact> ScrapStudioAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var _node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Studio')]/parent::div");
        if (_node == null)
            yield break;

        var nodes = _node.SelectNodes("./a")?.ToArray();
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
            href = IcotakuWebHelpers.GetBaseUrl(IcotakuSection.Anime) + href;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                continue;

            //Récupère l'id de la fiche du studio
            var sheetId = IcotakuWebHelpers.GetSheetId(uri);
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
    #endregion

    #region Licenses
    private static async IAsyncEnumerable<TanimeLicense> ScrapLicensesAsync(HtmlNode documentNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var nodes = documentNode.SelectNodes("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Licence')]");
        if (nodes == null || nodes.Count == 0)
            yield break;

        foreach (var node in nodes)
        {
            var licenseTypeText = node.InnerText?.Trim();
            if (licenseTypeText == null || licenseTypeText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            licenseTypeText = licenseTypeText.Replace("Licence", string.Empty).Trim();
            licenseTypeText = licenseTypeText.Replace(":", string.Empty).Trim();

            if (licenseTypeText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var distributor_A_Nodes = node.ParentNode?.SelectNodes(".//a[contains(@href, '/editeur/')]")?.ToArray();
            if (distributor_A_Nodes == null || distributor_A_Nodes.Length == 0)
                continue;

            var distributors = await ScrapEditorsAsync(distributor_A_Nodes, cancellationToken, cmd).ToArrayAsync();
            if (distributors.Length == 0)
                continue;

            TlicenseType? licenseType = new()
            {
                Name = licenseTypeText,
                Section = IcotakuSection.Anime,
            };

            licenseType = await TlicenseType.SingleOrCreateAsync(licenseType, true, cancellationToken, cmd);
            if (licenseType == null)
                continue;

            foreach (var distributor in distributors)
            {
                yield return new TanimeLicense()
                {
                    Type = licenseType,
                    Distributor = distributor,
                };
            }
        }
    }

    private static async IAsyncEnumerable<Tcontact> ScrapEditorsAsync(HtmlNode[] distributorNodes, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        foreach (var node in distributorNodes)
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
            href = IcotakuWebHelpers.GetBaseUrl(IcotakuSection.Anime) + href;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                continue;

            //Récupère l'id de la fiche du studio
            var sheetId = IcotakuWebHelpers.GetSheetId(uri);
            if (sheetId == null)
                continue;

            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
            var record = await Tcontact.SingleAsync(uri, cancellationToken, command);
            if (record != null)
            {
                yield return record;
                continue;
            }

            record = new Tcontact(sheetId.Value, ContactType.Distributor, uri, displayName);
            var result = await record.InsertAync(cancellationToken, command);
            if (!result.IsSuccess)
                continue;

            yield return record;
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

        return SCrapFullThumbnail(htmlDocument.DocumentNode);
    }

    public static string? SCrapFullThumbnail(HtmlNode htmlNode)
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

    [GeneratedRegex(@"\d+(\.\d+)?(?=/10)")]
    private static partial Regex GetNoteRegex();

    [GeneratedRegex(@"(\d+)")]
    private static partial Regex GetVoteCountRegex();

    #endregion
}