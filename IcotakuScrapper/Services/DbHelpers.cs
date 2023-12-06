using IcotakuScrapper.Anime;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Services
{
    /// <summary>
    /// Classe statique contenant des méthodes permettant de faciliter manipuler la base de données.
    /// </summary>
    internal static class DbHelpers
    {
        internal static string? ConvertGuidToStringSqlite(Guid _guid)
        {
            return _guid.ToString("N")?.ToUpper();
        }
        
        internal static Guid ConvertStringSqliteToGuid(string sqliteStringGuid)
        {
            if (sqliteStringGuid.IsStringNullOrEmptyOrWhiteSpace())
                return Guid.Empty;
            
            var isGuidCorrect = Guid.TryParse(sqliteStringGuid, out var guid);
            return !isGuidCorrect ? Guid.Empty : guid;
        }

        internal static async Task<int> GetLastInsertRowIdAsync(SqliteCommand command)
        {
            command.CommandText = "SELECT last_insert_rowid()";
            var value = await command.ExecuteScalarAsync();
            if (value is long id)
                return (int)id;

            throw new InvalidOperationException("Impossible de récupérer l'identifiant de la ligne insérée");
        }

        internal static void AddPagination(SqliteCommand command, uint currentPage = 1, uint maxContentByPage = 20)
        {
            var skipCount = (currentPage - 1) * maxContentByPage;
            command.CommandText += Environment.NewLine;
            command.AddLimitOffset(maxContentByPage, skipCount);
        }

        internal static void AddLimitOffset(SqliteCommand command, uint limit, uint offset)
        {
            if (limit > 0)
                command.CommandText += Environment.NewLine + $"LIMIT {limit}";

            if (offset > 0)
                command.CommandText += $" OFFSET {offset}";
        }

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

        public static void AddOrderSort(SqliteCommand command, SeasonalAnimePlanningSortBy sortBy, OrderBy orderBy)
        {
            command.CommandText += Environment.NewLine + sortBy switch
            {
                SeasonalAnimePlanningSortBy.Id => $"ORDER BY TanimeSeasonalPlanning.Id {orderBy}",
                SeasonalAnimePlanningSortBy.SheetId => $"ORDER BY TanimeSeasonalPlanning.SheetId {orderBy}",
                SeasonalAnimePlanningSortBy.ReleaseMonth => $"ORDER BY TanimeSeasonalPlanning.ReleaseMonth {orderBy}",
                SeasonalAnimePlanningSortBy.AnimeName => $"ORDER BY TanimeSeasonalPlanning.AnimeName {orderBy}",
                SeasonalAnimePlanningSortBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {orderBy}",
                SeasonalAnimePlanningSortBy.Season => $"ORDER BY Tseason.SeasonNumber {orderBy}",
                SeasonalAnimePlanningSortBy.GroupName => $"ORDER BY TanimeSeasonalPlanning.GroupName {orderBy}",
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

        /// <summary>
        /// Ajoute le filtrage du contenu adulte et explicite à la commande SQL.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="startFilterMode"></param>
        /// <param name="isAdultContent"></param>
        /// <param name="isExplicitContent"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void AddExplicitContentFilter(SqliteCommand command, DbStartFilterMode startFilterMode, string isAdultContentColumnName, string isExplicitContentColumnName, bool? isAdultContent = false, bool? isExplicitContent = false)
        {
            //Si l'utilisateur a accès au contenu adulte de manière globale...
            if (Main.IsAccessingToAdultContent)
            {
                //Et que l'utilisateur a aussi accès au contenu adulte via le paramètre de la méthode
                //Alors on commence par filtrer le contenu adulte
                if (isAdultContent.HasValue)
                    command.CommandText += Environment.NewLine + startFilterMode switch
                    {
                        DbStartFilterMode.Where => $"WHERE {isAdultContentColumnName} = $IsAdultContent",
                        DbStartFilterMode.And => $"AND {isAdultContentColumnName} = $IsAdultContent",
                        DbStartFilterMode.Or => $"OR {isAdultContentColumnName} = $IsAdultContent",
                        _ => throw new ArgumentOutOfRangeException(nameof(startFilterMode), startFilterMode, "La valeur spécifiée est invalide"),
                    };

                AddExplicitContent(command, isAdultContent.HasValue ? DbStartFilterMode.And : DbStartFilterMode.Where, isExplicitContentColumnName, isAdultContent, isExplicitContent);
            }
            else //Si l'utilisateur n'a pas accès au contenu adulte de manière globale ...
            {
                command.CommandText += Environment.NewLine + startFilterMode switch
                {
                    DbStartFilterMode.Where => $"WHERE {isAdultContentColumnName} = 0",
                    DbStartFilterMode.And => $"AND {isAdultContentColumnName} = 0",
                    DbStartFilterMode.Or => $"OR {isAdultContentColumnName} = 0",
                    _ => throw new ArgumentOutOfRangeException(nameof(startFilterMode), startFilterMode, "La valeur spécifiée est invalide"),
                };

                AddExplicitContent(command, DbStartFilterMode.And, isExplicitContentColumnName, false, isExplicitContent);
            }

            if (isAdultContent.HasValue)
                command.Parameters.AddWithValue("$IsAdultContent", isAdultContent.Value ? 1 : 0);
        }

        /// <summary>
        /// Ajoute le filtrage du contenu explicite à la commande SQL.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="startFilterMode"></param>
        /// <param name="isAdultContent"></param>
        /// <param name="isExplicitContent"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void AddExplicitContent(SqliteCommand command, DbStartFilterMode startFilterMode, string columnName, bool? isAdultContent = false, bool? isExplicitContent = false)
        {
            //Et que l'utilisateur a accès au contenu explicite de manière globale ...
            if (Main.IsAccessingToExplicitContent)
            {
                //Et que l'utilisateur a aussi accès au contenu explicite via le paramètre de la méthode
                if (isExplicitContent.HasValue)
                {
                    //Alors on poursuit par le filtrage du contenu explicite
                    if (isAdultContent.HasValue)
                        command.CommandText += Environment.NewLine + $"AND {columnName} = $IsExplicitContent";
                    //Alors on commence par filtrer le contenu explicite
                    else
                        command.CommandText += Environment.NewLine + startFilterMode switch
                        {
                            DbStartFilterMode.Where => $"WHERE {columnName} = $IsExplicitContent",
                            DbStartFilterMode.And => $"AND {columnName} = $IsExplicitContent",
                            DbStartFilterMode.Or => $"OR {columnName} = $IsExplicitContent",
                            _ => throw new ArgumentOutOfRangeException(nameof(startFilterMode), startFilterMode, "La valeur spécifiée est invalide"),
                        };
                }
            }
            else //Si l'utilisateur n'a pas accès au contenu explicite de manière globale ...
            {
                //Alors on poursuit par le filtrage du contenu explicite ignoré
                if (isAdultContent.HasValue)
                    command.CommandText += Environment.NewLine + "AND {columnName} = 0";

                //Alors on commence par le filtrage du contenu explicite ignoré
                else
                    command.CommandText += Environment.NewLine + startFilterMode switch
                    {
                        DbStartFilterMode.Where => "WHERE {columnName} = 0",
                        DbStartFilterMode.And => "AND {columnName} = 0",
                        DbStartFilterMode.Or => "OR {columnName} = 0",
                        _ => throw new ArgumentOutOfRangeException(nameof(startFilterMode), startFilterMode, "La valeur spécifiée est invalide"),
                    };
            }

            if (isExplicitContent.HasValue)
                command.Parameters.AddWithValue("$IsExplicitContent", isExplicitContent.Value ? 1 : 0);
        }

        public static bool IsIntColumnValidated(SqliteCommand command, IntColumnSelect currentSelectedColumn, HashSet<IntColumnSelect> acceptedColumns)
        {
            if (acceptedColumns.Count == 0)
            {
                LogServices.LogDebug("La liste des colonnes acceptées est vide.");
                command.CommandText = string.Empty;
                return false;
            }

            var any = acceptedColumns.Any(a => a == currentSelectedColumn);
            if (!any)
            {
                LogServices.LogDebug($"La colonne {currentSelectedColumn} n'est pas acceptée.");
                command.CommandText = string.Empty;
                return false;
            }

            return true;
        }
    }
}
