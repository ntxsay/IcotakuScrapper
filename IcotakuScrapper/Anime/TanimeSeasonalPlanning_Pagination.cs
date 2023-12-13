using System.ComponentModel;
using System.Runtime.CompilerServices;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class TanimeSeasonalPlanning
{
    public static async IAsyncEnumerable<ItemGroupCountStruct> GetItemsCountByLettersAsync(SeasonalAnimePlanningGroupBy groupBy, OrderBy orderBy = OrderBy.Asc, 
        bool? isAdultContent = false, bool? isExplicitContent = false, CancellationToken? cancellationToken = null)
    {
        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();

        switch (groupBy)
        {
            case SeasonalAnimePlanningGroupBy.Default:
                break;
            case SeasonalAnimePlanningGroupBy.OrigineAdaptation:
                command.CommandText =
                    """
                    SELECT
                        TorigineAdaptation.Name AS ItemName, 
                        TorigineAdaptation.Description,
                        COUNT(TanimeSeasonalPlanning.Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = TanimeSeasonalPlanning.IdOrigine
                    """;
                command.AddExplicitContentFilter(DbStartFilterMode.Where, "TanimeSeasonalPlanning.IsAdultContent", "TanimeSeasonalPlanning.IsExplicitContent", isAdultContent, isExplicitContent);
                command.CommandText += Environment.NewLine + "GROUP BY ItemName" + Environment.NewLine + $"ORDER BY ItemName {orderBy}";
                break;
            case SeasonalAnimePlanningGroupBy.Season:
                break;
            case SeasonalAnimePlanningGroupBy.ReleaseMonth:
                break;
            case SeasonalAnimePlanningGroupBy.GroupName:
                break;
            case SeasonalAnimePlanningGroupBy.Letter:
                command.CommandText =
                    """
                    SELECT
                        UPPER(SUBSTR(AnimeName, 1, 1)) AS ItemName,
                        COUNT(Id) AS ItemCount
                    FROM main.TanimeSeasonalPlanning
                    """;
                command.AddExplicitContentFilter(DbStartFilterMode.Where, "TanimeSeasonalPlanning.IsAdultContent", "TanimeSeasonalPlanning.IsExplicitContent", isAdultContent, isExplicitContent);
                command.CommandText += Environment.NewLine + "GROUP BY ItemName" + Environment.NewLine + $"ORDER BY ItemName {orderBy}";
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null);
        }
        
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            yield break;

        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new ItemGroupCountStruct
            {
                IdentifierKind = ItemGroupCountKind.AnimeLetter,
                Name = reader.GetString(reader.GetOrdinal("ItemName")),
                Count = (uint)reader.GetInt32(reader.GetOrdinal("ItemCount"))
            };
            
        }
    }

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
            if (options.HasKeyword)
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
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += "TanimeSeasonalPlanning.IsAdultContent = $IsAdultContent";
        }

        if (options.IsExplicitContent != null)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += "TanimeSeasonalPlanning.IsExplicitContent = $IsExplicitContent";
        }
        
        if (options.HasMinReleaseMonth || options.HasMaxReleaseMonth)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null || options.IsExplicitContent != null)
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
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdOrigine IN ({string.Join(',', options.IdOrigineAdaptationToInclude)})";
        }
        
        if (options.HasIdOrigineAdaptationToExclude)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdOrigine NOT IN ({string.Join(',', options.IdOrigineAdaptationToExclude)})";
        }
        
        if (options.HasIdDistributorsToInclude)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdDistributor IN ({string.Join(',', options.IdDistributorsToInclude)})";
        }
        
        if (options.HasIdDistributorsToExclude)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude || options.HasIdDistributorsToInclude)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdDistributor NOT IN ({string.Join(',', options.IdDistributorsToExclude)})";
        }
        
        if (options.HasIdStudiosToInclude)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude || options.HasIdDistributorsToInclude || options.HasIdDistributorsToExclude)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdStudio IN ({string.Join(',', options.IdStudiosToInclude)})";
        }
        
        if (options.HasIdStudiosToExclude)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude || options.HasIdDistributorsToInclude || options.HasIdDistributorsToExclude || options.HasIdStudiosToInclude)
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeSeasonalPlanning.IdStudio NOT IN ({string.Join(',', options.IdStudiosToExclude)})";
        }
        
        if (options.HasThumbnail != null)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude || options.HasIdDistributorsToInclude || options.HasIdDistributorsToExclude || options.HasIdStudiosToInclude || options.HasIdStudiosToExclude)
                sqlScript += Environment.NewLine + "AND ";
            sqlScript += Environment.NewLine + "WHERE ";

            if (options.HasThumbnail.Value)
                sqlScript += "TanimeSeasonalPlanning.ThumbnailUrl IS NOT NULL";
            else
                sqlScript += "TanimeSeasonalPlanning.ThumbnailUrl IS NULL";
        }

        if (options.HasGroupName)
        {
            if (options.HasKeyword || options.HasMinSeason || options.HasMaxSeason || options.IsAdultContent != null ||
                options.IsExplicitContent != null || options.HasMinReleaseMonth || options.HasMaxReleaseMonth || options.HasIdOrigineAdaptationToInclude || options.HasIdOrigineAdaptationToExclude || options.HasIdDistributorsToInclude || options.HasIdDistributorsToExclude || options.HasIdStudiosToInclude || options.HasIdStudiosToExclude || options.HasThumbnail != null)
                sqlScript += Environment.NewLine + "AND ";
            sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "TanimeSeasonalPlanning.GroupName = $GroupName COLLATE NOCASE";
        }


        sqlScript += Environment.NewLine;
        sqlScript += sortBy switch
        {
            SeasonalAnimePlanningSortBy.Id => $"ORDER BY TanimeSeasonalPlanning.Id {orderBy}",
            SeasonalAnimePlanningSortBy.Season => $"ORDER BY Tseason.SeasonNumber {orderBy}",
            SeasonalAnimePlanningSortBy.ReleaseMonth => $"ORDER BY TanimeSeasonalPlanning.ReleaseMonth {orderBy}",
            SeasonalAnimePlanningSortBy.AnimeName => $"ORDER BY TanimeSeasonalPlanning.AnimeName {orderBy}",
            SeasonalAnimePlanningSortBy.GroupName => $"ORDER BY TanimeSeasonalPlanning.GroupName {orderBy}",
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
    public HashSet<int> IdOrigineAdaptationToInclude { get; init; } = [];
    public HashSet<int> IdOrigineAdaptationToExclude { get; init; } = [];
    public HashSet<int> IdDistributorsToInclude { get; init; } = [];
    public HashSet<int> IdDistributorsToExclude { get; init; } = [];
    public HashSet<int> IdStudiosToInclude { get; init; } = [];
    public HashSet<int> IdStudiosToExclude { get; init; } = [];

    public bool HasMinSeason => MinSeason.Equals(default(WeatherSeason));
    public bool HasMaxSeason => MaxSeason.Equals(default(WeatherSeason));
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