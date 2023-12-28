using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    [GeneratedRegex(@"\d+(\.\d+)?(?=/10)")]
    protected static partial Regex GetNoteRegex();

    [GeneratedRegex(@"(\d+)")]
    protected static partial Regex GetVoteCountRegex();

    public static async Task<TanimeBase[]> FindAsync(AnimeFinderParameterStruct finderParameter, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await ScrapAnimeSearchAsync(finderParameter, cancellationToken, cmd).ToArrayAsync();

    protected static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(string htmlContent, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(htmlContent);

        return await ScrapAnimeBaseAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken, cmd);
    }

    internal static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);

        return await ScrapAnimeBaseAsync(htmlDocument.DocumentNode, sheetUri, options, cancellationToken, cmd);
    }


    /// <summary>
    /// Méthode permettant de récupérer les informations de base d'un animé
    /// </summary>
    /// <param name="documentNode"></param>
    /// <param name="sheetUri"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    protected static async Task<OperationState<TanimeBase?>> ScrapAnimeBaseAsync(HtmlNode documentNode, Uri sheetUri, AnimeScrapingOptions options, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

            var animeBase = new TanimeBase()
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
                Description = ScrapDescription(ref documentNode),
                Remark = ScrapRemark(ref documentNode),
                ThumbnailUrl = ScrapFullThumbnail(documentNode),
            };

            //Titres alternatifs
            var alternativeNames = ScrapAlternativeTitles(documentNode).ToArray();
            if (alternativeNames.Length > 0)
            {
                foreach (var title in alternativeNames)
                {
                    if (!animeBase.AlternativeTitles.Any(a => string.Equals(a.Title, title.Title, StringComparison.OrdinalIgnoreCase)))
                    {
                        animeBase.AlternativeTitles.Add(title);
                    }
                }
            }

            //Websites
            var websites = ScrapWebsites(documentNode).ToArray();
            if (websites.Length > 0)
            {
                foreach (var url in websites)
                {
                    if (!animeBase.Websites.Any(a => string.Equals(a.Url, url.Url, StringComparison.OrdinalIgnoreCase)))
                    {
                        animeBase.Websites.Add(url);
                    }
                }
            }

            //Genres et themes
            if (options.HasFlag(AnimeScrapingOptions.Categories) ||
                options.HasFlag(AnimeScrapingOptions.FullCategories))
            {
                var genres = await GetCategoriesAsync(documentNode, CategoryType.Genre, options.HasFlag(AnimeScrapingOptions.FullCategories), cancellationToken, command).ToArrayAsync();
                var themes = await GetCategoriesAsync(documentNode, CategoryType.Theme, options.HasFlag(AnimeScrapingOptions.FullCategories), cancellationToken, command).ToArrayAsync();
                List<Tcategory> categories = [.. genres, .. themes];
                if (categories.Count > 0)
                {
                    foreach (var category in categories)
                    {
                        if (!animeBase.Categories.Any(a => string.Equals(a.Name, category.Name, StringComparison.OrdinalIgnoreCase) && a.Type == category.Type))
                            animeBase.Categories.Add(category);
                    }
                }
            }

            //Studios
            if (options.HasFlag(AnimeScrapingOptions.Studios) || options.HasFlag(AnimeScrapingOptions.FullStudios))
            {
                var studios = await ScrapStudioAsync(documentNode, options.HasFlag(AnimeScrapingOptions.FullStudios), cancellationToken, cmd).ToArrayAsync();
                if (studios.Length > 0)
                {
                    foreach (var studio in studios)
                    {
                        if (!animeBase.Studios.Any(a => string.Equals(a.DisplayName, studio.DisplayName, StringComparison.OrdinalIgnoreCase) && string.Equals(a.Url, studio.Url, StringComparison.OrdinalIgnoreCase)))
                            animeBase.Studios.Add(studio);
                    }
                }
            }

            //Licenses
            if (options.HasFlag(AnimeScrapingOptions.Licenses) || options.HasFlag(AnimeScrapingOptions.FullLicenses))
            {
                //Licenses
                var licenses = await ScrapLicensesAsync(documentNode, options.HasFlag(AnimeScrapingOptions.FullLicenses), cancellationToken, cmd).ToArrayAsync();
                if (licenses.Length > 0)
                {
                    foreach (var license in licenses)
                    {
                        if (!animeBase.Licenses.Any(a => string.Equals(a.Distributor.Url, license.Distributor.Url, StringComparison.OrdinalIgnoreCase)))
                            animeBase.Licenses.Add(license);
                    }
                }
            }

            //Staff
            if (options.HasFlag(AnimeScrapingOptions.Staff) || options.HasFlag(AnimeScrapingOptions.FullStaff))
            {
                var staff = await ScrapStaffAsync(documentNode, options.HasFlag(AnimeScrapingOptions.FullStaff), cancellationToken, cmd).ToArrayAsync();
                if (staff.Length > 0)
                {
                    foreach (var tanimeStaff in staff)
                    {
                        if (!animeBase.Staffs.Any(a => a.Role.Id == tanimeStaff.Role.Id && a.Person.Id == tanimeStaff.Person.Id))
                            animeBase.Staffs.Add(tanimeStaff);
                    }
                }
            }



            return new OperationState<TanimeBase?>
            {
                IsSuccess = true,
                Data = animeBase,
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
    /// <param name="documentNode">Noeud à partir duquel commencer la recherche</param>
    /// <returns></returns>
    protected static string? ScrapDescription(ref HtmlNode documentNode)
    {
        var node = documentNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Histoire')]/following-sibling::p");
        return HttpUtility.HtmlDecode(node?.InnerText?.Trim());
    }

    protected static string? ScrapRemark(ref HtmlNode documentNode)
    {
        var node = documentNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Remarque')]/following-sibling::p");
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
        if (text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        //Remplace le point par une virgule pour pouvoir convertir en double
        text = text.Replace('.', ',').Trim();
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
        if (seasonNumber == null || seasonNumber is < 1 or > 4)
            return null;

        WeatherSeason season = new((WeatherSeasonKind)seasonNumber, year.Value);

        var record = new Tseason
        {
            SeasonNumber = season.ToIntSeason(),
            DisplayName = season.ToString(),
        };

        return await Tseason.SingleOrCreateAsync(record, false, cancellationToken, cmd);
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
    /// <param name="scrapFullCategory"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<Tcategory> GetCategoriesAsync(HtmlNode htmlNode, CategoryType categoryType, bool scrapFullCategory, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

            //Récupère l'id de la fiche du thème ou du genre s'il existe en base de données
            var category = await Tcategory.SingleAsync(uri, cancellationToken, cmd);
            if (category != null)
            {
                yield return category;
                continue;
            }

            //Si on ne scrappe pas la fiche du thème ou du genre depuis sa fiche via son url, on insère la catégorie dans la base de données depuis la fiche anime
            if (!scrapFullCategory)
            {
                var sheetId = IcotakuWebHelpers.GetSheetId(uri);
                if (sheetId < 0)
                    continue;

                category = new Tcategory()
                {
                    Name = name.Trim(),
                    Section = IcotakuSection.Anime,
                    Url = uri.ToString(),
                    SheetId = sheetId,
                    IsFullyScraped = false,
                };

                //Et insère la fiche dans la base de données
                var insertionState2 = await category.InsertAsync(false, cancellationToken, cmd);
                if (insertionState2.IsSuccess)
                {
                    yield return category;
                }

                continue;
            }

            //Sinon scrappe la fiche du thème ou du genre depuis sa fiche elle-même
            category = Tcategory.ScrapCategoryFromSheetPage(uri, IcotakuSection.Anime, categoryType);
            if (category == null)
                continue;

            //Et insère la fiche dans la base de données
            var result = await category.InsertAsync(false, cancellationToken, cmd);
            if (!result.IsSuccess)
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

    #region Studios
    /// <summary>
    /// Retourne les studios d'aimation de l'animé
    /// </summary>
    /// <param name="documentNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<TcontactBase> ScrapStudioAsync(HtmlNode documentNode, bool scrapFull, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlNode = documentNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Studio')]/parent::div");

        var nodes = htmlNode?.SelectNodes("./a")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var contact = await ScrapContactBase(node, scrapFull, cancellationToken, cmd);
            if (contact == null)
                continue;
            yield return contact;
        }
    }
    #endregion

    #region Licenses
    protected static async IAsyncEnumerable<TanimeLicense> ScrapLicensesAsync(HtmlNode documentNode, bool scrapFull, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

            var distributorANodes = node.ParentNode?.SelectNodes(".//a[contains(@href, '/editeur/')]")?.ToArray();
            if (distributorANodes == null || distributorANodes.Length == 0)
                continue;

            var distributors = await ScrapEditorsAsync(distributorANodes, scrapFull, cancellationToken, cmd).ToArrayAsync();
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
                yield return new TanimeLicense
                {
                    Type = licenseType,
                    Distributor = distributor,
                };
            }
        }
    }

    protected static async IAsyncEnumerable<Tcontact> ScrapEditorsAsync(HtmlNode[] distributorNodes, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        foreach (var node in distributorNodes)
        {
            //Récupère le nom d'affichage du studio
            var displayName = HttpUtility.HtmlDecode(node.InnerText?.Trim());
            if (displayName == null || displayName.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Récupère l'url de la fiche du studio
            var href = node.Attributes["href"]?.Value;
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Créer l'url de la fiche du studio
            href = IcotakuWebHelpers.GetBaseUrl(IcotakuSection.Anime) + href;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                continue;

            //Récupère l'id de la fiche du studio
            var sheetId = IcotakuWebHelpers.GetSheetId(uri);
            if (sheetId < 0)
                continue;

            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
            var record = await Tcontact.SingleAsync(uri, cancellationToken, command);
            if (record != null)
            {
                yield return record;
                continue;
            }

            record = new Tcontact(sheetId, ContactType.Distributor, uri, displayName);
            var result = await record.InsertAync(cancellationToken, command);
            if (!result.IsSuccess)
                continue;

            yield return record;
        }
    }

    protected static async IAsyncEnumerable<TcontactBase> ScrapEditorsAsync(HtmlNode[] distributorNodes, bool scrapFull, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        foreach (var node in distributorNodes)
        {
            var contact = await ScrapContactBase(node, scrapFull, cancellationToken, cmd);
            if (contact == null)
                continue;

            yield return contact;
        }
    }

    #endregion

    #region Staff

    protected static async IAsyncEnumerable<TanimeStaff> ScrapStaffAsync(HtmlNode documentNode, bool scrapFullStaff,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlNodes = documentNode.SelectNodes(
            "//table[contains(@class, 'staff')]//tr")?.ToArray();
        if (htmlNodes == null || htmlNodes.Length == 0)
            yield break;

        foreach (var node in htmlNodes)
        {
            var linkNode = node.SelectSingleNode("./td[1]/a");
            if (linkNode == null)
                continue;

            var contact = await ScrapContactBase(linkNode, scrapFullStaff, cancellationToken, cmd);
            if (contact == null)
                continue;

            //Récupère le rôle du contact
            var roleNode = node.SelectSingleNode("./td[2]/text()");

            var roleText = HttpUtility.HtmlDecode(roleNode?.InnerText?.Trim());
            if (roleText == null || roleText.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            ToeuvreRole? role = new()
            {
                Name = roleText,
                Type = RoleType.Staff,
            };

            role = await ToeuvreRole.SingleOrCreateAsync(role, true, cancellationToken, cmd);
            if (role == null)
                continue;

            yield return new TanimeStaff
            {
                Person = contact,
                Role = role
            };

        }
    }

    private static async Task<TcontactBase?> ScrapContactBase(HtmlNode linkNode, bool scrapFull = false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (linkNode is null)
            return null;

        var contactHref = linkNode.Attributes["href"]?.Value;
        if (contactHref == null || contactHref.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var contactUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(linkNode, IcotakuSection.Anime);
        if (contactUri == null)
            return null;

        var displayName = HttpUtility.HtmlDecode(linkNode.InnerText?.Trim());
        if (displayName == null || displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        //Récupère l'id de la fiche du thème ou du genre s'il existe en base de données
        Tcontact? contact = await Tcontact.SingleAsync(contactUri, cancellationToken, cmd);
        if (contact != null)
            return contact;

        //Si on ne scrappe pas la fiche du thème ou du genre depuis sa fiche via son url, on insère la catégorie dans la base de données depuis la fiche anime
        if (!scrapFull)
        {
            var sheetId = IcotakuWebHelpers.GetSheetId(contactUri);
            if (sheetId < 0)
                return null;

            var contactType = IcotakuWebHelpers.GetContactType(contactUri);
            if (contactType == null)
                return null;

            contact = new Tcontact()
            {
                SheetId = sheetId,
                Type = (ContactType)contactType,
                DisplayName = displayName,
                Url = contactUri.ToString(),
                ThumbnailUrl = Tcontact.ScrapFullThumbnail(contactUri),
                Presentation = null,
            };

            //Et insère la fiche dans la base de données
            var insertionState2 = await contact.InsertAync(cancellationToken, cmd);
            if (insertionState2.IsSuccess)
            {
                return contact;
            }
        }

        contact = await Tcontact.ScrapFromUriAsync(contactUri);
        if (contact == null)
            return null;

        contact = await Tcontact.SingleOrCreateAsync(contact, false, cancellationToken, cmd);
        if (contact == null)
            return null;

        return contact;
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



    protected static async IAsyncEnumerable<TanimeBase> ScrapSearchResult(HtmlNode documentNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

            var animeBaseResult = await ScrapAnimeBaseAsync(uri, AnimeScrapingOptions.All, cancellationToken, cmd);
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