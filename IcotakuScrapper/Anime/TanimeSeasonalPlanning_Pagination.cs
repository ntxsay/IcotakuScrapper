using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Text;

namespace IcotakuScrapper.Anime;

public partial class TanimeSeasonalPlanning
{
    #region Selection Mode

    public static async IAsyncEnumerable<ItemGroupCountStruct> CountAndGroupBySelectionMode(SeasonalAnimeSelectionMode selectionMode, WeatherSeason season, OrderBy orderBy = OrderBy.Asc,
        bool? isAdultContent = false, bool? isExplicitContent = false, CancellationToken? cancellationToken = null)
    {
        if (selectionMode == SeasonalAnimeSelectionMode.None)
            yield break;
        
        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

        switch (selectionMode)
        {
            case SeasonalAnimeSelectionMode.OrigineAdaptation:
                command.CommandText =
                    """
                    SELECT
                        TorigineAdaptation.Name AS ItemName,
                        TorigineAdaptation.Id AS ItemData,
                        TorigineAdaptation.Description AS ItemDescription,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM
                        main.TanimeSeasonalPlanning
                    LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = TanimeSeasonalPlanning.IdOrigine
                    WHERE
                        TanimeSeasonalPlanning.IdSeason = (SELECT Id FROM main.Tseason WHERE SeasonNumber = $SeasonNumber)
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            case SeasonalAnimeSelectionMode.Season:
                command.CommandText =
                    """
                    SELECT
                        Tseason.DisplayName AS ItemName,
                        Tseason.SeasonNumber AS ItemData,
                        'Recherche les animes dont la saison de diffusion est « ' || Tseason.DisplayName || ' »' AS ItemDescription,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.Tseason
                    LEFT JOIN main.TanimeSeasonalPlanning on TanimeSeasonalPlanning.IdSeason = Tseason.Id
                    WHERE
                        Tseason.SeasonNumber = $SeasonNumber
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            case SeasonalAnimeSelectionMode.Category:
                command.CommandText =
                    """
                    SELECT
                        Tcategory.Name AS ItemName,
                        Tcategory.Id AS ItemData,
                        Tcategory.Description AS ItemDescription,
                        Tanime.SheetId AS AnimeSheetId,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    LEFT JOIN main.Tanime on Tanime.SheetId = TanimeSeasonalPlanning.SheetId
                    LEFT JOIN main.TanimeCategory on TanimeCategory.IdAnime = Tanime.Id
                    LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
                    WHERE
                        TanimeSeasonalPlanning.IdSeason = (SELECT Id FROM main.Tseason WHERE SeasonNumber = $SeasonNumber)
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            case SeasonalAnimeSelectionMode.ReleaseMonth:
                command.CommandText =
                    """
                    SELECT
                        TanimeSeasonalPlanning.ReleaseMonth AS ItemName,
                        TanimeSeasonalPlanning.ReleaseMonth AS ItemData,
                        'Recherche les animes dont le mois de diffusion est celui-ci ' AS ItemDescription,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    WHERE
                        TanimeSeasonalPlanning.IdSeason = (SELECT Id FROM main.Tseason WHERE SeasonNumber = $SeasonNumber)
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            case SeasonalAnimeSelectionMode.GroupName:
                command.CommandText =
                    """
                    SELECT
                        TanimeSeasonalPlanning.GroupName AS ItemName,
                        TanimeSeasonalPlanning.GroupName AS ItemData,
                        'Recherche les animes dont le format est « ' || TanimeSeasonalPlanning.GroupName AS ItemDescription,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    WHERE
                        TanimeSeasonalPlanning.IdSeason = (SELECT Id FROM main.Tseason WHERE SeasonNumber = $SeasonNumber)
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            case SeasonalAnimeSelectionMode.Letter:
                command.CommandText =
                    """
                    SELECT
                        UPPER(SUBSTR(AnimeName, 1, 1)) AS ItemName,
                        UPPER(SUBSTR(AnimeName, 1, 1)) AS ItemData,
                        'Recherche les animes dont la première lettre commence par « ' || UPPER(SUBSTR(AnimeName, 1, 1)) || ' »' AS ItemDescription,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    WHERE
                        TanimeSeasonalPlanning.IdSeason = (SELECT Id FROM main.Tseason WHERE SeasonNumber = $SeasonNumber)
                    """;
                command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null);
        }

        command.AddExplicitContentFilter(DbStartFilterMode.And, "TanimeSeasonalPlanning.IsAdultContent", "TanimeSeasonalPlanning.IsExplicitContent", isAdultContent, isExplicitContent);
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
            var itemName = reader.IsDBNull(reader.GetOrdinal("ItemName")) ? null : reader.GetString(reader.GetOrdinal("ItemName"));
            var itemDescription = reader.IsDBNull(reader.GetOrdinal("ItemDescription")) ? null : reader.GetString(reader.GetOrdinal("ItemDescription"));
            var itemData = reader.IsDBNull(reader.GetOrdinal("ItemData")) ? null : reader.GetValue(reader.GetOrdinal("ItemData"));
            var itemCount = reader.GetInt32(reader.GetOrdinal("ItemCount"));
            
            //var y = reader.IsDBNull(reader.GetOrdinal("AnimeSheetId")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("AnimeSheetId"));
            
            if (itemName == null || itemName.IsStringNullOrEmptyOrWhiteSpace() || 
                itemData == null)
                continue;
            
            var groupName = selectionMode switch
            {
                SeasonalAnimeSelectionMode.OrigineAdaptation => "Origine de l'adaptation",
                SeasonalAnimeSelectionMode.Season => "Saison",
                SeasonalAnimeSelectionMode.ReleaseMonth => "Date de diffusion",
                SeasonalAnimeSelectionMode.GroupName => "Format",
                SeasonalAnimeSelectionMode.Letter => "Lettre",
                SeasonalAnimeSelectionMode.None => "Aucun",
                SeasonalAnimeSelectionMode.Category => "Catégories",
                _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
            };
            
            yield return new ItemGroupCountStruct
            {
                IdentifierKind = ConvertSelectionModeToItemGroupCountKind(selectionMode),
                GroupName = groupName,
                Name = ConvertItemCountName(selectionMode, itemName),
                Data = ConvertItemCountData(selectionMode, itemData),
                Description = itemDescription,
                Count = itemCount
            };

        }
    }
    private static ItemGroupCountKind ConvertSelectionModeToItemGroupCountKind(SeasonalAnimeSelectionMode selectionMode)
    {
        return selectionMode switch
        {
            SeasonalAnimeSelectionMode.OrigineAdaptation => ItemGroupCountKind.OrigineAdaptation,
            SeasonalAnimeSelectionMode.Season => ItemGroupCountKind.Season,
            SeasonalAnimeSelectionMode.ReleaseMonth => ItemGroupCountKind.ReleaseMonth,
            SeasonalAnimeSelectionMode.GroupName => ItemGroupCountKind.GroupName,
            SeasonalAnimeSelectionMode.Letter => ItemGroupCountKind.AnimeLetter,
            SeasonalAnimeSelectionMode.None => ItemGroupCountKind.None,
            SeasonalAnimeSelectionMode.Category => ItemGroupCountKind.Category,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
        };
    }
    
    private static string ConvertItemCountName(SeasonalAnimeSelectionMode selectionMode, string? value)
    {
        if (value == null || value.IsStringNullOrEmptyOrWhiteSpace())
            return selectionMode switch
            {
                SeasonalAnimeSelectionMode.OrigineAdaptation => "Origine inconnue",
                SeasonalAnimeSelectionMode.Season => "Saison inconnue",
                SeasonalAnimeSelectionMode.ReleaseMonth => "Mois inconnu",
                SeasonalAnimeSelectionMode.GroupName => "Groupe inconnu",
                SeasonalAnimeSelectionMode.Letter => "Caractère inconnu",
                SeasonalAnimeSelectionMode.None => "Aucun",
                SeasonalAnimeSelectionMode.Category => "Catégorie inconnue",
                _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
            };

        return selectionMode switch
        {
            SeasonalAnimeSelectionMode.OrigineAdaptation => value,
            SeasonalAnimeSelectionMode.Season => value,
            SeasonalAnimeSelectionMode.ReleaseMonth => DateHelpers.GetYearMonthLiteral(uint.Parse(value)) ?? "Mois inconnu",
            SeasonalAnimeSelectionMode.GroupName => value,
            SeasonalAnimeSelectionMode.Letter => value,
            SeasonalAnimeSelectionMode.None => "Aucun",
            SeasonalAnimeSelectionMode.Category => value,
            _ => throw new ArgumentOutOfRangeException(nameof(selectionMode), selectionMode, null)
        };
    }
    
    private static object? ConvertItemCountData(SeasonalAnimeSelectionMode selectionMode, object? value)
    {
        if (value == null)
            return null;

        switch (selectionMode)
        {
            case SeasonalAnimeSelectionMode.OrigineAdaptation:
                if (int.TryParse(value.ToString(), out var id))
                    return id;
                break;
            case SeasonalAnimeSelectionMode.Season:
                if (uint.TryParse(value.ToString(), out var seasonNumber))
                    return seasonNumber;
                break;
            case SeasonalAnimeSelectionMode.ReleaseMonth:
                if (uint.TryParse(value.ToString(), out var releaseMonth))
                    return releaseMonth;
                break;
            case SeasonalAnimeSelectionMode.GroupName:
                return value.ToString();
            case SeasonalAnimeSelectionMode.Letter:
                return value.ToString()?.FirstOrDefault();
            case SeasonalAnimeSelectionMode.None:
            case SeasonalAnimeSelectionMode.Category:
                if (int.TryParse(value.ToString(), out var idCategory))
                    return idCategory;
                break;
        }
        
        return null;
    }
    

    #endregion
    
    public static async Task<Paginate<TanimeSeasonalPlanning>> PaginateAsync(SeasonalAnimePlanningOptions options,
        uint currentPage = 1, uint maxContentByPage = 20,
        SeasonalAnimePlanningSortBy sortBy = SeasonalAnimePlanningSortBy.ReleaseMonth,
        OrderBy orderBy = OrderBy.Asc,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        int totalItems = 0;
        _ = GetSqlSelectScript(command, DbScriptMode.Count, options, sortBy, orderBy);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            totalItems = (int)count;

        if (totalItems <= 0)
            return new Paginate<TanimeSeasonalPlanning>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var totalPages = (uint)ExtensionMethods.CountPage(totalItems, (int)maxContentByPage);


        _ = GetSqlSelectScript(command, DbScriptMode.Select, options, sortBy, orderBy);
        command.AddPagination(currentPage, maxContentByPage);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return new Paginate<TanimeSeasonalPlanning>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var records = await GetRecords(reader, cancellationToken).ToArrayAsync();
        if (records.Length == 0)
            return new Paginate<TanimeSeasonalPlanning>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        return new Paginate<TanimeSeasonalPlanning>(
                currentPage: currentPage,
                totalPages: totalPages,
                maxItemsPerPage: maxContentByPage,
                totalItems: (uint)totalItems,
                items: records);
    }

    private static string GetSqlSelectScript(SqliteCommand command, DbScriptMode scriptMode,
        SeasonalAnimePlanningOptions options,
        SeasonalAnimePlanningSortBy sortBy = SeasonalAnimePlanningSortBy.ReleaseMonth,
        OrderBy orderBy = OrderBy.Asc)
    {
        var sqlScript = scriptMode switch
        {
            DbScriptMode.Select => SqlSelectScript,
            DbScriptMode.Count => SqlCountScript,
            _ => throw new ArgumentOutOfRangeException(nameof(scriptMode), scriptMode,
                "Le mode de script n'est pas supporté")
        };

        if (options.HasKeyword)
            sqlScript += Environment.NewLine +
                         $"WHERE (TanimeSeasonalPlanning.AnimeName LIKE $Keyword COLLATE NOCASE OR TanimeSeasonalPlanning.Description LIKE $Keyword COLLATE NOCASE)";


        if (options.HasMinSeason || options.HasMaxSeason)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";


            if (options is { HasMinSeason: true, HasMaxSeason: true })
                sqlScript += "(Tseason.SeasonNumber BETWEEN $MinSeason AND $MaxSeason)";
            else if (options.HasMinSeason)
                sqlScript += "Tseason.SeasonNumber >= $MinSeason";
            else if (options.HasMaxSeason)
                sqlScript += "Tseason.SeasonNumber <= $MaxSeason";
            else
                sqlScript += "";
        }

        if (options.IsAdultContent != null)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "TanimeSeasonalPlanning.IsAdultContent = $IsAdultContent";
        }

        if (options.IsExplicitContent != null)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "TanimeSeasonalPlanning.IsExplicitContent = $IsExplicitContent";
        }

        if (options.HasMinReleaseMonth || options.HasMaxReleaseMonth)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            if (options is { HasMinReleaseMonth: true, HasMaxReleaseMonth: true })
                sqlScript += "(TanimeSeasonalPlanning.ReleaseMonth BETWEEN $MinReleaseMonth AND $MaxReleaseMonth)";
            else if (options.HasMinReleaseMonth)
                sqlScript += "TanimeSeasonalPlanning.ReleaseMonth >= $MinReleaseMonth";
            else if (options.HasMaxReleaseMonth)
                sqlScript += "TanimeSeasonalPlanning.ReleaseMonth <= $MaxReleaseMonth";
            else
                sqlScript += "";
        }

        if (options.HasIdOrigineAdaptationToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdOrigine IN ({string.Join(',', options.IdOrigineAdaptationToInclude)})";
        }

        if (options.HasIdOrigineAdaptationToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdOrigine NOT IN ({string.Join(',', options.IdOrigineAdaptationToExclude)})";
        }

        if (options.HasIdDistributorsToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdDistributor IN ({string.Join(',', options.IdDistributorsToInclude)})";
        }

        if (options.HasIdDistributorsToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdDistributor NOT IN ({string.Join(',', options.IdDistributorsToExclude)})";
        }

        if (options.HasIdStudiosToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdStudio IN ({string.Join(',', options.IdStudiosToInclude)})";
        }

        if (options.HasIdStudiosToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdStudio NOT IN ({string.Join(',', options.IdStudiosToExclude)})";
        }

        if (options.HasThumbnail != null)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            if (options.HasThumbnail.Value)
                sqlScript += "TanimeSeasonalPlanning.ThumbnailUrl IS NOT NULL";
            else
                sqlScript += "TanimeSeasonalPlanning.ThumbnailUrl IS NULL";
        }

        if (options.HasGroupName)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "TanimeSeasonalPlanning.GroupName = $GroupName COLLATE NOCASE";
        }

        FilterSelection(command, ref sqlScript, options.ItemGroupCountData);

        sqlScript += Environment.NewLine;
        sqlScript += sortBy switch
        {
            SeasonalAnimePlanningSortBy.Id => $"ORDER BY TanimeSeasonalPlanning.Id {orderBy}",
            SeasonalAnimePlanningSortBy.Season => $"ORDER BY Tseason.SeasonNumber {orderBy}",
            SeasonalAnimePlanningSortBy.ReleaseMonth => $"ORDER BY TanimeSeasonalPlanning.ReleaseMonth {orderBy}",
            SeasonalAnimePlanningSortBy.AnimeName => $"ORDER BY TanimeSeasonalPlanning.AnimeName {orderBy}",
            SeasonalAnimePlanningSortBy.GroupName => $"ORDER BY TanimeSeasonalPlanning.GroupName {orderBy}",
            SeasonalAnimePlanningSortBy.SheetId => $"ORDER BY TanimeSeasonalPlanning.SheetId {orderBy}",
            SeasonalAnimePlanningSortBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };

        command.CommandText = sqlScript;
        command.Parameters.Clear();

        if (options.HasKeyword)
            command.Parameters.AddWithValue("$Keyword", $"%{options.Keyword}%");

        if (options.HasMinSeason)
            command.Parameters.AddWithValue("$MinSeason", options.MinSeason.ToIntSeason());

        if (options.HasMaxSeason)
            command.Parameters.AddWithValue("$MaxSeason", options.MaxSeason.ToIntSeason());

        if (options.IsAdultContent != null)
            command.Parameters.AddWithValue("$IsAdultContent", options.IsAdultContent.Value ? 1 : 0);

        if (options.IsExplicitContent != null)
            command.Parameters.AddWithValue("$IsExplicitContent", options.IsExplicitContent.Value ? 1 : 0);

        if (options.HasMinReleaseMonth)
            command.Parameters.AddWithValue("$MinReleaseMonth", options.MinReleaseMonth);

        if (options.HasMaxReleaseMonth)
            command.Parameters.AddWithValue("$MaxReleaseMonth", options.MaxReleaseMonth);

        if (options.HasGroupName)
            command.Parameters.AddWithValue("$GroupName", options.GroupName);

        return sqlScript;
    }

    private static void FilterSelection(SqliteCommand command, ref string sqlScript, IReadOnlyCollection<ItemGroupCountStruct> groups)
    {
        //Si la requête est vide, on ne fait rien
        if (sqlScript.IsStringNullOrEmptyOrWhiteSpace())
            return;
        
        //Si la liste est vide, on ne fait rien
        if (groups.Count == 0)
            return;
        
        //Si la liste contient plus d'un élément, on ne fait rien
        var isSame = groups.Select(x => x.IdentifierKind).Distinct().Count() == 1;
        if (!isSame)
        {
            LogServices.LogDebug("Les groupes ne sont pas du même type");
            return;
        }

        //On récupère le type de données
        var kind = groups.First().IdentifierKind;
        //Si le type de données est None, on ne fait rien
        if (kind == ItemGroupCountKind.None)
            return;
        
        

        var sqlPart = "";
        switch (kind)
        {
            case ItemGroupCountKind.OrigineAdaptation:
                var origineData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (origineData.Length == 0)
                    return;
                sqlPart += $"(TanimeSeasonalPlanning.IdOrigine IN ({string.Join(',', origineData)}))";
                break;
            case ItemGroupCountKind.Season:
                var seasonData = groups.Select(x => x.Data).OfType<uint>().ToArray();
                if (seasonData.Length == 0)
                    return;
                sqlPart += $"(Tseason.SeasonNumber IN ({string.Join(',', seasonData)}))";
                break;
            case ItemGroupCountKind.ReleaseMonth:
                var releaseMonthData = groups.Select(x => x.Data).OfType<uint>().ToArray();
                if (releaseMonthData.Length == 0)
                    return;
                sqlPart += $"(TanimeSeasonalPlanning.ReleaseMonth IN ({string.Join(',', releaseMonthData)}))";
                break;
            case ItemGroupCountKind.GroupName:
                var groupNameData = groups.Select(x => x.Data).OfType<string>().ToArray();
                if (groupNameData.Length == 0)
                    return;
                sqlPart += $"(TanimeSeasonalPlanning.GroupName IN ({string.Join(',', groupNameData.Select(s => $"'{s}'"))}))";
                break;
            case ItemGroupCountKind.AnimeLetter:
                var letterData = groups.Select(x => x.Data).OfType<char>().ToArray();
                if (letterData.Length == 0)
                    return;
                var letterCondition = new StringBuilder();
                letterCondition.AppendLine();
                letterCondition.AppendLine($"TanimeSeasonalPlanning.AnimeName COLLATE NOCASE LIKE '{letterData[0]}%'");
                foreach (var value in letterData[1..])
                    letterCondition.Append($" OR TanimeSeasonalPlanning.AnimeName COLLATE NOCASE LIKE '{value}%'");
                sqlPart += $"({letterCondition})";
                break;
            case ItemGroupCountKind.None:
                break;
            case ItemGroupCountKind.Category:
                var categoryData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (categoryData.Length == 0)
                    return;
                sqlPart += $"(Tcategory.Id IN ({string.Join(',', categoryData)}))";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (sqlPart.IsStringNullOrEmptyOrWhiteSpace())
            return;
        
        if (sqlScript.Contains("WHERE"))
            sqlScript += Environment.NewLine + "AND ";
        else
            sqlScript += Environment.NewLine + "WHERE ";
        
        sqlScript += sqlPart;
    }
}


public readonly struct SeasonalAnimePlanningOptions
{
    public string? Keyword { get; init; } = null;
    public string? GroupName { get; init; } = null;
    public bool? IsAdultContent { get; init; }
    public bool? IsExplicitContent { get; init; }
    public bool? HasThumbnail { get; init; }
    public WeatherSeason MinSeason { get; init; }
    public WeatherSeason MaxSeason { get; init; }
    public uint MinReleaseMonth { get; init; }
    public uint MaxReleaseMonth { get; init; }

    public ItemGroupCountStruct[] ItemGroupCountData { get; init; } = [];

    public HashSet<int> IdOrigineAdaptationToInclude { get; init; } = [];
    public HashSet<int> IdOrigineAdaptationToExclude { get; init; } = [];
    public HashSet<int> IdDistributorsToInclude { get; init; } = [];
    public HashSet<int> IdDistributorsToExclude { get; init; } = [];
    public HashSet<int> IdStudiosToInclude { get; init; } = [];
    public HashSet<int> IdStudiosToExclude { get; init; } = [];

    public bool HasMinSeason => !MinSeason.Equals(default(WeatherSeason)) && MinSeason.Season != WeatherSeasonKind.Unknown && MinSeason.Year > 0;
    public bool HasMaxSeason => !MaxSeason.Equals(default(WeatherSeason)) && MaxSeason.Season != WeatherSeasonKind.Unknown && MaxSeason.Year > 0;
    public bool HasMinReleaseMonth => MinReleaseMonth > 0;
    public bool HasMaxReleaseMonth => MaxReleaseMonth > 0;
    public bool HasKeyword => Keyword != null && !Keyword.IsStringNullOrEmptyOrWhiteSpace();

    public bool HasIdOrigineAdaptationToInclude => IdOrigineAdaptationToInclude.Count > 0;
    public bool HasIdOrigineAdaptationToExclude => IdOrigineAdaptationToExclude.Count > 0;

    public bool HasIdDistributorsToInclude => IdDistributorsToInclude.Count > 0;
    public bool HasIdDistributorsToExclude => IdDistributorsToExclude.Count > 0;

    public bool HasIdStudiosToInclude => IdStudiosToInclude.Count > 0;
    public bool HasIdStudiosToExclude => IdStudiosToExclude.Count > 0;
    public bool HasGroupName => GroupName != null && !GroupName.IsStringNullOrEmptyOrWhiteSpace();

    public SeasonalAnimePlanningOptions()
    {
    }
}