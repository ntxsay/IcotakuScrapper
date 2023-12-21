using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class Tanime
{
    /// <summary>
    /// Récupère les informations de l'anime via l'id Icotaku de la fiche
    /// </summary>
    /// <param name="sheetId">Id Icotaku de la fiche</param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromSheetIdAsync(int sheetId,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var index = await TsheetIndex.SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken, cmd);
        if (index == null)
            return new OperationState<int>(false, "L'index permettant de récupérer l'url de la fiche de l'anime n'a pas été trouvé dans la base de données.");

        if (!Uri.TryCreate(index.Url, UriKind.Absolute, out var sheetUri) || !sheetUri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime est invalide.");

        return await ScrapFromUrlAsync(sheetUri, cancellationToken, cmd);
    }

    /// <summary>
    /// Récupère les informations de l'anime via l'url de la fiche
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="userName"></param>
    /// <param name="passWord"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, string userName, string passWord,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlContent = await IcotakuWebHelpers.GetRestrictedHtmlAsync(IcotakuSection.Anime, sheetUri, userName, passWord);
        if (htmlContent == null || htmlContent.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le contenu de la fiche est introuvable.");

        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var animeResult = await ScrapAnimeAsync(htmlContent, sheetUri, cancellationToken, command);

        return await ScrapAnimeFromUrlAsync(animeResult, cancellationToken, command);
    }

    public static async Task<OperationState<int>> ScrapFromUrlAsync(Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!IcotakuWebHelpers.IsHostNameValid(IcotakuSection.Anime, sheetUri))
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas une url icotaku.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var animeResult = await ScrapAnimeAsync(sheetUri, cancellationToken, command);

        return await ScrapAnimeFromUrlAsync(animeResult, cancellationToken, command);
    }

    #region Prepration scrapping

    /// <summary>
    /// Récupère l'anime depuis l'url de la fiche icotaku.
    /// </summary>
    /// <param name="animeResult"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async Task<OperationState<int>> ScrapAnimeFromUrlAsync(OperationState<Tanime?> animeResult, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!animeResult.IsSuccess)
            return new OperationState<int>(false, animeResult.Message);

        var anime = animeResult.Data;
        if (anime == null)
            return new OperationState<int>(false, "Une erreur est survenue lors de la récupération de l'anime");

        if (anime.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime est introuvable.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        _ = await CreateIndexAsync(anime.Name, anime.Url, anime.SheetId, cancellationToken, command);

        return await anime.AddOrUpdateAsync(cancellationToken, command);
    }

    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(string htmlContent, Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(htmlContent);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, cancellationToken, cmd);
    }

    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);

        return await ScrapAnimeAsync(htmlDocument.DocumentNode, sheetUri, cancellationToken, cmd);
    }

    /// <summary>
    /// Récupère les informations de la fiche anime à partir de son url
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="documentNode"></param>
    /// <returns></returns>
    private static async Task<OperationState<Tanime?>> ScrapAnimeAsync(HtmlNode documentNode, Uri sheetUri, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        try
        {
            var isAdultContent = ScrapIsAdultContent(documentNode);
            if (Main.IsAccessingToAdultContent == false && isAdultContent)
                return new OperationState<Tanime?>(false, "L'anime est considéré comme étant un contenu adulte (Hentai, Yuri, Yaoi).");

            var isExplicitContent = ScrapIsExplicitContent(documentNode);
            if (Main.IsAccessingToExplicitContent == false && isExplicitContent)
                return new OperationState<Tanime?>(false, "L'anime est considéré comme étant un contenu explicite (Violence ou nudité explicite).");

            var mainName = ScrapMainName(documentNode);
            if (mainName == null || mainName.IsStringNullOrEmptyOrWhiteSpace())
                throw new Exception("Le nom de l'anime n'a pas été trouvé");

            var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

            var anime = new Tanime
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

            //Titres alternatifs
            var alternativeNames = ScrapAlternativeTitles(documentNode).ToArray();
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
            var websites = ScrapWebsites(documentNode).ToArray();
            if (websites.Length > 0)
            {
                foreach (var url in websites)
                {
                    if (!anime.Websites.Any(a => string.Equals(a.Url, url.Url, StringComparison.OrdinalIgnoreCase)))
                    {
                        anime.Websites.Add(url);
                    }
                }
            }

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

            //Studios
            var studios = await ScrapStudioAsync(documentNode, cancellationToken, command).ToArrayAsync();
            if (studios.Length > 0)
            {
                foreach (var studio in studios)
                {
                    if (!anime.Studios.Any(a => string.Equals(a.DisplayName, studio.DisplayName, StringComparison.OrdinalIgnoreCase) && string.Equals(a.Url, studio.Url, StringComparison.OrdinalIgnoreCase)))
                        anime.Studios.Add(studio);
                }
            }

            //Licenses
            var licenses = await ScrapLicensesAsync(documentNode, cancellationToken, command).ToArrayAsync();
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
                if (anime.EpisodesCount == episodes.Length)
                {
                    var endDate = episodes.Max(m => m.ReleaseDate);
                    anime.EndDate = endDate.ToString("yyyy-MM-dd");
                }
            }
            
            //Staff
            var staff = await ScrapStaffAsync(documentNode, cancellationToken, command).ToArrayAsync();
            if (staff.Length > 0)
            {
                foreach (var tanimeStaff in staff)
                {
                    if (!anime.Staffs.Any(a => a.Role.Id == tanimeStaff.Role.Id && a.Person.Id == tanimeStaff.Person.Id))
                        anime.Staffs.Add(tanimeStaff);
                }
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

    #endregion

   
    #region Studios
    /// <summary>
    /// Retourne les studios d'aimation de l'animé
    /// </summary>
    /// <param name="documentNode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    private static async IAsyncEnumerable<Tcontact> ScrapStudioAsync(HtmlNode documentNode, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlNode = documentNode.SelectSingleNode("//div[contains(@class, 'info_fiche')]//b[starts-with(text(), 'Studio')]/parent::div");
        if (htmlNode == null)
            yield break;

        var nodes = htmlNode.SelectNodes("./a")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            yield break;

        foreach (var node in nodes)
        {
            //Récupère le nom d'affichage du studio
            var displayName = HttpUtility.HtmlDecode(node?.InnerText?.Trim());
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
            if (sheetId < 0)
                continue;

            await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
            var record = await Tcontact.SingleAsync(uri, cancellationToken, command);
            if (record != null)
            {
                yield return record;
                continue;
            }

            record = new Tcontact(sheetId, ContactType.Studio, uri, displayName);
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

            var distributorANodes = node.ParentNode?.SelectNodes(".//a[contains(@href, '/editeur/')]")?.ToArray();
            if (distributorANodes == null || distributorANodes.Length == 0)
                continue;

            var distributors = await ScrapEditorsAsync(distributorANodes, cancellationToken, cmd).ToArrayAsync();
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

    private static async IAsyncEnumerable<Tcontact> ScrapEditorsAsync(HtmlNode[] distributorNodes, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
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

    #endregion

    #region Staff

    private static async IAsyncEnumerable<TanimeStaff> ScrapStaffAsync(HtmlNode documentNode,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        var htmlNodes = documentNode.SelectNodes(
            "//table[contains(@class, 'staff')]//tr")?.ToArray();
        if (htmlNodes == null || htmlNodes.Length == 0)
            yield break;

        foreach (var node in htmlNodes)
        {
            var contactNode = node.SelectSingleNode("./td[1]/a");

            var contactHref = contactNode?.Attributes["href"]?.Value;
            if (contactHref == null || contactHref.IsStringNullOrEmptyOrWhiteSpace())
                continue;
            
            var contactUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(contactNode!, IcotakuSection.Anime);
            if (contactUri == null)
                continue;

            var contact = await Tcontact.ScrapFromUriAsync(contactUri);
            if (contact == null)
                continue;
            
            contact = await Tcontact.SingleOrCreateAsync(contact, false, cancellationToken, cmd);
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


    #endregion
}