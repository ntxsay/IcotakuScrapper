using IcotakuScrapper.Anime;
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

        public static ContactType ConvertTo(this SheetType sheetType) => ExtensionMethods.ConvertTo(sheetType);
        public static SheetType ConvertTo(this ContactType contactType) => ExtensionMethods.ConvertTo(contactType);

        public static void AddOrderSort(this SqliteCommand command, AnimeSeasonalPlanningSortBy sortBy, OrderBy orderBy) 
            => ExtensionMethods.AddOrderSort(command, sortBy, orderBy);
        
        public static void AddOrderSort(this SqliteCommand command, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy)
            => ExtensionMethods.AddOrderSort(command, sortBy, orderBy);
        
        public static void StartWithInsertMode(this SqliteCommand command, DbInsertMode insertMode)
            => ExtensionMethods.StartWithInsertMode(command, insertMode);
        
        public static bool IsIntColumnValidated(this SqliteCommand command, IntColumnSelect currentSelectedColumn, HashSet<IntColumnSelect> acceptedColumns)
            => ExtensionMethods.IsIntColumnValidated(command, currentSelectedColumn, acceptedColumns);
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

        public static void StartWithInsertMode(SqliteCommand command, DbInsertMode insertMode)
        {
            command.CommandText = insertMode switch
            {
                DbInsertMode.Insert => "INSERT ",
                DbInsertMode.InsertOrAbort => "INSERT OR ABORT ",
                DbInsertMode.InsertOrFail => "INSERT OR FAIL ",
                DbInsertMode.InsertOrIgnore => "INSERT OR IGNORE ",
                DbInsertMode.InsertOrReplace => "INSERT OR REPLACE ",
                DbInsertMode.InsertOrRollback => "INSERT OR ROLLBACK ",
                DbInsertMode.Replace => "REPLACE ",
                _ => throw new ArgumentOutOfRangeException(nameof(insertMode), insertMode, null)
            };
        }
        
        public static void AddOrderSort(SqliteCommand command, AnimeSeasonalPlanningSortBy sortBy, OrderBy orderBy)
        {
            command.CommandText += Environment.NewLine + sortBy switch
            {
                AnimeSeasonalPlanningSortBy.Id => $"ORDER BY TanimeSeasonalPlanning.Id {orderBy}",
                AnimeSeasonalPlanningSortBy.SheetId => $"ORDER BY TanimeSeasonalPlanning.SheetId {orderBy}",
                AnimeSeasonalPlanningSortBy.ReleaseDate => $"ORDER BY TanimeSeasonalPlanning.ReleaseMonth {orderBy}",
                AnimeSeasonalPlanningSortBy.AnimeName => $"ORDER BY TanimeSeasonalPlanning.AnimeName {orderBy}",
                AnimeSeasonalPlanningSortBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {orderBy}",
                AnimeSeasonalPlanningSortBy.Season => $"ORDER BY Tseason.SeasonNumber {orderBy}",
                AnimeSeasonalPlanningSortBy.GroupName => $"ORDER BY TanimeSeasonalPlanning.GroupName {orderBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "La valeur spécifiée est invalide")
            };
        }
        
        public static void AddOrderSort(SqliteCommand command, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy)
        {
            command.CommandText += Environment.NewLine + sortBy switch
            {
                AnimeDailyPlanningSortBy.Id => $"ORDER BY TanimeDailyPlanning.Id {orderBy}",
                AnimeDailyPlanningSortBy.ReleaseDate => $"ORDER BY TanimeDailyPlanning.ReleaseDate {orderBy}",
                AnimeDailyPlanningSortBy.EpisodeNumber => $"ORDER BY TanimeDailyPlanning.NoEpisode {orderBy}",
                AnimeDailyPlanningSortBy.EpisodeName => $"ORDER BY TanimeDailyPlanning.EpisodeName {orderBy}",
                AnimeDailyPlanningSortBy.Day => $"ORDER BY TanimeDailyPlanning.NoDay {orderBy}",
                AnimeDailyPlanningSortBy.AnimeName => $"ORDER BY TanimeDailyPlanning.AnimeName {orderBy}",
                AnimeDailyPlanningSortBy.SheetId => $"ORDER BY TanimeDailyPlanning.SheetId {orderBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "La valeur spécifiée est invalide")
            };
        }
        
        public static bool IsIntColumnValidated(SqliteCommand command, IntColumnSelect currentSelectedColumn, HashSet<IntColumnSelect> acceptedColumns)
        {
            if (acceptedColumns.Count == 0)
            {
                command.CommandText = string.Empty;
                return false;
            }

            var any = acceptedColumns.Any(a => a == currentSelectedColumn);
            if (!any)
            {
                command.CommandText = string.Empty;
                return false;
            }

            return true;
        }
    }
}
