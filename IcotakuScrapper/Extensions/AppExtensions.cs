namespace IcotakuScrapper.Extensions
{
    internal static class AppExtensions
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

        public static ContactType ConvertTo(this IcotakuSheetType sheetType) 
            => ExtensionMethods.ConvertTo(sheetType);
        public static IcotakuSheetType ConvertTo(this ContactType contactType) 
            => ExtensionMethods.ConvertTo(contactType);

        public static IcotakuDefaultFolder ConvertDefaultFolderTo(this IcotakuSection section) 
            => ExtensionMethods.ConvertDefaultFolderTo(section);

        public static IcotakuDefaultFolder ConvertToDefaultFolder(this IcotakuSheetType sheetType)
            => ExtensionMethods.ConvertToDefaultFolder(sheetType);

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

        internal static int CountPage(int countItems, int maxContentByPage = 20)
        {
            try
            {
                if (countItems == 0)
                    return 10;
                return maxContentByPage <= 0 ? 10 : (int)Math.Ceiling((double)countItems / maxContentByPage);
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}
