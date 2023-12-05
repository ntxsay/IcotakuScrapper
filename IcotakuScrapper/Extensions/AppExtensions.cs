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

        public static ContactType ConvertTo(this SheetType sheetType) => ExtensionMethods.ConvertTo(sheetType);
        public static SheetType ConvertTo(this ContactType contactType) => ExtensionMethods.ConvertTo(contactType);


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

        public static ContactType ConvertTo(SheetType sheetType) => sheetType switch
        {
            SheetType.Anime => ContactType.Unknown,
            SheetType.Unknown => ContactType.Unknown,
            SheetType.Person => ContactType.Person,
            SheetType.Character => ContactType.Character,
            SheetType.Studio => ContactType.Studio,
            SheetType.Distributor => ContactType.Distributor,
            _ => throw new ArgumentOutOfRangeException(nameof(sheetType), sheetType, "La valeur spécifiée est invalide")
        };

        public static SheetType ConvertTo(ContactType contactType) => contactType switch
        {
            ContactType.Unknown => SheetType.Unknown,
            ContactType.Person => SheetType.Person,
            ContactType.Character => SheetType.Character,
            ContactType.Studio => SheetType.Studio,
            ContactType.Distributor => SheetType.Distributor,
            _ => throw new ArgumentOutOfRangeException(nameof(contactType), contactType, "La valeur spécifiée est invalide")
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
