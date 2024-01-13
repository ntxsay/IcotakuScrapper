using System.Collections.ObjectModel;

namespace IcotakuScrapper.Extensions
{
    public static class AppExtensions
    {
        /// <summary>
        /// Retourne une valeur booléenne indiquant si la valeur de type string est null, vide ou ne contient que des espaces blancs
        /// </summary>
        /// <param name="self">Valeur</param>
        /// <returns>Une valeur booléenne</returns>
        public static bool IsStringNullOrEmptyOrWhiteSpace(this string? self) => ExtensionMethods.IsStringNullOrEmptyOrWhiteSpace(self);

        /// <summary>
        /// Retourne une valeur booléenne indiquant si la valeur de type string est vide ou ne contenant que des espaces blancs
        /// </summary>
        /// <param name="self">Valeur</param>
        /// <returns>Une valeur booléenne</returns>
        public static bool IsStringEmptyOrWhiteSpace(this string self) => ExtensionMethods.IsStringEmptyOrWhiteSpace(self);

        /// <summary>
        /// Convertit une énumération en ObservableCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ObservableCollection<T> ToObservableNewCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ObservableCollection<T>(source);
        }

        public static void ToObservableCollection<T>(this ObservableCollection<T> source, IEnumerable<T>? values, bool clearSource = false)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (clearSource && source.Count > 0)
                source.Clear();

            if (values == null) 
                return;
            
            var enumerable = values as T[] ?? values.ToArray();
            if (enumerable.Length == 0)
                return;

