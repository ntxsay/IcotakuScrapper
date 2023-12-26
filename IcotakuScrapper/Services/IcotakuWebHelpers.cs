using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Services.IOS;
using System.Net;
using System.Net.Http.Headers;
using IcotakuScrapper.Anime;

namespace IcotakuScrapper.Services;

/// <summary>
/// Classe contenant des méthodes permettant de récupérer des informations à partir du site icotaku.com
/// </summary>
public static class IcotakuWebHelpers
{
    public const string IcotakuBaseUrl = "https://icotaku.com";
    public const string IcotakuAnimeUrl = "https://anime.icotaku.com";
    public const string IcotakuMangaUrl = "https://manga.icotaku.com";
    public const string IcotakuLightNovelUrl = "https://novel.icotaku.com";
    public const string IcotakuDramaUrl = "https://drama.icotaku.com";
    public const string IcotakuCommunityUrl = "https://communaute.icotaku.com";
    public const string IcotakuDownloadBaseUrl = "https://communaute.icotaku.com/uploads";


    public const string IcotakuBaseHostName = "icotaku.com";
    public const string IcotakuAnimeHostName = "anime.icotaku.com";
    public const string IcotakuMangaHostName = "manga.icotaku.com";
    public const string IcotakuLightNovelHostName = "novel.icotaku.com";
    public const string IcotakuDramaHostName = "drama.icotaku.com";
    public const string IcotakuCommunityHostName = "communaute.icotaku.com";


    /// <summary>
    /// Retourne la base url du site à partir de la section
    /// </summary>
    /// <param name="section">Correspond à la section du site permettant de sélectionner la bonne base Url</param>
    /// <returns></returns>
    public static string? GetBaseUrl(IcotakuSection section) => section switch
    {
        IcotakuSection.Anime => IcotakuAnimeUrl,
        IcotakuSection.Manga => IcotakuMangaUrl,
        IcotakuSection.LightNovel => IcotakuLightNovelUrl,
        IcotakuSection.Drama => IcotakuDramaUrl,
        IcotakuSection.Community => IcotakuCommunityUrl,
        _ => null
    };

    /// <summary>
    /// Retourne le nom de domaine du site à partir de la section
    /// </summary>
    /// <param name="section">Correspond à la section du site permettant de sélectionner le bon nom de domaine</param>
    /// <returns></returns>
    public static string? GetHostName(IcotakuSection section) => section switch
    {
        IcotakuSection.Anime => IcotakuAnimeHostName,
        IcotakuSection.Manga => IcotakuMangaHostName,
        IcotakuSection.LightNovel => IcotakuLightNovelHostName,
        IcotakuSection.Drama => IcotakuDramaHostName,
        IcotakuSection.Community => IcotakuCommunityHostName,
        _ => null
    };

    /// <summary>
    /// Retourne une valeur booléenne indiquant si le nom de domaine de l'url correspond à celui de la section
    /// </summary>
    /// <param name="section"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static bool IsHostNameValid(IcotakuSection section, Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            return false;

        var host = GetHostName(section);
        if (host == null)
            return false;

