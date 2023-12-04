using IcotakuScrapper.Anime;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Extensions
{
    internal static class DbExtensions
    {
        /// <summary>
        /// Retourne l'identifiant de la dernière ligne insérée dans la base de données.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static async Task<int> GetLastInsertRowIdAsync(this SqliteCommand command) => await DbHelpers.GetLastInsertRowIdAsync(command);

        /// <summary>
        ///Ajoute les clauses LIMIT et OFFSET à la commande SQL.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        internal static void AddLimitOffset(this SqliteCommand command, uint limit, uint offset) => DbHelpers.AddLimitOffset(command, limit, offset);

        public static void AddOrderSort(this SqliteCommand command, AnimeSeasonalPlanningSortBy sortBy, OrderBy orderBy)
            => DbHelpers.AddOrderSort(command, sortBy, orderBy);

        public static void AddOrderSort(this SqliteCommand command, AnimeDailyPlanningSortBy sortBy, OrderBy orderBy)
            => DbHelpers.AddOrderSort(command, sortBy, orderBy);

        public static void StartWithInsertMode(this SqliteCommand command, DbInsertMode insertMode)
            => DbHelpers.StartWithInsertMode(command, insertMode);

        public static bool IsIntColumnValidated(this SqliteCommand command, IntColumnSelect currentSelectedColumn, HashSet<IntColumnSelect> acceptedColumns)
            => DbHelpers.IsIntColumnValidated(command, currentSelectedColumn, acceptedColumns);
    }
}
