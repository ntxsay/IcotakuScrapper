using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Services.IOS;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Services
{
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
        public static int? GetSheetId(Uri sheetUri)
        {
            var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
            if (splitUrl.Length == 0)
                return null;

            var sheetId = splitUrl.FirstOrDefault(f =>
                       !f.Any(a => char.IsLetter(a) || a == '-' || a == '_'));

            if (sheetId.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            if (!int.TryParse(sheetId, out var sheetIdInt))
                return null;

            return sheetIdInt;
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
        public static string? GetDownloadFolderUrl(IcotakuDownloadSection section, int sheetId)
        {
            var url = IcotakuDownloadBaseUrl + section switch
            {
                IcotakuDownloadSection.Anime => "/animes",
                IcotakuDownloadSection.Manga => "/mangas",
                IcotakuDownloadSection.LightNovel => "/novels",
                IcotakuDownloadSection.Drama => "/dramas",
                IcotakuDownloadSection.Character => "/personnages",
                IcotakuDownloadSection.Person => "/individus",
                _ => null
            };

            url += section switch
            {
                IcotakuDownloadSection.Anime => $"/anime_{sheetId}",
                IcotakuDownloadSection.Manga => $"/manga_{sheetId}",
                IcotakuDownloadSection.LightNovel => $"/novel_{sheetId}",
                IcotakuDownloadSection.Drama => $"/drama_{sheetId}",
                IcotakuDownloadSection.Character => $"/personnage_{sheetId}",
                IcotakuDownloadSection.Person => $"/individu_{sheetId}",
                _ => null
            };

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
                return uri.ToString();

            return null;
        }

        public static string? GetDownloadFolderUrl(IcotakuDownloadSection section, int sheetId, string fileName)
        {
            var url = GetDownloadFolderUrl(section, sheetId);
            if (url == null)
                return null;

            url += $"/{fileName}";

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
                return uri.ToString();

            return null;
        }

        public static string? GetDownloadFolderUrl(IcotakuDownloadSection section, int sheetId, IcotakuDownloadType type, int episodeNumber = 0)
        {
            var url = GetDownloadFolderUrl(section, sheetId);
            if (url == null)
                return null;

            url += type switch
            {
                IcotakuDownloadType.Episod => $"/episodes/episode_{episodeNumber}",
                IcotakuDownloadType.Tome => $"/tomes/tome_{episodeNumber}",
                IcotakuDownloadType.Sheet => $"/fiche",
                IcotakuDownloadType.None => string.Empty,
                _ => null
            };

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
                return uri.ToString();

            return null;
        }
    }

    


}
