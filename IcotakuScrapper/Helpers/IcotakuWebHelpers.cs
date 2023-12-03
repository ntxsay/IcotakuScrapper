using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Helpers
{
    internal static class IcotakuWebHelpers
    {
        public const string IcotakuBaseUrl = "https://icotaku.com";
        public static string? GetBaseUrl(IcotakuSection section) => section switch
        {
            IcotakuSection.Anime => "https://anime.icotaku.com",
            IcotakuSection.Manga => "https://manga.icotaku.com",
            IcotakuSection.LightNovel => "https://novel.icotaku.com",
            IcotakuSection.Drama => "https://drama.icotaku.com",
            IcotakuSection.Community => "https://communaute.icotaku.com",
            _ => null
        };

        public static IcotakuSection? GetIcotakuSection(Uri sheetUri)
        {
            var host = sheetUri.Host;
            if (host.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            return host switch
            {
                "anime.icotaku.com" => IcotakuSection.Anime,
                "manga.icotaku.com" => IcotakuSection.Manga,
                "novel.icotaku.com" => IcotakuSection.LightNovel,
                "drama.icotaku.com" => IcotakuSection.Drama,
                "communaute.icotaku.com" => IcotakuSection.Community,
                _ => null
            };
        }


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

        public static Uri? GetFullHrefFromHtmlNode(HtmlNode node, IcotakuSection section)
        {
            var href = node.GetAttributeValue("href", string.Empty);
            if (href.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            if (href.StartsWith('/'))
                href = href.TrimStart('/');

            href = $"{GetBaseUrl(section)}/{href}";

            if (Uri.TryCreate(href, UriKind.Absolute, out var uri) && uri.IsAbsoluteUri)
                return uri;

            return null;
        }

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
    }
}
