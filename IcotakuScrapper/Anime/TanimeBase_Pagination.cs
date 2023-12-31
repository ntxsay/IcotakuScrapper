﻿using System.Text;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    public static Paginate<TanimeBase> PaginateAsync(IReadOnlyCollection<TanimeBase> values,
        uint currentPage = 1, uint maxContentByPage = 20,
        AnimeSortBy sortBy = AnimeSortBy.Name,
        OrderBy orderBy = OrderBy.Asc)
    {
        int totalItems = values.Count;
        if (totalItems <= 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var totalPages = ExtensionMethods.CountPage((uint)totalItems, maxContentByPage);
        var paginatedValues = ExtensionMethods.GetPage(values, currentPage, maxContentByPage);
        if (paginatedValues.Length == 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        return new Paginate<TanimeBase>(
            currentPage: currentPage,
            totalPages: totalPages,
            maxItemsPerPage: maxContentByPage,
            totalItems: (uint)totalItems,
            items: paginatedValues);
    }
    
    public static async Task<Paginate<TanimeBase>> PaginateAsync(AnimeDbFinderOptions options,
        uint currentPage = 1, uint maxContentByPage = 20,
        AnimeSortBy sortBy = AnimeSortBy.Name,
        OrderBy orderBy = OrderBy.Asc, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        int totalItems = 0;
        _ = GetSqlSelectScript(Main.Command, DbScriptMode.Count, options, sortBy, orderBy);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            totalItems = (int)count;

        if (totalItems <= 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var totalPages = ExtensionMethods.CountPage((uint)totalItems, (uint)maxContentByPage);


        _ = GetSqlSelectScript(Main.Command, DbScriptMode.Select, options, sortBy, orderBy);
        command.AddPagination(currentPage, maxContentByPage);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var records = await GetRecords(reader, cancellationToken);
        if (records.Length == 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        return new Paginate<TanimeBase>(
            currentPage: currentPage,
            totalPages: totalPages,
            maxItemsPerPage: maxContentByPage,
            totalItems: (uint)totalItems,
            items: records);
    }

    private static string GetSqlSelectScript(SqliteCommand command, DbScriptMode scriptMode,
        AnimeDbFinderOptions options,
        AnimeSortBy sortBy = AnimeSortBy.Name,
        OrderBy orderBy = OrderBy.Asc)
    {
        var sqlScript = scriptMode switch
        {
            DbScriptMode.Select => IcotakuSqlSelectScript,
            DbScriptMode.Count => IcotakuSqlCountScript,
            _ => throw new ArgumentOutOfRangeException(nameof(scriptMode), scriptMode,
                "Le mode de script n'est pas supporté")
        };

        if (options.HasKeyword)
        {
            if (options.IsFindInTitles)
            {
                if (sqlScript.IsContainsWhereClause())
                    sqlScript += Environment.NewLine + "AND (";
                else
                    sqlScript += Environment.NewLine + "WHERE (";
                
                sqlScript += Environment.NewLine +
                             "Tanime.Name LIKE $Keyword COLLATE NOCASE OR TanimeAlternativeTitle.Title LIKE $Keyword COLLATE NOCASE ";
                
                if (options is { IsFindInDescription: false, IsFindInRemark: false })
                    sqlScript += ")";
            }
            
            if (options.IsFindInDescription)
            {
                if (sqlScript.IsContainsWhereClause())
                    sqlScript += Environment.NewLine + "OR ";
                else
                    sqlScript += Environment.NewLine + "WHERE (";
                sqlScript += Environment.NewLine +
                             "Tanime.Description LIKE $Keyword COLLATE NOCASE";
                
                if (!options.IsFindInRemark)
                    sqlScript += ")";
            }
        
            if (options.IsFindInRemark)
            {
                if (sqlScript.IsContainsWhereClause())
                    sqlScript += Environment.NewLine + "OR ";
                else
                    sqlScript += Environment.NewLine + "WHERE (";
                sqlScript += Environment.NewLine +
                             "Tanime.Remark LIKE $Keyword COLLATE NOCASE)";
            }
        }

        
        if (options.HasMinDate || options.HasMaxDate)
        {
            AddWhereOrAndClause(ref sqlScript);
            
            if (options is { HasMinDate: true, HasMaxDate: true })
                sqlScript += "(Tanime.ReleaseMonth BETWEEN $MinDate AND $MaxDate)";
            else if (options.HasMinDate)
                sqlScript += "Tanime.ReleaseMonth >= $MinDate";
            else if (options.HasMaxDate)
                sqlScript += "Tanime.ReleaseMonth <= $MaxDate";
            else
                sqlScript += "";
        }

        if (options.IsAdultContent != null)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript += "Tanime.IsAdultContent = $IsAdultContent";
        }

        if (options.IsExplicitContent != null)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript += "Tanime.IsExplicitContent = $IsExplicitContent";
        }

        if (options.HasIdOrigineAdaptationToInclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript +=
                $"TorigineAdaptation.Id IN ({string.Join(',', options.IdOrigineAdaptationToInclude)})";
        }

        if (options.HasIdOrigineAdaptationToExclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript +=
                $"TorigineAdaptation.Id NOT IN ({string.Join(',', options.IdOrigineAdaptationToExclude)})";
        }

        if (options.HasIdDistributorsToInclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript +=
                $"ContactDistributorId IN ({string.Join(',', options.IdDistributorsToInclude)})";
        }

        if (options.HasIdDistributorsToExclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript +=
                $"ContactDistributorId NOT IN ({string.Join(',', options.IdDistributorsToExclude)})";
        }

        if (options.HasIdStudiosToInclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript += $"ContactStudioId IN ({string.Join(',', options.IdStudiosToInclude)})";
        }

        if (options.HasIdStudiosToExclude)
        {
            AddWhereOrAndClause(ref sqlScript);

            sqlScript += $"ContactStudioId NOT IN ({string.Join(',', options.IdStudiosToExclude)})";
        }

        if (options.HasThumbnail != null)
        {
            AddWhereOrAndClause(ref sqlScript);

            if (options.HasThumbnail.Value)
                sqlScript += "Tanime.ThumbnailUrl IS NOT NULL";
            else
                sqlScript += "Tanime.ThumbnailUrl IS NULL";
        }

        FilterSelection(ref command, ref sqlScript, options.ItemGroupCountData);

        sqlScript += Environment.NewLine;
        sqlScript += sortBy switch
        {
            AnimeSortBy.Id => $"ORDER BY Tanime.Id {orderBy}",
            AnimeSortBy.Season => $"ORDER BY Tanime.Season {orderBy}",
            AnimeSortBy.Name => $"ORDER BY Tanime.Name {orderBy}",
            AnimeSortBy.Format => $"ORDER BY Tformat.Name {orderBy}",
            AnimeSortBy.SheetId => $"ORDER BY Tanime.SheetId {orderBy}",
            AnimeSortBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {orderBy}",
            AnimeSortBy.EpisodesCount => $"ORDER BY Tanime.EpisodeCount {orderBy}",
            AnimeSortBy.ReleaseDate => $"ORDER BY Tanime.ReleaseMonth {orderBy}",
            AnimeSortBy.EndDate => $"ORDER BY Tanime.EndDate {orderBy}",
            AnimeSortBy.Duration => $"ORDER BY Tanime.Duration {orderBy}",
            AnimeSortBy.Target => $"ORDER BY Ttarget.Name {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };

        command.CommandText = sqlScript;
        command.Parameters.Clear();

        if (options.HasKeyword)
            command.Parameters.AddWithValue("$Keyword", $"%{options.Keyword}%");

        if (options.HasMinDate)
            command.Parameters.AddWithValue("$MinDate", options.MinDate.ToNumberedDate());

        if (options.HasMaxDate)
            command.Parameters.AddWithValue("$MaxDate", options.MaxDate.ToNumberedDate());

        if (options.IsAdultContent != null)
            command.Parameters.AddWithValue("$IsAdultContent", options.IsAdultContent.Value ? 1 : 0);

        if (options.IsExplicitContent != null)
            command.Parameters.AddWithValue("$IsExplicitContent", options.IsExplicitContent.Value ? 1 : 0);



        return sqlScript;
    }
    
    private static void FilterSelection(ref SqliteCommand command, ref string sqlScript, IReadOnlyCollection<ItemGroupCountStruct> groups)
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
        if (kind is ItemGroupCountKind.None or ItemGroupCountKind.GroupName)
            return;
        
        var sqlPart = "";
        switch (kind)
        {
            case ItemGroupCountKind.OrigineAdaptation:
                var origineData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (origineData.Length == 0)
                    return;
                sqlPart += $"(TorigineAdaptation.Name IN ({string.Join(',', origineData)}))";
                break;
            case ItemGroupCountKind.Season:
                var seasonData = groups.Select(x => x.Data).OfType<uint>().ToArray();
                if (seasonData.Length == 0)
                    return;
                sqlPart += $"(Tseason.Season IN ({string.Join(',', seasonData)}))";
                break;
            case ItemGroupCountKind.ReleaseMonth:
                var releaseMonthData = groups.Select(x => x.Data).OfType<uint>().ToArray();
                if (releaseMonthData.Length == 0)
                    return;
                sqlPart += $"(Tanime.ReleaseMonth IN ({string.Join(',', releaseMonthData)}))";
                break;
            case ItemGroupCountKind.AnimeLetter:
                var letterData = groups.Select(x => x.Data).OfType<char>().ToArray();
                if (letterData.Length == 0)
                    return;
                var letterCondition = new StringBuilder();
                letterCondition.AppendLine();
                letterCondition.AppendLine($"Tanime.Name COLLATE NOCASE LIKE '{letterData[0]}%'");
                foreach (var value in letterData[1..])
                    letterCondition.Append($" OR Tanime.Name COLLATE NOCASE LIKE '{value}%'");
                sqlPart += $"({letterCondition})";
                break;
            case ItemGroupCountKind.Category:
                var categoryData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (categoryData.Length == 0)
                    return;
                sqlPart += $"(TanimeCategory.IdCategory IN ({string.Join(',', categoryData)}))";
                break;
            case ItemGroupCountKind.Target:
                break;
            case ItemGroupCountKind.GroupName:
            case ItemGroupCountKind.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (sqlPart.IsStringNullOrEmptyOrWhiteSpace())
            return;
        
        AddWhereOrAndClause(ref sqlScript);
        
        sqlScript += sqlPart;
    }

    
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