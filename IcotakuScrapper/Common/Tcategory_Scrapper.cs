﻿using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using System.Web;

namespace IcotakuScrapper.Common
{
    public partial class Tcategory
    {
        /// <summary>
        /// Retourne le type de catégorie en fonction de l'url de la page de catégorie depuis icotaku.com
        /// </summary>
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
        /// <param name="isDeleteSectionRecords"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="insertMode"></param>
        /// <returns></returns>
        public static async Task<OperationState> ScrapAsync(HashSet<IcotakuSection> sections,
             DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        bool isDeleteSectionRecords = true, CancellationToken? cancellationToken = null)
        {
            if (sections.Count == 0)
                return new OperationState(false, "Aucune section n'a été spécifiée");

            List<Tcategory> listOfCategories = [];

            foreach (var section in sections)
            {
                if (isDeleteSectionRecords)
                {
                    var deleteResult = await DeleteAsync(section, cancellationToken);
                    if (!deleteResult.IsSuccess)
                        return deleteResult;
                }

                var categories = ScrapFromCategoriesArrayPage(section, CategoryType.Theme).ToList();
                if (categories.Count > 0)
                    listOfCategories.AddRange(categories);

                categories = ScrapFromCategoriesArrayPage(section, CategoryType.Genre).ToList();
                if (categories.Count > 0)
                    listOfCategories.AddRange(categories);
            }

            if (listOfCategories.Count == 0)
                return new OperationState(false, "Aucune catégorie n'a été trouvée");


            return await InsertOrReplaceAsync(listOfCategories, insertMode, cancellationToken);
        }

        /// <summary>
        /// Scrape les catégories depuis la page contenant toutes les catégories en fonction du type de contenu (Anime, Manga, etc) depuis icotaku.com
        /// </summary>
        /// <param name="section"></param>
        /// <param name="categoryType"></param>
        /// <returns></returns>
        private static IEnumerable<Tcategory> ScrapFromCategoriesArrayPage(IcotakuSection section, CategoryType categoryType)
        {
            var pageUrl = IcotakuWebHelpers.GetCategoriesUrl(section, categoryType);
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
                var uri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(node, section);
                if (uri == null)
                    continue;

                Tcategory? category = ScrapCategoryFromSheetPage(uri, section, categoryType);
                if (category != null)
                    yield return category;
            }
        }

        /// <summary>
        /// Scrape les informations de la catégorie depuis la page contenant la fiche de la catégorie depuis icotaku.com
        /// </summary>
        /// <param name="sheetUri"></param>
        /// <param name="categorySection"></param>
        /// <param name="categoryTypeToCheck"></param>
        /// <returns></returns>
        internal static Tcategory? ScrapCategoryFromSheetPage(Uri sheetUri, IcotakuSection? categorySection = null, CategoryType? categoryTypeToCheck = null)
        {
            // On récupère l'id de la fiche de la catégorie
            var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
            if (sheetId == 0)
                return null;

            // On vérifie que la section de la fiche est bien celle demandée
            var section = IcotakuWebHelpers.GetIcotakuSection(sheetUri);
            if (!section.HasValue)
                return null;

            if (categorySection.HasValue && section.Value != categorySection.Value)
                return null;

            // On vérifie que le type de la catégorie est bien celui demandé
            var categoryType = GetCategoryType(sheetUri);
            if (!categoryType.HasValue)
                return null;

            if (categoryTypeToCheck.HasValue && categoryType.Value != categoryTypeToCheck.Value)
                return null;

            HtmlWeb web = new();
            var htmlDocument = web.Load(sheetUri.ToString());

            var nameNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='fiche_entete']//h1/text()");

            var name = HttpUtility.HtmlDecode(nameNode?.InnerText?.Trim())?.Trim();
            if (name == null || name.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']/div[@class='contenu']/p[1]/text()");
            var description = HttpUtility.HtmlDecode(descriptionNode?.InnerText?.Trim())?.Trim();

            Tcategory tcategory = new()
            {
                SheetId = sheetId,
                Section = section.Value,
                Type = categoryType.Value,
                Url = sheetUri.ToString(),
                Name = name,
                Description = description
            };

            return tcategory;
        }
    }
}