            foreach (var value in enumerable)
            {
                source.Add(value);
            }
        }
        
        public static void ToObservableCollection<T>(this HashSet<T> source, IEnumerable<T>? values, bool clearSource = false)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (clearSource && source.Count > 0)
                source.Clear();

            if (values == null) 
                return;
            
            var enumerable = values as T[] ?? values.ToArray();
            if (enumerable.Length == 0)
                return;

            foreach (var value in enumerable)
            {
                source.Add(value);
            }
        }
        
        public static ContactType ConvertTo(this IcotakuSheetType sheetType)
            => ExtensionMethods.ConvertTo(sheetType);
        public static IcotakuSheetType ConvertTo(this ContactType contactType)
            => ExtensionMethods.ConvertTo(contactType);

        public static IcotakuDefaultFolder ConvertDefaultFolderTo(this IcotakuSection section)
            => ExtensionMethods.ConvertDefaultFolderTo(section);

        /// <summary>
        /// Convertit valeur de l'énumération <see cref="IcotakuSheetType"/> en <see cref="IcotakuDefaultFolder"/>
        /// </summary>
        /// <param name="sheetType"></param>
        /// <returns></returns>
        public static IcotakuDefaultFolder ConvertToDefaultFolder(this IcotakuSheetType sheetType)
            => ExtensionMethods.ConvertToDefaultFolder(sheetType);

        public static string GetLiteralDiffusion(this DiffusionStateKind stateKind)
            => IcotakuHelpers.GetDiffusionStateLiteral(stateKind);

        public static DiffusionStateKind GetDiffusionStateKind(this string? value)
            => IcotakuHelpers.GetDiffusionStateKind(value);
        
        public static string? GetSearchPamareter(this DiffusionStateKind stateKind)
            => IcotakuHelpers.GetDiffusionStateSearchPamareter(stateKind);
        
        public static string? GetMonthSearchPamareter(this byte monthNumber)
            => DateHelpers.GetMonthSearchParameter(monthNumber);
        
        public static string? GetSearchPamareter(this WeatherSeasonKind seasonKind)
            => SeasonHelpers.GetSeasonSearchParameter(seasonKind);
    }

    internal static class ExtensionMethods
    {
        /// <summary>
        /// Obtient une valeur Booléenne indiquant si la chaîne de caractères saisit est Null ou vide ou ne contenant que des espaces blancs.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsStringNullOrEmptyOrWhiteSpace(string? value) =>
            value == null || string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
        internal static bool IsStringEmptyOrWhiteSpace(string value) =>
            string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);

        public static ContactType ConvertTo(IcotakuSheetType sheetType) => sheetType switch
        {
            IcotakuSheetType.Anime => ContactType.Unknown,
            IcotakuSheetType.Unknown => ContactType.Unknown,
            IcotakuSheetType.Person => ContactType.Person,
            IcotakuSheetType.Character => ContactType.Character,
            IcotakuSheetType.Studio => ContactType.Studio,
            IcotakuSheetType.Distributor => ContactType.Distributor,
            _ => throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, "La valeur spécifiée est invalide")
        };

        public static IcotakuSheetType ConvertTo(ContactType contactType) => contactType switch
        {
            ContactType.Unknown => IcotakuSheetType.Unknown,
            ContactType.Person => IcotakuSheetType.Person,
            ContactType.Character => IcotakuSheetType.Character,
            ContactType.Studio => IcotakuSheetType.Studio,
            ContactType.Distributor => IcotakuSheetType.Distributor,
            _ => throw new ArgumentOutOfRangeException(nameof(contactType), contactType, "La valeur spécifiée est invalide")
        };

        public static IcotakuDefaultFolder ConvertDefaultFolderTo(IcotakuSection section) => section switch
        {
            IcotakuSection.Anime => IcotakuDefaultFolder.Animes,
            IcotakuSection.Manga => IcotakuDefaultFolder.Mangas,
            IcotakuSection.LightNovel => IcotakuDefaultFolder.LightNovels,
            IcotakuSection.Drama => IcotakuDefaultFolder.Dramas,
            IcotakuSection.Community => IcotakuDefaultFolder.Community,
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, "La valeur spécifiée est invalide")
        };

        /// <summary>
        /// Convertit valeur de l'énumération <see cref="IcotakuSheetType"/> en <see cref="IcotakuDefaultFolder"/>
        /// </summary>
        /// <param name="sheetType"></param>
        /// <returns></returns>
        public static IcotakuDefaultFolder ConvertToDefaultFolder(IcotakuSheetType sheetType) => sheetType switch
        {
            IcotakuSheetType.Anime => IcotakuDefaultFolder.Animes,
            IcotakuSheetType.Manga => IcotakuDefaultFolder.Mangas,
            IcotakuSheetType.LightNovel => IcotakuDefaultFolder.LightNovels,
            IcotakuSheetType.Drama => IcotakuDefaultFolder.Dramas,
            IcotakuSheetType.Person => IcotakuDefaultFolder.Contacts,
            IcotakuSheetType.Character => IcotakuDefaultFolder.Contacts,
            IcotakuSheetType.Studio => IcotakuDefaultFolder.Contacts,
            IcotakuSheetType.Distributor => IcotakuDefaultFolder.Contacts,
            _ => throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, "La valeur spécifiée est invalide")
        };

        /// <summary>
        /// Compte le nombre de pages en fonction du nombre d'éléments et du nombre d'éléments par page
        /// </summary>
        /// <param name="countItems"></param>
        /// <param name="maxContentByPage"></param>
        /// <returns></returns>
        internal static uint CountPage(uint countItems, uint maxContentByPage = 20)
        {
            try
            {
                if (countItems == 0)
                    return 10;
                return maxContentByPage <= 0 ? 10 : (uint)Math.Ceiling((double)countItems / maxContentByPage);
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Retourne un tableau d'éléments en fonction de la page courante et du nombre d'éléments par page
        /// </summary>
        /// <param name="values"></param>
        /// <param name="currentPage"></param>
        /// <param name="maxContentByPage"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T[] GetPage<T>(IEnumerable<T>? values, uint currentPage = 1, uint maxContentByPage = 20)
        {
            if (values == null)
                return [];

            var enumerable = values as T[] ?? values.ToArray();
            if (enumerable.Length == 0)
                return [];

            var countItems = (uint)enumerable.Length;
            var totalPages = CountPage(countItems, maxContentByPage);
            if (currentPage > totalPages)
                currentPage = totalPages;
            else if (currentPage <= 0)
                currentPage = 1;

            var skip = (int)((currentPage - 1) * maxContentByPage);
            var take = (int)maxContentByPage;

            return enumerable.Skip(skip).Take(take).ToArray();
        }
        
        
    }
}
