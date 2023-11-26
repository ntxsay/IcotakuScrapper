using System.Diagnostics;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

/// <summary>
/// Afin d'éviter de créer plusieurs méthodes de sélection, cette énumération permet de sélectionner la colonne à utiliser pour la sélection.
/// </summary>
public enum SheetIntColumnSelect
{
    Id,
    SheetId,
}

public enum SheetSortBy
{
    Id,
    SheetId,
    Type,
    Url,
    FoundedPage
}

/// <summary>
/// Représente un index d'une fiche. Cet index permet de retrouver une fiche à partir de son url.
/// </summary>
public class TsheetIndex
{
    /// <summary>
    /// Obtient ou définit l'identifiant de l'index
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// Obtient ou définit l'identifiant de la fiche associée
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>
    /// Obtient ou définit le type de contenu (anime, manga, etc.)
    /// </summary>
    public IcotakuContentType ContentType { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche
    /// </summary>
    public string Url { get; set; } = null!;
    
    /// <summary>
    /// Obtient ou définit le nom de la fiche
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Obtient ou définit le numéro de page où l'url a été trouvée
    /// </summary>
    public uint FoundedPage { get; set; }


    public TsheetIndex()
    {
    }

    public TsheetIndex(int id)
    {
        Id = id;
    }
    
    public TsheetIndex(int id, int sheetId, IcotakuContentType contentType, string url, uint foundedPage = 0)
    {
        Id = id;
        SheetId = sheetId;
        ContentType = contentType;
        Url = url;
        FoundedPage = foundedPage;
    }

    /// <summary>
    /// Obtient l'url de la page de la liste des animes
    /// </summary>
    /// <param name="contentContentType"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string GetIcotakuFilterUrl(IcotakuContentType contentContentType, uint page = 1)
    {
        return contentContentType switch
        {
            IcotakuContentType.Anime => $"https://anime.icotaku.com/animes.html?filter=all{(page == 0 ? "" : "&page=" + page.ToString())}",
            _ => throw new ArgumentOutOfRangeException(nameof(contentContentType), contentContentType, null)
        };
    }
    
    /// <summary>
    /// Obtient l'url de la page de la liste des animes
    /// </summary>
    /// <param name="letter"></param>
    /// <param name="contentContentType"></param>
    /// <param name="page"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string? GetIcotakuFilterUrl(char letter, IcotakuContentType contentContentType, uint page = 1)
    {
        if (!char.IsLetter(letter))
            return null;
        return contentContentType switch
        {
            IcotakuContentType.Anime => $"https://anime.icotaku.com/animes.html?filter={letter}{(page == 0 ? "" : "&page=" + page.ToString())}",
            _ => throw new ArgumentOutOfRangeException(nameof(contentContentType), contentContentType, "Ce type de contenu n'est pas pris en charge.")
        };
    }
    
    #region Html Manipulation

    /// <summary>
    /// Crée les index de toutes les fiches de la liste des animes
    /// </summary>
    /// <param name="contentContentType"></param>
    /// <returns></returns>
    public static async Task<OperationState> CreateIndexesAsync(IcotakuContentType contentContentType, CancellationToken? cancellationToken = null)
    {
        var (minPage, maxPage) = GetMinAndMaxPage(contentContentType);
        if (minPage == 0 || maxPage == 0)
            return new OperationState(false, "Impossible de récupérer le nombre de pages de la liste des animes.");

        await using var command = (await Main.GetSqliteConnectionAsync()).CreateCommand();
        await DeleteAllAsync(cancellationToken, command);
        
        List<OperationState> results = new();
        for (uint i = (uint)minPage; i <= maxPage; i++)
        {
            var pageResults = GetPageUrls(contentContentType, i).ToArray();
            if (pageResults.Length == 0)
                continue;
            var result = await InsertAsync(pageResults, cancellationToken, command);
            Debug.WriteLine($"Page {i} :: Nombre: {pageResults.Length}, Succès: {result.IsSuccess}, Message: {result.Message}");
            results.Add(result);
        }

        return results.All(a => a.IsSuccess) 
            ? new OperationState(true, "Tous les index ont été créés avec succès.") 
            : new OperationState(false, "Une ou plusieurs erreurs sont survenues lors de la création des index.");
    }

    
    private static IEnumerable<TsheetIndex> GetPageUrls(IcotakuContentType contentContentType, uint currentPage = 1)
    {
        //url de la page en cours contenant le tableau des fiches
        var pageUrl = GetIcotakuFilterUrl(contentContentType, currentPage);
        HtmlWeb web = new();
        var htmlDocument = web.Load(pageUrl);
        
        //Obtient la liste des urls des fiches
        var sheetUrlsNode = htmlDocument.DocumentNode.SelectNodes("//div[@id='page']/table[@class='table_apercufiche']//div[@class='td_apercufiche']/a[2]").ToArray();
        if (sheetUrlsNode.Length == 0)
           yield break;

        foreach (var htmlNode in sheetUrlsNode)
        {
            if (htmlNode == null)
                continue;
            
            var sheetIndex = GetSheetIndex(htmlNode, contentContentType, currentPage);
            if (sheetIndex != null)
                yield return sheetIndex;
        }
    }

    /// <summary>
    /// Crée un index à partir d'un noeud HTML
    /// </summary>
    /// <param name="htmlNode"></param>
    /// <param name="contentContentType"></param>
    /// <param name="currentPage"></param>
    /// <returns></returns>
    private static TsheetIndex? GetSheetIndex(HtmlNode htmlNode, IcotakuContentType contentContentType, uint currentPage)
    {
        var sheetHref = htmlNode.GetAttributeValue("href", string.Empty);
        if (sheetHref.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        var split = sheetHref.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
            return null;
        var sheetId = split.FirstOrDefault(f => 
            !f.Any(a => 
                char.IsLetter(a) || a == '-' || a == '_'));
        
        if (sheetId.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        if (!int.TryParse(sheetId, out var sheetIdInt))
            return null;
        
        sheetHref = "https://anime.icotaku.com" + sheetHref;
        if (!Uri.TryCreate(sheetHref, UriKind.Absolute, out var sheetUri))
            return null;
        
        var sheetName = HttpUtility.HtmlDecode(htmlNode.InnerText.Trim());
        
        return new TsheetIndex()
        {
            SheetId = sheetIdInt,
            ContentType = contentContentType,
            Url = sheetUri.OriginalString,
            Name = sheetName,
            FoundedPage = currentPage
        };
    }
    
    /// <summary>
    /// Retourne le nombre de pages de la liste des animes
    /// </summary>
    /// <returns></returns>
    private static (int minPage, int maxPage) GetMinAndMaxPage(IcotakuContentType contentContentType)
    {
        HtmlWeb web = new();

        var url = contentContentType switch
        {
            IcotakuContentType.Anime => "https://anime.icotaku.com/animes.html?filter=all",
            _ => throw new ArgumentOutOfRangeException(nameof(contentContentType), contentContentType, null)
        };
        
        var htmlDocument = web.Load(url);
        
        var minPageNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[1]");
        var maxPageNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='page']//div[@class='anime_pager']/a[last()]");
        
        if (minPageNode is null || maxPageNode is null)
            return (0, 0);
        
        var minPageHref = minPageNode.GetAttributeValue("href", string.Empty);
        var maxPageHref = maxPageNode.GetAttributeValue("href", string.Empty);
        if (minPageHref.IsStringNullOrEmptyOrWhiteSpace() || maxPageHref.IsStringNullOrEmptyOrWhiteSpace())
            return (0, 0);
        
        minPageHref = HttpUtility.UrlDecode("https://anime.icotaku.com" + minPageHref).Replace("&amp;", "&");
        maxPageHref = HttpUtility.UrlDecode("https://anime.icotaku.com" + maxPageHref).Replace("&amp;", "&");

        if (!Uri.TryCreate(minPageHref, UriKind.Absolute, out var minPageUri) ||
            !Uri.TryCreate(maxPageHref, UriKind.Absolute, out var maxPageUri))
            return (0, 0);
        
        var minPage = HttpUtility.ParseQueryString(minPageUri.Query).Get("page");
        var maxPage = HttpUtility.ParseQueryString(maxPageUri.Query).Get("page");
        if (minPage is null || maxPage is null)
            return (0, 0);
        
        if (int.TryParse(minPage, out var minPageInt) && int.TryParse(maxPage, out var maxPageInt))
            return (minPageInt, maxPageInt);
        
        return (0, 0);
    }
    

    #endregion
    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TsheetIndex
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TsheetIndex ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = columnSelect switch
        {
            SheetIntColumnSelect.Id => "SELECT COUNT(Id) FROM TsheetIndex WHERE Id = $Id",
            SheetIntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TsheetIndex WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TsheetIndex ayant le type de contenu spécifiée
    /// </summary>
    /// <param name="contentContentType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(IcotakuContentType contentContentType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Type = $Type";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Type", (byte)contentContentType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TsheetIndex ayant l'url spécifiée
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string url, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(string url, IcotakuContentType contentContentType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Type == $Type AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        command.Parameters.AddWithValue("$Type", (byte)contentContentType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(string url,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetIndex WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    
    public static async Task<int?> GetIdOfAsync(string url, IcotakuContentType contentContentType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetIndex WHERE Type == $Type AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        command.Parameters.AddWithValue("$Type", (byte)contentContentType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(IcotakuContentType contentContentType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(contentContentType, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string url, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(url, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string url, IcotakuContentType contentContentType,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(url, contentContentType, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    /// <summary>
    /// Retourne tous les enregistrements de la table TsheetIndex
    /// </summary>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TsheetIndex[]> SelectAsync(SheetSortBy sortBy, OrderBy orderBy, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = sqlSelectScript + Environment.NewLine;

        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY Type",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
        
        command.CommandText += orderBy switch
        {
            OrderBy.Asc => " ASC",
            OrderBy.Desc => " DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };
        
        if (limit > 0)
            command.CommandText += $" LIMIT {limit} OFFSET {skip}";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records;
    }
    
    /// <summary>
    /// Retourne tous les enregistrements de la table TsheetIndex ayant le type de contenu spécifié
    /// </summary>
    /// <param name="contentContentType"></param>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TsheetIndex[]> SelectAsync(IcotakuContentType contentContentType, SheetSortBy sortBy, OrderBy orderBy, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = sqlSelectScript + Environment.NewLine;
        command.CommandText += " WHERE Type = $Type" + Environment.NewLine;
        
        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY Type",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
        
        command.CommandText += orderBy switch
        {
            OrderBy.Asc => " ASC",
            OrderBy.Desc => " DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };
        
        if (limit > 0)
            command.CommandText += $" LIMIT {limit} OFFSET {skip}";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Type", (byte)contentContentType);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records;
    }

    #endregion
    
    #region Single

    /// <summary>
    /// Retourne un enregistrement de la table TsheetIndex à partir de l'identifiant spécifié de la table
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<TsheetIndex?> SingleAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = sqlSelectScript + Environment.NewLine + columnSelect switch
        {
            SheetIntColumnSelect.Id => " WHERE Id = $Id",
            SheetIntColumnSelect.SheetId => " WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records.Length > 0 ? records[0] : null;
    }
    
    public static async Task<TsheetIndex?> SingleAsync(string url, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = sqlSelectScript + " WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records.Length > 0 ? records[0] : null;
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await InsertAsync(this, cancellationToken, cmd);
    
    public static async Task<OperationState<int>> InsertAsync(TsheetIndex record, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (record.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url ne peut pas être vide.");
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        if (await ExistsAsync(record.Url, cancellationToken, command))
            return new OperationState<int>(false, "L'url existe déjà dans la base de données.");
        command.CommandText = 
            """
            INSERT INTO TsheetIndex
                (SheetId, Type, Url, FoundedPage)
            VALUES
                ($SheetId, $Type, $Url, $FoundedPage)
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", record.SheetId);
        command.Parameters.AddWithValue("$Type", (byte)record.ContentType);
        command.Parameters.AddWithValue("$Url", record.Url);
        command.Parameters.AddWithValue("$FoundedPage", record.FoundedPage);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Impossible d'insérer l'enregistrement dans la base de données.");

            record.Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "L'enregistrement a été inséré avec succès.", record.Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion de l'enregistrement.");
        }
    }
    
    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TsheetIndex> records, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (records.Count == 0)
            return new OperationState(false, "La liste ne peut pas être vide.");
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        command.CommandText =  "INSERT INTO TsheetIndex (SheetId, Type, Url, FoundedPage, Name)" + Environment.NewLine;
        
        command.Parameters.Clear();
        
        for (uint i = 0; i < records.Count; i++)
        {
            var record = records.ElementAt((int)i);
            if (record.Url.IsStringNullOrEmptyOrWhiteSpace())
                return new OperationState(false, "L'url ne peut pas être vide.");
            
            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($SheetId{i}, $Type{i}, $Url{i}, $FoundedPage{i}, $Name{i})";
            
            command.Parameters.AddWithValue($"$SheetId{i}", record.SheetId);
            command.Parameters.AddWithValue($"$Type{i}", (byte)record.ContentType);
            command.Parameters.AddWithValue($"$Url{i}", record.Url);
            command.Parameters.AddWithValue($"$FoundedPage{i}", record.FoundedPage);
            command.Parameters.AddWithValue($"$Name{i}", record.Name);

            Debug.WriteLine($"SheetId: {record.SheetId}, Type: {record.ContentType}, Url: {record.Url}, FoundedPage: {record.FoundedPage}, Name: {record.Name}");
        }


        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return count == 0 
                ? new OperationState(false, "Impossible d'insérer l'enregistrement dans la base de données.") 
                : new OperationState(true, $"{count} enregistrement(s) sur {records.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion de l'enregistrement.");
        }
    }

    #endregion
    
    #region Update
    
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await UpdateAsync(this, cancellationToken, cmd);
    
    public static async Task<OperationState> UpdateAsync(TsheetIndex record, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (record.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url ne peut pas être vide.");
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        var existingId = await GetIdOfAsync(record.Url, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != record.Id)
            return new OperationState(false, "L'url existe déjà dans la base de données.");
        
        command.CommandText = 
            """
            UPDATE TsheetIndex
            SET
                SheetId = $SheetId,
                Type = $Type,
                Url = $Url,
                FoundedPage = $FoundedPage
            WHERE
                Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", record.Id);
        command.Parameters.AddWithValue("$SheetId", record.SheetId);
        command.Parameters.AddWithValue("$Type", (byte)record.ContentType);
        command.Parameters.AddWithValue("$Url", record.Url);
        command.Parameters.AddWithValue("$FoundedPage", record.FoundedPage);

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0 
                ? new OperationState(false, "Impossible de mettre à jour l'enregistrement dans la base de données.") 
                : new OperationState(true, "L'enregistrement a été mis à jour avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour de l'enregistrement.");
        }
    }
    
    #endregion
    
    #region Delete
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        command.CommandText = 
            """
            DELETE FROM TsheetIndex
            WHERE
                Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0 
                ? new OperationState(false, "Impossible de supprimer l'enregistrement dans la base de données.") 
                : new OperationState(true, "L'enregistrement a été supprimé avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex";

        command.Parameters.Clear();

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return count == 0 
                ? new OperationState(false, "Impossible de supprimer les enregistrements dans la base de données.") 
                : new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    #endregion

    private static async IAsyncEnumerable<TsheetIndex> GetRecordsAsync(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                sheetIdIndex: reader.GetOrdinal("SheetId"),
                typeIndex: reader.GetOrdinal("Type"),
                urlIndex: reader.GetOrdinal("Url"),
                nameIndex: reader.GetOrdinal("Name"),
                foundedPageIndex: reader.GetOrdinal("FoundedPage"));
        }
    }

    /// <summary>
    ///  Obtient un enregistrement de la table TsheetIndex à partir du lecteur de données.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="idIndex"></param>
    /// <param name="typeIndex"></param>
    /// <param name="urlIndex"></param>
    /// <param name="foundedPageIndex"></param>
    /// <returns></returns>
    internal static TsheetIndex GetRecord(SqliteDataReader reader, int idIndex, int sheetIdIndex, int typeIndex, int urlIndex, int nameIndex,
        int foundedPageIndex)
    {
        var record = new TsheetIndex
        {
            Id = reader.GetInt32(idIndex),
            SheetId = reader.GetInt32(sheetIdIndex),
            ContentType = (IcotakuContentType)reader.GetByte(typeIndex),
            Url = reader.GetString(urlIndex),
            Name = reader.IsDBNull(nameIndex) ? null : reader.GetString(nameIndex),
            FoundedPage = (uint)reader.GetInt32(foundedPageIndex)
        };

        return record;
    }

    private const string sqlSelectScript =
        """
        SELECT
            Id,
            SheetId,
            Url,
            Name,
            Type,
            FoundedPage
        FROM TsheetIndex
        """;
}