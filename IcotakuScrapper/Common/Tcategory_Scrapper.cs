using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using System.Web;

namespace IcotakuScrapper.Common
{
    public partial class Tcategory
    {
        /// <summary>
        /// Retourne l'url de la page de catégorie en fonction du type de contenu (Anime, Manga, etc) depuis icotaku.com
        /// </summary>
        /// <param name="section"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string GetCategoriesUrl(IcotakuSection section, CategoryType categoryType)
        {
            return section switch
            {
                IcotakuSection.Anime => categoryType switch
                {
                    CategoryType.Theme => Main.GetBaseUrl(section) + "/themes.html",
                    CategoryType.Genre => Main.GetBaseUrl(section) + "/genres.html",
                    _ => throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, "Ce type de catégorie n'est pas pris en charge")
                },
                _ => throw new ArgumentOutOfRangeException(nameof(section), section, "Ce type de contenu n'est pas pris en charge")
            };
        }

        /// <summary>
        /// Retourne le type de catégorie en fonction de l'url de la page de catégorie depuis icotaku.com
        /// </summary>
        /// <param name="sheetUri"></param>
        /// <returns></returns>
        public static CategoryType? GetCategoryType(Uri sheetUri)
        {
            var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
            if (splitUrl.Length == 0)
                return null;

            return splitUrl[0] switch
            {
                "themes" or "theme" => CategoryType.Theme,
                "genres" or "genre" => CategoryType.Genre,
                _ => null
            };
        }

        /// <summary>
        /// Ajoute à la base de données les catégories en fonction du type de contenu (Anime, Manga, etc) depuis icotaku.com
        /// </summary>
        /// <param name="sections"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<OperationState> CreateIndexAsync(HashSet<IcotakuSection> sections, CancellationToken? cancellationToken = null)
        {
            if (sections.Count == 0)
                return new OperationState(false, "Aucune section n'a été spécifiée");

            await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

            List<Tcategory> listOfCategories = [];

            foreach (var section in sections)
            {
                var deleteAllResult = await DeleteAsync(section, cancellationToken, command);
                if (!deleteAllResult.IsSuccess)
                    continue;

                var categories = GetCategories(section, CategoryType.Theme).ToList();
                if (categories.Count > 0)
                    listOfCategories.AddRange(categories);

                categories = GetCategories(section, CategoryType.Genre).ToList();
                if (categories.Count > 0)
                    listOfCategories.AddRange(categories);
            }

            if (listOfCategories.Count == 0)
                return new OperationState(false, "Aucune catégorie n'a été trouvée");


            return await InsertAsync(listOfCategories, cancellationToken, command);
        }

        /// <summary>
        /// Retourne les catégories en fonction du type de contenu (Anime, Manga, etc) depuis icotaku.com
        /// </summary>
        /// <param name="section"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        private static IEnumerable<Tcategory> GetCategories(IcotakuSection section, CategoryType categoryType)
        {
            var pageUrl = GetCategoriesUrl(section, categoryType);
            HtmlWeb web = new();
            var htmlDocument = web.Load(pageUrl);

            var nodes = categoryType switch
            {
                CategoryType.Genre => htmlDocument.DocumentNode.SelectNodes("//div[@id='listecontenu']//a[contains(@href, '/genre/')]").ToArray(),
                CategoryType.Theme => htmlDocument.DocumentNode.SelectNodes("//div[@id='listecontenu']//a[contains(@href, '/theme/')]").ToArray(),
                _ => throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, "Ce type de catégorie n'est pas pris en charge")
            };

            if (nodes.Length == 0)
                yield break;

            foreach (var node in nodes)
            {
                // On récupère l'url de la fiche de la catégorie
                var uri = Main.GetFullHrefFromHtmlNode(node, section);
                if (uri == null)
                    continue;

                // On récupère l'id de la fiche de la catégorie
                var sheetId = Main.GetSheetId(uri);
                if (!sheetId.HasValue)
                    continue;

                // On vérifie que la section de la fiche est bien celle demandée
                var checksection = Main.GetIcotakuSection(uri);
                if (!checksection.HasValue)
                    continue;

                if (checksection.Value != section)
                    continue;

                // On vérifie que le type de la catégorie est bien celui demandé
                var checkCategoryType = GetCategoryType(uri);
                if (!checkCategoryType.HasValue)
                    continue;

                if (checkCategoryType.Value != categoryType)
                    continue;

                Tcategory tcategory = new()
                {
                    SheetId = sheetId.Value,
                    Section = section,
                    Type = categoryType,
                    Url = uri.ToString()
                };

                var result = GetCategory(tcategory);
                if (result != null)
                    yield return result;
            }
        }


        /// <summary>
        /// Rempli les propriétés Name et Description de l'objet Tcategory
        /// </summary>
        /// <param name="tcategory"></param>
        /// <returns></returns>
        private static Tcategory? GetCategory(Tcategory tcategory)
        {
            HtmlWeb web = new();
            var htmlDocument = web.Load(tcategory.Url);

            var nameNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='fiche_entete']//h1/text()");

            var name = HttpUtility.HtmlDecode(nameNode?.InnerText?.Trim())?.Trim();
            if (name == null || name.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']/div[@class='contenu']/p[1]/text()");
            var description = HttpUtility.HtmlDecode(descriptionNode?.InnerText?.Trim())?.Trim();

            tcategory.Name = name;
            tcategory.Description = description;

            return tcategory;
        }
    }
}
