using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using IcotakuScrapper.Contact;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    private static async Task<Tanime?> GetAnimeAsync(Uri uri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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
                SheetId =  sheetId,
                Url = uri.ToString(),
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
            
            return anime;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

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
    /// <param name="htmlNode">Correspond au DocumentNode</param>
    /// <returns></returns>
    private static string? GetDescription(HtmlNode htmlNode)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'informations')]/h2[contains(text(), 'Histoire')]/parent::div/p");
        return HttpUtility.HtmlDecode(node?.InnerText?.Trim());
    }

    private static TanimeAlternativeTitle[] GetAlternativeTitles(HtmlNode htmlNode)
    {
        var nodes = htmlNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Titre ')]").ToArray();

        if (nodes.Length == 0)
            return Array.Empty<TanimeAlternativeTitle>();

        return nodes.Select(s => new TanimeAlternativeTitle()
        {
            Title = HttpUtility.HtmlDecode(s.NextSibling.InnerText.Trim()),
            Description = HttpUtility.HtmlDecode(s.InnerText.Trim()),
        }).Where(w => !w.Title.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
    }

    private static IEnumerable<TanimeWebSite> GetWebsites(HtmlNode htmlNode)
    {
        var nodes = htmlNode.SelectNodes("//div[contains(@class, 'info_fiche')]/div/b[starts-with(text(), 'Site ')]").ToArray();
        if (nodes.Length == 0)
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
                        Description = description,
                    };
            }         
        }
    }

    private static async IAsyncEnumerable<Tcategory> GetCategoriesAsync(HtmlNode htmlNode, CategoryType categoryType, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlNode[] nodes = categoryType switch
        {
            CategoryType.Genre => htmlNode.SelectNodes("//span[@id='id_genre']/a[contains(@href, '/genre/')]").ToArray(),
            CategoryType.Theme => htmlNode.SelectNodes("//span[@id='id_theme']/a[contains(@href, '/theme/')]").ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, "Le type de catégorie est invalide.")
        };

        if (nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            var name = HttpUtility.HtmlDecode(node.InnerText?.Trim())?.Trim();
            if (name == null || name.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Récupère l'url de la fiche du thème ou du genre
            var href = node?.Attributes["href"]?.Value;
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            //Créer l'url de la fiche du thème ou du genre
            href = Main.GetBaseUrl(IcotakuSection.Anime) + href;
            if (!Uri.TryCreate(href, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                continue;

            //Récupère l'id de la fiche du thème ou du genre
            var sheetId = Main.GetSheetId(uri);
            if (sheetId == null)
                continue;

            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
            command.Parameters.Clear();
            var record = await Tcategory.SingleAsync(name, IcotakuSection.Anime, categoryType, cancellationToken, command);
            if (record != null)
            {
                yield return record;
                continue;
            }

            record = new Tcategory(IcotakuSection.Anime, categoryType, sheetId.Value, uri, name, null);
            var result = await record.InsertAsync(cancellationToken, command);
            if (result.IsSuccess)
            {
                yield return record;
            }
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

    private static async Task<TorigineAdaptation?> GetOrigineAdaptationAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Origine :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var record = await TorigineAdaptation.SingleAsync(text, cancellationToken, command);
        if (record != null)
            return record;

        record = new TorigineAdaptation(text, null);
        var result = await record.InsertAsync(cancellationToken, command);
        return result.IsSuccess ? record : null;
    }

    private static async Task<Tformat?> GetFormatAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Catégorie :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var record = await Tformat.SingleAsync(text, cancellationToken, command);
        if (record != null)
            return record;

        record = new Tformat(text, null);
        var result = await record.InsertAsync(cancellationToken, command);
        return result.IsSuccess ? record : null;
    }

    private static async Task<Ttarget?> GetTargetAsync(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var node = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Public visé :')]/following-sibling::text()[1]");
        var text = node?.InnerText?.Trim();
        if (text == null || text.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var record = await Ttarget.SingleAsync(text, cancellationToken, command);
        if (record != null)
            return record;

        record = new Ttarget(text, null);
        var result = await record.InsertAsync(cancellationToken, command);
        return result.IsSuccess ? record : null;
    }

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

    private static async Task<Tseason?> GetSeason(HtmlNode htmlNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var year = GetYearDiffusion(htmlNode);
        if (year == null)
            return null;

        var seasonNumber = GetSeasonNumber(htmlNode);
        if (seasonNumber == null)
            return null;

        var record = await Tseason.SingleAsync(year.Value, seasonNumber.Value, cancellationToken, cmd);
        if (record != null)
            return record;

        record = new Tseason
        {
            Year = year.Value,
            SeasonNumber = seasonNumber.Value,
            DisplayName = $"{DateHelpers.GetSeasonName(seasonNumber.Value)} {year.Value}"
        };

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var result = await record.InsertAsync(cancellationToken, command);
        return result.IsSuccess ? record : null;
    }

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

    private static ushort? GetYearDiffusion(HtmlNode htmlNode)
    {
        var yearNode = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Année de diffusion :')]/following-sibling::text()[1]");
        if (yearNode == null || yearNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        var yearText = yearNode.InnerText.Trim();
        return ushort.TryParse(yearText, out ushort year) ? year : null;
    }

    private static byte? GetMonthDiffusion(HtmlNode htmlNode)
    {
        var monthNode = htmlNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Mois de début de diffusion :')]/following-sibling::text()[1]");
        if (monthNode == null || monthNode.InnerText.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var monthText = monthNode.InnerText.Trim();

        return DateHelpers.GetMonthNumber(monthText);
    }
}