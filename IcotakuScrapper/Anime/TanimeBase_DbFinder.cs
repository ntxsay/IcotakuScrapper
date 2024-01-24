using System.Text;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects.Models;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    /// <summary>
    /// Pagine les éléments en fonction des options
    /// </summary>
    /// <param name="options"></param>
    /// <param name="currentPage"></param>
    /// <param name="maxContentByPage"></param>
    /// <param name="sortBy"></param>
    /// <param name="groupBy"></param>
    /// <param name="itemsOrderedBy"></param>
    /// <param name="groupHeaderOrderedBy"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Paginate<TanimeBase>> PaginateAsync(AnimeDbFinderOptions options,
        uint currentPage = 1, uint maxContentByPage = 20,
        AnimeSortBy sortBy = AnimeSortBy.ReleaseMonth,
        AnimeGroupBy groupBy = AnimeGroupBy.ReleaseMonth,
        OrderBy itemsOrderedBy = OrderBy.Asc, OrderBy groupHeaderOrderedBy = OrderBy.Asc,
        CancellationToken? cancellationToken = null)
    {
        //Initialise la connexion à la base de données
        await using var command = Main.Connection.CreateCommand();

        //Declaration de la variable qui permet de compter le nombre total d'éléments
        uint totalItems;
        
        //Initialise la liste qui contiendra les Ids des éléments
        List<int> resultIds = [];
        
        //Obtient le script de sélection des Id des éléments en prenant en compte les options
        GetSqlSelectScript(command, DbScriptMode.GetId, options, sortBy, groupBy, itemsOrderedBy, groupHeaderOrderedBy);

        //Execute la requête
        await using (var resultReader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None))
        {
            //Si la requête ne retourne aucun élément, on retourne une pagination vide
            if (!resultReader.HasRows)
                return new Paginate<TanimeBase>(
                    currentPage: 1,
                    totalPages: 1,
                    maxItemsPerPage: maxContentByPage,
                    totalItems: 0,
                    items: []);
            
            //Lit les Ids des éléments et les ajoutes à la liste
            while (await resultReader.ReadAsync(cancellationToken ?? CancellationToken.None))
                if (!resultReader.IsDBNull(resultReader.GetOrdinal("AnimeId")))
                    resultIds.Add(resultReader.GetInt32(resultReader.GetOrdinal("AnimeId")));

            //Met à jour le nombre total d'éléments
            totalItems = (uint)resultIds.Count;
        
            //Si le nombre total d'éléments est égal à 0, on retourne une pagination vide
            if (totalItems == 0)
                return new Paginate<TanimeBase>(
                    currentPage: 1,
                    totalPages: 1,
                    maxItemsPerPage: maxContentByPage,
                    totalItems: 0,
                    items: []);
        }
        
        //Obtient le nombre total de pages
        var totalPages = ExtensionMethods.CountPage(totalItems, maxContentByPage);
        
        //Obtient les Ids des éléments de la page actuelle
        var paginatedValues = ExtensionMethods.GetPage(resultIds, currentPage, maxContentByPage);
        
        //Si les Ids des éléments de la page actuelle est vide, on retourne une pagination vide
        if (paginatedValues.Length == 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        //Obtient le script de sélection des éléments en fonction des Ids précédemment récupérés
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine +
                              $"WHERE Tanime.Id IN ({string.Join(',', paginatedValues)})";

        //Execute la requête
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        
        //Si la requête ne retourne aucun élément, on retourne une pagination vide
        if (!reader.HasRows)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        //Obtient les éléments
        var records = await GetRecords(reader, cancellationToken);
        
        //Si les éléments sont vides, on retourne une pagination vide
        if (records.Length == 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        //Retourne la pagination
        return new Paginate<TanimeBase>(
                currentPage: currentPage,
                totalPages: totalPages,
                maxItemsPerPage: maxContentByPage,
                totalItems: totalItems,
                items: records);
    }
    
    /// <summary>
    /// Prépare la requête en fonction des options
    /// </summary>
    /// <param name="command"></param>
    /// <param name="scriptMode"></param>
    /// <param name="options"></param>
    /// <param name="sortBy"></param>
    /// <param name="groupBy"></param>
    /// <param name="itemsOrderedBy"></param>
    /// <param name="groupHeaderOrderedBy"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static void GetSqlSelectScript(SqliteCommand command, DbScriptMode scriptMode,
            AnimeDbFinderOptions options,
        AnimeSortBy sortBy = AnimeSortBy.Name, 
        AnimeGroupBy groupBy = AnimeGroupBy.Format,
        OrderBy itemsOrderedBy = OrderBy.Asc, OrderBy groupHeaderOrderedBy = OrderBy.Asc)
    {
        var sqlScript = scriptMode switch
        {
            DbScriptMode.Select => IcotakuSqlSelectScript,
            DbScriptMode.Count => IcotakuSqlCountScript,
            DbScriptMode.GetId => IcotakuSqlGetIdScript,
            _ => throw new ArgumentOutOfRangeException(nameof(scriptMode), scriptMode,
                "Le mode de script n'est pas supporté")
        };

        if (options.HasKeyword)
            sqlScript += Environment.NewLine +
                         "WHERE (Tanime.Name LIKE $Keyword COLLATE NOCASE OR Tanime.Description LIKE $Keyword COLLATE NOCASE)";
        
        if (options.HasMinDate || options.HasMaxDate)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";


            if (options is { HasMinDate: true, HasMaxDate: true })
                sqlScript += "(Tanime.ReleaseMonth BETWEEN $MinMonthDate AND $MaxMonthDate)";
            else if (options.HasMinDate)
                sqlScript += "Tanime.ReleaseMonth >= $MinMonthDate";
            else if (options.HasMaxDate)
                sqlScript += "Tanime.ReleaseMonth <= $MaxMonthDate";
            else
                sqlScript += "";
        }

        #region Adult Content

        if (Main.IsAccessingToAdultContent)
        {
            if (options.IsAdultContent != null)
            {
                if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                    sqlScript += Environment.NewLine + "AND ";
                else
                    sqlScript += Environment.NewLine + "WHERE ";

                sqlScript += "Tanime.IsAdultContent = $IsAdultContent";
            }
        }
        else
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "Tanime.IsAdultContent = 0";
        }

        #endregion

        #region Explicit Content

        if (options.IsExplicitContent != null)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += "Tanime.IsExplicitContent = $IsExplicitContent";
        }
        else
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += "Tanime.IsExplicitContent = 0";
        }

        #endregion

        #region Origine

        if (options.HasIdOrigineAdaptationToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"Tanime.IdOrigine IN ({string.Join(',', options.IdOrigineAdaptationToInclude)})";
        }

        if (options.HasIdOrigineAdaptationToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"Tanime.IdOrigine NOT IN ({string.Join(',', options.IdOrigineAdaptationToExclude)})";
        }

        #endregion

        #region Format

        if (options.HasIdFormatToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Tformat.Id IN ({string.Join(',', options.IdFormatToInclude)})";
        }
        
        if (options.HasIdFormatToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Tformat.Id NOT IN ({string.Join(',', options.IdFormatToExclude)})";
        }

        #endregion

        #region Target

        if (options.HasIdTargetToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Ttarget.Id IN ({string.Join(',', options.IdTargetToInclude)})";
        }
        
        if (options.HasIdTargetToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Ttarget.Id NOT IN ({string.Join(',', options.IdTargetToExclude)})";
        }

        #endregion
        
        #region Categories
        
        if (options.HasIdCategoryToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Tcategory.Id IN ({string.Join(',', options.IdCategoriesToInclude)})";
        }
        
        if (options.HasIdCategoryToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";
            
            sqlScript += $"Tcategory.Id NOT IN ({string.Join(',', options.IdCategoriesToExclude)})";
        }
        
        #endregion
        
        #region Distributors

        if (options.HasIdDistributorsToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeLicense.IdDistributor IN ({string.Join(',', options.IdDistributorsToInclude)})";
        }

        if (options.HasIdDistributorsToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeLicense.IdDistributor NOT IN ({string.Join(',', options.IdDistributorsToExclude)})";
        }

        #endregion

        #region Studios

        if (options.HasIdStudiosToInclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeStudio.IdStudio IN ({string.Join(',', options.IdStudiosToInclude)})";
        }

        if (options.HasIdStudiosToExclude)
        {
            if (sqlScript.Contains("WHERE", StringComparison.OrdinalIgnoreCase))
                sqlScript += Environment.NewLine + "AND ";
            else
                sqlScript += Environment.NewLine + "WHERE ";

            sqlScript += $"TanimeStudio.IdStudio NOT IN ({string.Join(',', options.IdStudiosToExclude)})";
        }

        #endregion
        
        AnimeFilterSelection(ref sqlScript, options.ItemGroupCountData);

        OrderRequestBy(ref sqlScript, sortBy, groupBy, itemsOrderedBy, groupHeaderOrderedBy);
        
        command.CommandText = sqlScript;
        command.Parameters.Clear();

        if (options.HasKeyword)
            command.Parameters.AddWithValue("$Keyword", $"%{options.Keyword}%");

        if (options.HasMinDate)
            command.Parameters.AddWithValue("$MinMonthDate", options.MinDate.ToNumberedDate());

        if (options.HasMaxDate)
            command.Parameters.AddWithValue("$MaxMonthDate", options.MaxDate.ToNumberedDate());

        if (options.IsAdultContent != null)
            command.Parameters.AddWithValue("$IsAdultContent", options.IsAdultContent.Value ? 1 : 0);

        if (options.IsExplicitContent != null)
            command.Parameters.AddWithValue("$IsExplicitContent", options.IsExplicitContent.Value ? 1 : 0);
    }

    private static void OrderRequestBy(ref string sqlScript, AnimeSortBy sortBy,
        AnimeGroupBy groupBy, OrderBy itemsOrderedBy, OrderBy groupHeaderOrderedBy)
    {
        sqlScript += Environment.NewLine;
        sqlScript += sortBy switch
        {
            AnimeSortBy.Id => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.Id {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.Season => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.Id {itemsOrderedBy}, Tseason.SeasonNumber {groupHeaderOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tseason.SeasonNumber {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.ReleaseMonth => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.Id {itemsOrderedBy}, Tanime.ReleaseMonth {groupHeaderOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.ReleaseMonth {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.Name => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.Name {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.Format => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tformat.Name {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.SheetId => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.SheetId {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.OrigineAdaptation => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, TorigineAdaptation.Name {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.EpisodesCount => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.EpisodeCount {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.EndDate => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.EndDate {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.Duration => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {groupHeaderOrderedBy}, Tanime.EpisodeDuration {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            AnimeSortBy.Target => groupBy switch
            {
                AnimeGroupBy.OrigineAdaptation => $"ORDER BY TorigineAdaptation.Name {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.ReleaseMonth => $"ORDER BY Tanime.ReleaseMonth {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Season => $"ORDER BY Tseason.SeasonNumber {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Default => $"ORDER BY Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Categories => $"ORDER BY Tcategory.Name {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Letter => $"ORDER BY UPPER(SUBSTR(Tanime.Name, 1, 1)) {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Format => $"ORDER BY Tformat.Name {groupHeaderOrderedBy}, Ttarget.Name {itemsOrderedBy}",
                AnimeGroupBy.Target => $"ORDER BY Ttarget.Name {itemsOrderedBy}",
                _ => throw new ArgumentOutOfRangeException(nameof(groupBy), groupBy, null)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
    
    private static void AnimeFilterSelection(ref string sqlScript, IReadOnlyCollection<ItemGroupCountStruct> groups)
    {
        //Si la requête est vide, on sort de la méthode
        if (sqlScript.IsStringNullOrEmptyOrWhiteSpace())
            return;
        
        //Si la liste est vide, on sort de la méthode
        if (groups.Count == 0)
            return;
        
        //Si la liste contient plus d'un élément, on sort de la méthode
        var isSame = groups.Select(x => x.IdentifierKind).Distinct().Count() == 1;
        if (!isSame)
        {
            LogServices.LogDebug("Les groupes ne sont pas du même type");
            return;
        }

        //On récupère le type de données
        var kind = groups.First().IdentifierKind;
        //Si le type de données est None, on sort de la méthode
        if (kind == ItemGroupCountKind.None)
            return;
        
        var sqlPart = "";
        switch (kind)
        {
            case ItemGroupCountKind.OrigineAdaptation:
                var origineData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (origineData.Length == 0)
                    return;
                sqlPart += $"(Tanime.IdOrigine IN ({string.Join(',', origineData)}))";
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
                sqlPart += $"(Tanime.ReleaseMonth IN ({string.Join(',', releaseMonthData)}))";
                break;
            case ItemGroupCountKind.Format:
                var groupNameData = groups.Select(x => x.Data).OfType<int>().ToArray();
                if (groupNameData.Length == 0)
                    return;
                sqlPart += $"(Tformat.Name IN ({string.Join(',', groupNameData.Select(s => $"'{s}'"))}))";
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