        return string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Contains(host, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retourne la section du site à partir de l'url de la fiche anime, manga, light novel, drama, editeur, etc...
    /// </summary>
    /// <param name="sheetUri">Url de la fiche</param>
    /// <returns></returns>
    public static IcotakuSection? GetIcotakuSection(Uri sheetUri)
    {
        var host = sheetUri.Host;
        if (host.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        return host switch
        {
            IcotakuAnimeHostName => IcotakuSection.Anime,
            IcotakuMangaHostName => IcotakuSection.Manga,
            IcotakuLightNovelHostName => IcotakuSection.LightNovel,
            IcotakuDramaHostName => IcotakuSection.Drama,
            IcotakuCommunityHostName => IcotakuSection.Community,
            _ => null
        };
    }

    /// <summary>
    /// Extrait l'id de la fiche à partir de l'url de la fiche anime, manga, light novel, drama, editeur, etc...
    /// </summary>
    /// <param name="sheetUri">Url de la fiche</param>
    /// <returns></returns>
    public static int GetSheetId(Uri sheetUri)
    {
        var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace())
            .ToArray();
        if (splitUrl.Length == 0)
            return -1;

        var sheetId = splitUrl.FirstOrDefault(f =>
            !f.Any(a => char.IsLetter(a) || a == '-' || a == '_'));

        if (sheetId.IsStringNullOrEmptyOrWhiteSpace())
            return -1;

        if (!int.TryParse(sheetId, out var sheetIdInt))
            return -1;

        return sheetIdInt;
    }
    
    /// <summary>
    /// Retourne le type de contact à partir de l'url de la fiche du contact (personne, personnage, studio, éditeur, etc...)
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <returns></returns>
    public static ContactType? GetContactType(Uri sheetUri)
    {
        var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace())
            .ToArray();
        if (splitUrl.Length == 0)
            return null;

        var contactType = splitUrl[0].ToLower() switch
        {
            "studio" => ContactType.Studio,
            "editeur" => ContactType.Distributor,
            "individu" => ContactType.Person,
            "personnage" => ContactType.Character,
            _ => ContactType.Unknown
        };
            
        return contactType != ContactType.Unknown 
            ? contactType 
            : null;
    }

    #region Get Url

