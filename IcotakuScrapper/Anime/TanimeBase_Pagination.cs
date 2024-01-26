using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;
using IcotakuScrapper.Objects.Models;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
        #region Selection Mode

        /// <summary>
        /// Retourne des groupe d'éléments en fonction du mode de sélection et affiche le nombre d'éléments par groupe.
        /// </summary>
        /// <param name="selectionMode"></param>
        /// <param name="scopeData"></param>
        /// <param name="orderBy"></param>
        /// <param name="isAdultContent"></param>
        /// <param name="isExplicitContent"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="scopeKind"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static async IAsyncEnumerable<AnimeItemGroupCountStruct> CountAndGroupBySelectionMode(AnimeSelectionMode selectionMode, AnimeItemGroupCountScopeKind scopeKind, object? scopeData, OrderBy orderBy = OrderBy.Asc,
        bool? isAdultContent = false, bool? isExplicitContent = false, CancellationToken? cancellationToken = null)
    {
        if (selectionMode == AnimeSelectionMode.None)
            yield break;
        
        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

        switch (selectionMode)
        {
            case AnimeSelectionMode.OrigineAdaptation:
                command.CommandText =
                    """
                    SELECT
                        TorigineAdaptation.Name AS ItemName,
                        TorigineAdaptation.Id AS ItemData,
                        TorigineAdaptation.Description AS ItemDescription,
                        'Origine de l''adaptation' AS GroupName,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM
                        main.Tanime
                    LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
                    """;
                break;
            case AnimeSelectionMode.Season:
                command.CommandText =
                    """
                    SELECT
                        Tseason.DisplayName AS ItemName,
                        Tseason.SeasonNumber AS ItemData,
                        'Recherche les animes dont la saison de diffusion est « ' || Tseason.DisplayName || ' »' AS ItemDescription,
                        'Saison' AS GroupName,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM main.Tseason
                    LEFT JOIN main.Tanime on Tanime.IdSeason = Tseason.Id
                    """;
                break;
            case AnimeSelectionMode.Category:
                command.CommandText =
                    """
                    SELECT
                        Tcategory.Name AS ItemName,
                        Tcategory.Id AS ItemData,
                        Tcategory.Description AS ItemDescription,
                        CASE
                            WHEN Tcategory.Type = 0 THEN 'Thème'
                            WHEN Tcategory.Type = 1 THEN 'Genre'
                        END AS GroupName,
                        Tanime.SheetId AS AnimeSheetId,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM main.Tanime
                    LEFT JOIN main.TanimeCategory on TanimeCategory.IdAnime = Tanime.Id
                    LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
                    """;
                break;
            case AnimeSelectionMode.ReleaseMonth:
                command.CommandText =
                    """
                    SELECT
                        Tanime.ReleaseMonth AS ItemName,
                        Tanime.ReleaseMonth AS ItemData,
                        'Recherche les animes dont le mois de diffusion est celui-ci ' AS ItemDescription,
                        'Date de diffusion' AS GroupName,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM main.Tanime
                    """;
                break;
            case AnimeSelectionMode.Format:
                command.CommandText =
                    """
                    SELECT
                        Tformat.Name AS ItemName,
                        Tformat.Id AS ItemData,
                        Tformat.Description AS ItemDescription,
                        'Format' AS GroupName,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM main.Tanime
                    LEFT JOIN main.Tformat on Tanime.IdFormat = Tformat.Id
                    """;
                break;
            case AnimeSelectionMode.Letter:
                command.CommandText =
                    """
                    SELECT
                        UPPER(SUBSTR(Tanime.Name, 1, 1)) AS ItemName,
                        UPPER(SUBSTR(Tanime.Name, 1, 1)) AS ItemData,
                        'Recherche les animes dont la première lettre commence par « ' || UPPER(SUBSTR(Tanime.Name, 1, 1)) || ' »' AS ItemDescription,
                        'Lettre' AS GroupName,
                        COUNT(Tanime.Id) AS ItemCount
                    FROM main.Tanime
                    """;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null);
        }

        command.CommandText += Environment.NewLine + scopeKind switch
        {
            AnimeItemGroupCountScopeKind.None => "",
            AnimeItemGroupCountScopeKind.Season => 
                "WHERE Tanime.IdSeason = (SELECT Id FROM Tseason WHERE Tseason.SeasonNumber = $SeasonNumber)",
            AnimeItemGroupCountScopeKind.Day => "WHERE Tanime.ReleaseDate = $ReleaseDate",
            _ => throw new ArgumentOutOfRangeException(nameof(scopeKind), scopeKind, null)
        };

        switch (scopeKind)
        {
            case AnimeItemGroupCountScopeKind.None:
                break;
            case AnimeItemGroupCountScopeKind.Season:
                if (scopeData is WeatherSeason season)
                    command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                else
                    throw new InvalidOperationException(
                        $"Zone attendue est {nameof(AnimeItemGroupCountScopeKind.Season)} alors que l'argument n'est pas du type {nameof(WeatherSeason)}");
                break;
            case AnimeItemGroupCountScopeKind.Day:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scopeKind), scopeKind, null);
        }
        
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", isAdultContent, isExplicitContent);
        command.CommandText += Environment.NewLine + 
                               $"""
                                GROUP BY ItemName
                                ORDER BY ItemName {orderBy}
                                """;


        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            yield break;

        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var groupName = reader.IsDBNull(reader.GetOrdinal("GroupName")) ? null : reader.GetString(reader.GetOrdinal("GroupName"));
            var itemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? null : reader.GetString(reader.GetOrdinal("ItemName"));
            var itemDescription = reader.IsDBNull(reader.GetOrdinal("ItemDescription")) ? null : reader.GetString(reader.GetOrdinal("ItemDescription"));
            var itemData = reader.IsDBNull(reader.GetOrdinal("ItemData")) ? null : reader.GetValue(reader.GetOrdinal("ItemData"));
            var itemCount = reader.GetInt32(reader.GetOrdinal("ItemCount"));
            
            if (itemName == null || itemName.IsStringNullOrEmptyOrWhiteSpace() || 
                itemData == null)
                continue;
            
            /*var groupName = selectionMode switch
            {
                AnimeSelectionMode.OrigineAdaptation => "Origine de l'adaptation",
                AnimeSelectionMode.Season => "Saison",
                AnimeSelectionMode.ReleaseMonth => "Date de diffusion",
                AnimeSelectionMode.GroupName => "Format",
                AnimeSelectionMode.Letter => "Lettre",
                AnimeSelectionMode.None => "Aucun",
                AnimeSelectionMode.Category => "Catégories",
                _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
            };*/
            
            yield return new AnimeItemGroupCountStruct()
            {
                IdentifierKind = ConvertSelectionModeToItemGroupCountKind(selectionMode),
                GroupName = groupName ?? "Groupe inconnu",
                Name = ConvertItemCountName(selectionMode, itemName),
                Data = ConvertItemCountData(selectionMode, itemData),
                Description = itemDescription,
                Count = itemCount
            };

        }
    }
    private static AnimeItemGroupCountKind ConvertSelectionModeToItemGroupCountKind(AnimeSelectionMode selectionMode)
    {
        return selectionMode switch
        {
            AnimeSelectionMode.OrigineAdaptation => AnimeItemGroupCountKind.OrigineAdaptation,
            AnimeSelectionMode.Season => AnimeItemGroupCountKind.Season,
            AnimeSelectionMode.ReleaseMonth => AnimeItemGroupCountKind.ReleaseMonth,
            AnimeSelectionMode.Format => AnimeItemGroupCountKind.Format,
            AnimeSelectionMode.Letter => AnimeItemGroupCountKind.AnimeLetter,
            AnimeSelectionMode.None => AnimeItemGroupCountKind.None,
            AnimeSelectionMode.Category => AnimeItemGroupCountKind.Category,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
        };
    }
    
    private static string ConvertItemCountName(AnimeSelectionMode selectionMode, string? value)
    {
        if (value == null || value.IsStringNullOrEmptyOrWhiteSpace())
            return selectionMode switch
            {
                AnimeSelectionMode.OrigineAdaptation => "Origine inconnue",
                AnimeSelectionMode.Season => "Saison inconnue",
                AnimeSelectionMode.ReleaseMonth => "Mois inconnu",
                AnimeSelectionMode.Format => "Format inconnu",
                AnimeSelectionMode.Letter => "Caractère inconnu",
                AnimeSelectionMode.None => "Aucun",
                AnimeSelectionMode.Category => "Catégorie inconnue",
                _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
            };

        return selectionMode switch
        {
            AnimeSelectionMode.OrigineAdaptation => value,
            AnimeSelectionMode.Season => value,
            AnimeSelectionMode.ReleaseMonth => DateHelpers.GetYearMonthLiteral(uint.Parse(value)) ?? "Mois inconnu",
            AnimeSelectionMode.Format => value,
            AnimeSelectionMode.Letter => value,
            AnimeSelectionMode.None => "Aucun",
            AnimeSelectionMode.Category => value,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
        };
    }
    
    private static object? ConvertItemCountData(AnimeSelectionMode selectionMode, object? value)
    {
        if (value == null)
            return null;

        switch (selectionMode)
        {
            case AnimeSelectionMode.OrigineAdaptation:
                if (int.TryParse(value.ToString(), out var id))
                    return id;
                break;
            case AnimeSelectionMode.Season:
                if (uint.TryParse(value.ToString(), out var seasonNumber))
                    return seasonNumber;
                break;
            case AnimeSelectionMode.ReleaseMonth:
                if (uint.TryParse(value.ToString(), out var releaseMonth))
                    return releaseMonth;
                break;
            case AnimeSelectionMode.Format:
                return value.ToString();
            case AnimeSelectionMode.Letter:
                return value.ToString()?.FirstOrDefault();
            case AnimeSelectionMode.None:
            case AnimeSelectionMode.Category:
                if (int.TryParse(value.ToString(), out var idCategory))
                    return idCategory;
                break;
        }
        
        return null;
    }
    

    #endregion

    
    /// <summary>
    /// Ajoute la clause Where ou And à la commande SQL pour la commencer ou la poursuivre.
    /// </summary>
    /// <param name="sqlScript"></param>
    private static void AddWhereOrAndClause(ref string sqlScript)
    {
        if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
            sqlScript += Environment.NewLine + "AND ";
        else
            sqlScript += Environment.NewLine + "WHERE ";
    }
    
}