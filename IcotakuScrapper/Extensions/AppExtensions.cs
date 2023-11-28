using Microsoft.Data.Sqlite;

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
        
        /// <summary>
        /// Retourne l'identifiant de la dernière ligne insérée dans la base de données.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static async Task<int> GetLastInsertRowIdAsync(this SqliteCommand command) => await ExtensionMethods.GetLastInsertRowIdAsync(command);
        
        /// <summary>
        ///Ajoute les clauses LIMIT et OFFSET à la commande SQL.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        internal static void AddLimitOffset(this SqliteCommand command, uint limit, uint offset) => ExtensionMethods.AddLimitOffset(command, limit, offset);
        
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
        
        internal static async Task<int> GetLastInsertRowIdAsync(SqliteCommand command)
        {
            command.CommandText = "SELECT last_insert_rowid()";
            var value = await command.ExecuteScalarAsync();
            if (value is long id)
                return (int)id;

            throw new InvalidOperationException("Impossible de récupérer l'identifiant de la ligne insérée");
        }
        
        internal static void AddLimitOffset(SqliteCommand command, uint limit, uint offset)
        {
            if (limit > 0)
                command.CommandText += $" LIMIT {limit}";
            if (offset > 0)
                command.CommandText += $" OFFSET {offset}";
        }

    }
}