    /// <summary>
    /// Obtient l'url de la page de la liste des animes
    /// </summary>
    /// <param name="contentSection"></param>
    /// <param name="sheetType"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string? GetIcotakuFilterUrl(IcotakuSection contentSection, IcotakuSheetType sheetType, uint page = 1)
    {
        return contentSection switch
        {
            IcotakuSection.Anime => sheetType switch
            {
                IcotakuSheetType.Unknown => null,
                IcotakuSheetType.Anime => $"https://anime.icotaku.com/animes.html?filter=all{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Person => $"https://anime.icotaku.com/individus.html?filter=all{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Character => $"https://anime.icotaku.com/personnages.html?filter=all{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Studio => null,
                IcotakuSheetType.Distributor => null,
                _ => throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, "Ce type de fiche n'est pas pris en charge.")
            },
            IcotakuSection.Manga => null,
            IcotakuSection.LightNovel => null,
            IcotakuSection.Drama => null,
            IcotakuSection.Community => null,
            _ => throw new ArgumentOutOfRangeException(nameof(contentSection), contentSection, "Cette section du site Icotaku n'est pas prise en charge.")
        };
    }

    public static Uri? GetAdvancedSearchUri(IcotakuSection section, AnimeFinderParameterStruct findParameter, uint page = 1)
    {
        
        var baseUrl = section switch
        {
            IcotakuSection.Anime => $"{IcotakuAnimeUrl}/recherche-avancee.html?",
            IcotakuSection.Manga => null,
            IcotakuSection.LightNovel => null,
            IcotakuSection.Drama => null,
            IcotakuSection.Community => null,
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, "Cette section du site Icotaku n'est pas prise en charge.")
        };
        
        if (baseUrl == null)
            return null;
        
        //Rechercher dans les titres
        if (findParameter.Title != null && !findParameter.Title.IsStringNullOrEmptyOrWhiteSpace())
            baseUrl += $"titre={findParameter.Title}";
        
        //Rechercher dans les catégories
        if (findParameter.Format != null && !findParameter.Format.IsStringNullOrEmptyOrWhiteSpace())
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&categorie={findParameter.Format}";
        }
        
        //Rechercher dans les publics
        if (findParameter.Target != null && !findParameter.Target.IsStringNullOrEmptyOrWhiteSpace())
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&public={findParameter.Target}";
        }
        
        //Rechercher dans les origines
        if (findParameter.OrigineAdaptation != null && !findParameter.OrigineAdaptation.IsStringNullOrEmptyOrWhiteSpace())
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&origine={findParameter.OrigineAdaptation}";
        }
        
        //Rechercher dans les studios
        if (findParameter.StudioId is > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&studio={findParameter.StudioId}";
        }
        
        //Rechercher dans les états de diffusion
        if (findParameter.DiffusionState != DiffusionStateKind.Unknown)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&diffusion={findParameter.DiffusionState.GetSearchPamareter()}";
        }
        
        //Rechercher dans les éditeurs
        if (findParameter.DistributorId is > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&editeurFr={findParameter.DistributorId}";
        }
        
        //Rechercher dans les années
        if (findParameter.Year > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&annee={findParameter.Year}";
        }
        
        //Rechercher dans les mois
        if (findParameter.MonthNumber is >= 1 and <= 12)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&mois={findParameter.MonthNumber.GetMonthSearchPamareter()}";
        }
        
        //Rechercher dans les saisons
        if (findParameter.Season != WeatherSeasonKind.Unknown)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&saison={findParameter.Season.GetSearchPamareter()}";
        }
        
        //Rechercher dans les genres (inclus)
        if (findParameter.IncludeGenresId.Length > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&genres_inclus[]={string.Join("&genres_inclus[]=", findParameter.IncludeGenresId)}";
        }
        
        //Rechercher dans les genres (exclus)
        if (findParameter.ExcludeGenresId.Length > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&genres_exclus[]={string.Join("&genres_exclus[]=", findParameter.ExcludeGenresId)}";
        }
        
        //Rechercher dans les thèmes (inclus)
        if (findParameter.IncludeThemesId.Length > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&themes_inclus[]={string.Join("&themes_inclus[]=", findParameter.IncludeThemesId)}";
        }
        
        //Rechercher dans les thèmes (exclus)
        if (findParameter.ExcludeThemesId.Length > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&themes_exclus[]={string.Join("&themes_exclus[]=", findParameter.ExcludeThemesId)}";
        }
        
        //commit
        if (!baseUrl.EndsWith('?'))
        {
            baseUrl += "&";
            baseUrl += "commit=Rechercher";
        }
        
        //page
        if (page > 0)
        {
            if (!baseUrl.EndsWith('?'))
                baseUrl += "&";
            baseUrl += $"&page={page}";
        }
        
        if (baseUrl.EndsWith('?'))
            baseUrl = baseUrl.TrimEnd('?');
        else if (baseUrl.EndsWith('&'))
            baseUrl = baseUrl.TrimEnd('&');


        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;

        return uri;
    }

    /// <summary>
    /// Obtient l'url de la page de la liste des animes
    /// </summary>
    /// <param name="letter"></param>
    /// <param name="contentSection"></param>
    /// <param name="sheetType"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string? GetIcotakuFilterUrl(char letter, IcotakuSection contentSection, IcotakuSheetType sheetType, uint page = 1)
    {
        if (!char.IsLetter(letter))
            return null;
        
        return contentSection switch
        {
            IcotakuSection.Anime =>sheetType switch
            {
                IcotakuSheetType.Unknown => null,
                IcotakuSheetType.Anime => $"{IcotakuAnimeUrl}/animes.html?filter={letter}{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Person => $"{IcotakuAnimeUrl}/individus.html?filter={{letter}}{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Character => $"{IcotakuAnimeUrl}/personnages.html?filter={{letter}}{(page == 0 ? "" : "&page=" + page)}",
                IcotakuSheetType.Studio => null,
                IcotakuSheetType.Distributor => null,
                _ => throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, "Ce type de fiche n'est pas pris en charge.")
            },
            IcotakuSection.Manga => null,
            IcotakuSection.LightNovel => null,
            IcotakuSection.Drama => null,
            IcotakuSection.Community => null,
            _ => throw new ArgumentOutOfRangeException(nameof(contentSection), contentSection, "Cette section du site Icotaku n'est pas prise en charge.")
        };
    }
    
    /// <summary>
    /// Obtient l'url de la page du planning saisonnier
    /// </summary>
    /// <param name="section"></param>
    /// <remarks>L'url n'est valide que pour les animes et les dramas</remarks>
    /// <returns></returns>
    public static string? GetSeasonalPlanningUrl(IcotakuSection section)
    {
        if (section is IcotakuSection.Anime or IcotakuSection.Drama)
            return $"{GetBaseUrl(section)}/planning_saisonnier.html";
        LogServices.LogDebug($"La section {section} n'est pas prise en charge.");
        return null;
    }
    
    
    /// <summary>
    /// Retourne l'url absolue de l'image à partir de l'attribut src du noeud img
    /// </summary>
    /// <param name="section">Correspond à la section du site permettant de sélectionner la bonne base Url</param>
    /// <param name="src">Valeur de l'attribut src</param>
    /// <remarks>Cette méthode remplace "/images/.." par la base url</remarks>
    /// <returns></returns>
    public static Uri? GetImageFromSrc(IcotakuSection section, string? src)
    {
        if (src == null || src.IsStringNullOrEmptyOrWhiteSpace())
        {
            return null;
        }

        var value = src.Replace("/images/..", GetBaseUrl(section));

        //https://anime.icotaku.com/uploads/animes/anime_229/fiche/affiche_umzrcyl4lhodbB8.jpg
        bool isUri = Uri.TryCreate(value, UriKind.Absolute, out var uri);
        return isUri && uri != null ? uri : null;
    }
    
    public static string? GetStartImgSrcByContactType(ContactType contactType)
    {
        return contactType switch
        {
            ContactType.Studio => null,
            ContactType.Distributor => null,
            ContactType.Person => "/uploads/individus/",
            ContactType.Character => "/uploads/personnages/",
            _ => null
        };
    }

    /// <summary>
    /// Retourne l'url absolue du lien à partir de l'attribut href du noeud a
    /// </summary>
    /// <param name="node">Noeud html contenant l'attribut Href</param>
    /// <param name="section">Base Url correspondant à la section du site</param>
    /// <returns></returns>
    public static Uri? GetFullHrefFromHtmlNode(HtmlNode node, IcotakuSection section)
    {
        var href = node.GetAttributeValue("href", string.Empty);
        if (href.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        return GetFullHrefFromRelativePath(href, section);
    }

    /// <summary>
    /// Retourne l'url absolue du lien à partir de son chemin relatif
    /// </summary>
    /// <param name="relativePath">Chemin relatif du lien </param>
    /// <param name="section">Base Url correspondant à la section du site</param>
    /// <returns></returns>
    public static Uri? GetFullHrefFromRelativePath(string relativePath, IcotakuSection section)
    {
        if (relativePath.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var href = relativePath.ToString();

        if (href.StartsWith('/'))
            href = href.TrimStart('/');

        href = $"{GetBaseUrl(section)}/{href}";
        
        href = href.Replace("&amp;", "&");

        if (Uri.TryCreate(href, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
            return uri;

        return null;
    }


    /// <summary>
    /// Retourne l'url absolue du dossier de téléchargement à partir de la section et de l'id de la fiche
    /// </summary>
    /// <param name="section"></param>
    /// <param name="sheetId"></param>
    /// <returns></returns>
    public static string? GetDownloadFolderUrl(IcotakuSheetType section, int sheetId)
    {
        var url = IcotakuDownloadBaseUrl + section switch
        {
            IcotakuSheetType.Anime => "/animes",
            IcotakuSheetType.Manga => "/mangas",
            IcotakuSheetType.LightNovel => "/novels",
            IcotakuSheetType.Drama => "/dramas",
            IcotakuSheetType.Character => "/personnages",
            IcotakuSheetType.Person => "/individus",
            _ => null
        };

        url += section switch
        {
            IcotakuSheetType.Anime => $"/anime_{sheetId}",
            IcotakuSheetType.Manga => $"/manga_{sheetId}",
            IcotakuSheetType.LightNovel => $"/novel_{sheetId}",
            IcotakuSheetType.Drama => $"/drama_{sheetId}",
            IcotakuSheetType.Character => $"/personnage_{sheetId}",
            IcotakuSheetType.Person => $"/individu_{sheetId}",
            _ => null
        };

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
            return uri.ToString();

        return null;
    }

    public static string? GetDownloadFolderUrl(IcotakuSheetType section, int sheetId, string fileName)
    {
        var url = GetDownloadFolderUrl(section, sheetId);
        if (url == null)
            return null;

        url += $"/{fileName}";

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
            return uri.ToString();

        return null;
    }

    public static string? GetDownloadFolderUrl(IcotakuSheetType sheetType, int sheetId, IcotakuDefaultSubFolder type,
        int episodeNumber = 0)
    {
        var url = GetDownloadFolderUrl(sheetType, sheetId);
        if (url == null)
            return null;

        url += type switch
        {
            IcotakuDefaultSubFolder.Episod => $"/episodes/episode_{episodeNumber}",
            IcotakuDefaultSubFolder.Tome => $"/tomes/tome_{episodeNumber}",
            IcotakuDefaultSubFolder.Sheet => $"/fiche",
            IcotakuDefaultSubFolder.None => string.Empty,
            _ => null
        };

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
            return uri.ToString();

        return null;
    }

    #endregion

    /// <summary>
    /// Retourne le chemin relatif du dossier de téléchargement à partir de la section et de l'id de la fiche
    /// </summary>
    /// <param name="type"></param>
    /// <param name="episodeNumber"></param>
    /// <returns></returns>
    public static string? GetRelativeSubFolderPath(IcotakuDefaultSubFolder type, int episodeNumber = 0)
    {
        return type switch
        {
            IcotakuDefaultSubFolder.Episod => $"{Path.DirectorySeparatorChar}episodes{Path.DirectorySeparatorChar}episode_{episodeNumber}",
            IcotakuDefaultSubFolder.Tome => $"{Path.DirectorySeparatorChar}tomes{Path.DirectorySeparatorChar}tome_{episodeNumber}",
            IcotakuDefaultSubFolder.Sheet => $"{Path.DirectorySeparatorChar}fiche",
            IcotakuDefaultSubFolder.None => string.Empty,
            _ => null
        };
    }

    /// <summary>
    /// Retourne le contenu html de la fiche restreinte (Hentai, Yaoi, Yuri, etc...) en se connectant au serveur d'Icotaku 
    /// </summary>
    /// <param name="section"></param>
    /// <param name="sheetUri"></param>
    /// <param name="username">Nom d'utilisateur du compte Icotaku</param>
    /// <param name="password">Mot de passe du compe Icotaku</param>
    /// <returns></returns>
    internal static async Task<string?> GetRestrictedHtmlAsync(IcotakuSection section, Uri sheetUri, string username,
        string password)
    {
        try
        {
            var baseUrl = GetBaseUrl(section);
            if (baseUrl == null)
                return null;

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri(baseUrl);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));


            // Récupérer la page d'accueil pour récupérer le jeton CSRF
            using var homePageResult = await client.GetAsync("/");
            homePageResult.EnsureSuccessStatusCode();

            var homePageContent = await homePageResult.Content.ReadAsStringAsync();
            var homePageDocument = new HtmlDocument();
            homePageDocument.LoadHtml(homePageContent);

            var csrfTokenNode = homePageDocument.DocumentNode.SelectSingleNode("//input[@name='_csrf_token']");
            if (csrfTokenNode == null)
            {
                LogServices.LogDebug("Le jeton CSRF n'a pas été trouvé");
                return null;
            }

            var csrfToken = csrfTokenNode.GetAttributeValue("value", string.Empty);

            if (csrfToken.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug("Le jeton CSRF est vide");
                return null;
            }

            // Récupérer le cookie de session
            var cookies = handler.CookieContainer.GetCookies(client.BaseAddress);
            var sessionCookie = cookies["icookie"];

            if (sessionCookie == null)
            {
                LogServices.LogDebug("Le cookie de session n'a pas été trouvé");
                return null;
            }

            // Se connecter
            using var loginResult = await client.PostAsync("/login.html", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "_csrf_token", csrfToken },
                    { "login", username },
                    { "password", password },
                    { "referer", "/" }
                }));

            loginResult.EnsureSuccessStatusCode();

            // Récupérer la page de la fiche
            var sheetResult = await client.GetAsync(sheetUri.PathAndQuery);

            sheetResult.EnsureSuccessStatusCode();

            var sheetContent = await sheetResult.Content.ReadAsStringAsync();
            return sheetContent;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

        internal static async Task<string?> GetAdvancedSearchHtmlAsync(IcotakuSection section, Uri sheetUri, string username,
        string password)
    {
        try
        {
            var baseUrl = GetBaseUrl(section);
            if (baseUrl == null)
                return null;

            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = true
            };

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri(baseUrl);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr"));


            // Récupérer la page d'accueil pour récupérer le jeton CSRF
            using var homePageResult = await client.GetAsync("/");
            homePageResult.EnsureSuccessStatusCode();

            var homePageContent = await homePageResult.Content.ReadAsStringAsync();
            var homePageDocument = new HtmlDocument();
            homePageDocument.LoadHtml(homePageContent);

            var csrfTokenNode = homePageDocument.DocumentNode.SelectSingleNode("//input[@name='_csrf_token']");
            if (csrfTokenNode == null)
            {
                LogServices.LogDebug("Le jeton CSRF n'a pas été trouvé");
                return null;
            }

            var csrfToken = csrfTokenNode.GetAttributeValue("value", string.Empty);

            if (csrfToken.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug("Le jeton CSRF est vide");
                return null;
            }

            // Récupérer le cookie de session
            var cookies = handler.CookieContainer.GetCookies(client.BaseAddress);
            var sessionCookie = cookies["icookie"];

            if (sessionCookie == null)
            {
                LogServices.LogDebug("Le cookie de session n'a pas été trouvé");
                return null;
            }

            // Se connecter
            using var loginResult = await client.PostAsync("/login.html", new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "_csrf_token", csrfToken },
                    { "login", username },
                    { "password", password },
                    { "referer", "/" }
                }));

            loginResult.EnsureSuccessStatusCode();

            // Récupérer la page de la fiche
            var sheetResult = await client.GetAsync(sheetUri.PathAndQuery);

            sheetResult.EnsureSuccessStatusCode();

            var sheetContent = await sheetResult.Content.ReadAsStringAsync();
            return sheetContent;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }


    #region Download

    internal static async Task<string?> DownloadThumbnailAsync(IcotakuSheetType sheetType, Guid itemGuid,
        Uri thumbnailUri, bool deleteIfExists, CancellationToken cancellationToken)
    {
        if (itemGuid == Guid.Empty)
        {
            LogServices.LogDebug($"Impossible de récupérer le Guid de l'item {sheetType}");
            return null;
        }

        var defaultFolder = sheetType.ConvertToDefaultFolder();

        var relativeSubFolderPath = GetRelativeSubFolderPath(IcotakuDefaultSubFolder.Sheet);
        if (relativeSubFolderPath == null)
            return null;

        var baseFolderPath = InputOutput.CreateItemDirectory(defaultFolder, itemGuid);
        if (baseFolderPath == null || baseFolderPath.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var partialPaths = relativeSubFolderPath
            .Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        partialPaths.Insert(0, baseFolderPath);
        
        var folderPath = Path.Combine(partialPaths.ToArray());
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Path.GetFileName(thumbnailUri.LocalPath);
        if (fileName.IsStringNullOrEmptyOrWhiteSpace() || !Path.HasExtension(fileName))
            return null;

        var thumbnailPath = Path.Combine(folderPath, fileName);
        if (!Path.IsPathFullyQualified(thumbnailPath))
            return null;
        
        if (File.Exists(thumbnailPath))
        {
            if (!deleteIfExists)
                return thumbnailPath;
            
            File.Delete(thumbnailPath);
        }

        var isSuccess = await WebServices.DownloadFileAsync(thumbnailUri, thumbnailPath, cancellationToken);
        if (!isSuccess || !File.Exists(thumbnailPath))
            return null;

        return thumbnailPath;
    }


    /// <summary>
    /// Télécharge le dossier complet de la fiche comprendant les fichiers et les sous dossiers
    /// </summary>
    /// <param name="sheetType"></param>
    /// <param name="sheetId"></param>
    /// <param name="itemGuid"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Retourne le chemin d'accès complet du dossier</returns>
    internal static async Task<bool> DownloadFullSheetFolderAsync(IcotakuSheetType sheetType, int sheetId,
        Guid itemGuid, CancellationToken cancellationToken)
    {
        if (itemGuid == Guid.Empty)
        {
            LogServices.LogDebug($"Impossible de récupérer le Guid de l'item {sheetType} {sheetId}");
            return false;
        }

        var defaultFolder = sheetType.ConvertToDefaultFolder();

        var directoryUrl = GetDownloadFolderUrl(sheetType, sheetId);
        if (directoryUrl == null || directoryUrl.IsStringNullOrEmptyOrWhiteSpace() ||
            (!Uri.TryCreate(directoryUrl, UriKind.Absolute, out var url) || !url.IsAbsoluteUri))
            return false;

        var baseFolderPath = InputOutput.CreateItemDirectory(defaultFolder, itemGuid);
        if (baseFolderPath == null || baseFolderPath.IsStringNullOrEmptyOrWhiteSpace())
            return false;

        var web = new HtmlWeb();
        var doc = web.Load(url);

        var nodes = doc.DocumentNode.SelectNodes("//tr[position() >= 4]//a[@href]")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            return false;

        foreach (var node in nodes)
        {
            if (cancellationToken.IsCancellationRequested)
                return false;

            var href = node.GetAttributeValue("href", null);
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var absoluteNodeUrl = url.ToString();
            if (!absoluteNodeUrl.EndsWith('/'))
                absoluteNodeUrl += '/';
            absoluteNodeUrl += href;

            if (!Uri.TryCreate(absoluteNodeUrl, UriKind.Absolute, out var _url) || !_url.IsAbsoluteUri)
                continue;

            if (node.InnerText.Trim().EndsWith('/'))
            {
                var directoryName = node.InnerText.Trim().Replace("/", "");

                var folderPath = Path.Combine(baseFolderPath, directoryName);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                await DownLoadFolderAsync(folderPath, _url, cancellationToken);
            }
            else if (Path.HasExtension(href))
            {
                var destinationFile = Path.Combine(baseFolderPath, href);
                if (!Path.IsPathFullyQualified(destinationFile))
                    continue;
                _ = await WebServices.DownloadFileAsync(_url, destinationFile, cancellationToken);
            }
        }

        return true;
    }

    private static async Task DownLoadFolderAsync(string folderPath, Uri url, CancellationToken cancellationToken)
    {
        if (folderPath == null || folderPath.IsStringNullOrEmptyOrWhiteSpace() || !Directory.Exists(folderPath))
            return;

        var web = new HtmlWeb();
        var doc = web.Load(url);

        var nodes = doc.DocumentNode.SelectNodes("//tr[position() >= 4]//a[@href]")?.ToArray();
        if (nodes == null || nodes.Length == 0)
            return;

        foreach (var node in nodes)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            var href = node.GetAttributeValue("href", null);
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
                continue;

            var absoluteNodeUrl = url.ToString();
            if (!absoluteNodeUrl.EndsWith('/'))
                absoluteNodeUrl += '/';
            absoluteNodeUrl += href;

            if (!Uri.TryCreate(absoluteNodeUrl, UriKind.Absolute, out var _url) || !_url.IsAbsoluteUri)
                continue;

            if (node.InnerText.Trim().EndsWith('/'))
            {
                var directoryName = node.InnerText.Trim().Replace("/", "");
                var _folderPath = Path.Combine(folderPath, directoryName);
                if (!Directory.Exists(_folderPath))
                    Directory.CreateDirectory(_folderPath);

                await DownLoadFolderAsync(_folderPath, _url, cancellationToken);
            }
            else if (Path.HasExtension(href))
            {
                var destinationFile = Path.Combine(folderPath, href);
                if (!Path.IsPathFullyQualified(destinationFile))
                    continue;
                _ = await WebServices.DownloadFileAsync(_url, destinationFile, cancellationToken);
            }
        }
    }

    #endregion
}