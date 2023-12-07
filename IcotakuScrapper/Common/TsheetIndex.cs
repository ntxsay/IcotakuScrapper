using System.Diagnostics;
using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum SheetSortBy
{
    Id,
    Name,
    SheetId,
    Section,
    Type,
    Url,
    FoundedPage
}

/// <summary>
/// Représente un index d'une fiche. Cet index permet de retrouver une fiche à partir de son url.
/// </summary>
public partial class TsheetIndex
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
    /// Obtient ou définit la section icotaku concerné (anime, manga, etc.)
    /// </summary>
    public IcotakuSection Section { get; set; }

    /// <summary>
    /// Obtient ou définit le type de la fiche (anime, manga, personnage, studio, etc.)
    /// </summary>
    public IcotakuSheetType Type { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Obtient ou définit le nom de la fiche
    /// </summary>
    public string Name { get; set; } = string.Empty;

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

    public TsheetIndex(int id, int sheetId, IcotakuSection section, string url, uint foundedPage = 0)
    {
        Id = id;
        SheetId = sheetId;
        Section = section;
        Url = url;
        FoundedPage = foundedPage;
    }

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
    public static async Task<int> CountAsync(int id, SheetIntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
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
    /// <param name="contentSection"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(IcotakuSection contentSection, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE SECTION = $Section";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", (byte)contentSection);
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

    public static async Task<int> CountAsync(string url, IcotakuSection contentSection,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Section == $Section AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        command.Parameters.AddWithValue("$Section", (byte)contentSection);
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

    public static async Task<int?> GetIdOfAsync(string url, IcotakuSection contentSection,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetIndex WHERE Section == $Section AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", url.Trim());
        command.Parameters.AddWithValue("$Section", (byte)contentSection);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(int id, SheetIntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(IcotakuSection contentSection,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(contentSection, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string url, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(url, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string url, IcotakuSection contentSection,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(url, contentSection, cancellationToken, cmd) > 0;

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
    public static async Task<TsheetIndex[]> SelectAsync(SheetSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;

        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY ItemType",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            SheetSortBy.Name => " ORDER BY ItemName",
            SheetSortBy.Section => " ORDER BY Section",
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
    /// <param name="sections"></param>
    /// <param name="sheetTypes"></param>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TsheetIndex[]> SelectAsync(HashSet<IcotakuSection> sections, HashSet<IcotakuSheetType> sheetTypes, SheetSortBy sortBy,
        OrderBy orderBy, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        if (sections.Count > 0)
        {
            command.CommandText += Environment.NewLine + "WHERE Type IN (";
            for (var i = 0; i < sections.Count; i++)
            {
                command.CommandText += i == 0 ? $"$Section{i}" : $", $Section{i}";
                command.Parameters.AddWithValue($"$Section{i}", (byte)sections.ElementAt(i));
            }

            command.CommandText += ")";
        }
        
        if (sheetTypes.Count > 0)
        {
            if (sections.Count > 0)
                command.CommandText += " AND Type IN (";
            else
                command.CommandText += Environment.NewLine + "WHERE Type IN (";

            for (var i = 0; i < sheetTypes.Count; i++)
            {
                command.CommandText += i == 0 ? $"$SheetType{i}" : $", $SheetType{i}";
                command.Parameters.AddWithValue($"$SheetType{i}", (byte)sheetTypes.ElementAt(i));
            }

            command.CommandText += ")";
        }

        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY ItemType",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            SheetSortBy.Name => " ORDER BY ItemName",
            SheetSortBy.Section => " ORDER BY Section",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };

        command.CommandText += orderBy switch
        {
            OrderBy.Asc => " ASC",
            OrderBy.Desc => " DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };

        command.AddLimitOffset(limit, skip);

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
    public static async Task<TsheetIndex?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);
        
        if (!isColumnSelectValid)
        {
            return null;
        }

        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => " WHERE Id = $Id",
            IntColumnSelect.SheetId => " WHERE SheetId = $Id",
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
        command.CommandText = SqlSelectScript + " WHERE Url = $Url COLLATE NOCASE";

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

    /// <summary>
    /// Insère un enregistrement dans la table TsheetIndex
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await InsertAsync(Section, Type, Name, Url, SheetId, FoundedPage, cancellationToken, cmd);

    /// <summary>
    /// Insère un enregistrement dans la table TsheetIndex
    /// </summary>
    /// <param name="section"></param>
    /// <param name="sheetType"></param>
    /// <param name="name"></param>
    /// <param name="url"></param>
    /// <param name="sheetId"></param>
    /// <param name="foundedPage"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    internal static async Task<OperationState<int>> InsertAsync(IcotakuSection section, IcotakuSheetType sheetType, string name, string url,
        int sheetId, uint foundedPage, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime ne peut pas être vide");

        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");

        if (sheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (await ExistsAsync(url, cancellationToken, command))
            return new OperationState<int>(false, "L'url existe déjà dans la base de données.");

        command.CommandText =
            """
            INSERT INTO TsheetIndex 
                (SheetId, Url, Section, ItemType, ItemName, FoundedPage) 
            VALUES 
                ($SheetId, $Url, $Section, $ItemType, $ItemName, $FoundedPage)
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$ItemType", (byte)sheetType);
        command.Parameters.AddWithValue("$ItemName", name);
        command.Parameters.AddWithValue("$FoundedPage", foundedPage);
        

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result > 0
                ? new OperationState<int>(true, "L'anime a été ajouté avec succès")
                : new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
        }
    }
    
    /// <summary>
    /// Insère une collection d'index dans la table TsheetIndex
    /// </summary>
    /// <param name="records"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TsheetIndex> records,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (records.Count == 0)
            return new OperationState(false, "La liste ne peut pas être vide.");
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "INSERT OR IGNORE INTO TsheetIndex (SheetId, Url, Section, ItemType, ItemName, FoundedPage)" + Environment.NewLine;

        command.Parameters.Clear();

        for (uint i = 0; i < records.Count; i++)
        {
            var record = records.ElementAt((int)i);
            if (record.Url.IsStringNullOrEmptyOrWhiteSpace())
                return new OperationState(false, "L'url ne peut pas être vide.");

            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($SheetId{i}, $Url{{i}}, $Section{i}, $ItemType{i}, $ItemName{i}, $FoundedPage{i})";
            
            command.Parameters.AddWithValue($"$SheetId{i}", record.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", record.Url);
            command.Parameters.AddWithValue($"$Section{i}", (byte)record.Section);
            command.Parameters.AddWithValue($"$ItemType{i}", (byte)record.Type);
            command.Parameters.AddWithValue($"$ItemName{i}", record.Name);
            command.Parameters.AddWithValue($"$FoundedPage{i}", record.FoundedPage);

            Debug.WriteLine(
                $"SheetId: {record.SheetId}, Type: {record.Section}, Url: {record.Url}, FoundedPage: {record.FoundedPage}, Name: {record.Name}");
        }


        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return count == 0
                ? new OperationState(false, "Impossible d'insérer l'enregistrement dans la base de données.")
                : new OperationState(true,
                    $"{count} enregistrement(s) sur {records.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion de l'enregistrement.");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await UpdateAsync(Id, Section, Type, Name, Url, SheetId, FoundedPage, cancellationToken, cmd);

    public static async Task<OperationState> UpdateAsync(int id,IcotakuSection section, IcotakuSheetType sheetType, string name, string url,
        int sheetId, uint foundedPage,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'id de l'enregistrement n'est pas valide");
        
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de la fiche ne peut pas être vide");

        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url de la fiche de l'anime ne peut pas être vide");

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'url de la fiche de l'anime n'est pas valide");

        if (sheetId <= 0)
            return new OperationState(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var existingId = await GetIdOfAsync(url, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != id)
            return new OperationState(false, "L'url existe déjà dans la base de données.");

        command.CommandText =
            """
            UPDATE TsheetIndex
            SET
                SheetId = $SheetId,
                Section = $Section,
                ItemType = $ItemType,
                ItemName = $ItemName,
                Url = $Url,
                FoundedPage = $FoundedPage
            WHERE
                Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$FoundedPage", foundedPage);
        command.Parameters.AddWithValue("$ItemType", (byte)sheetType);
        command.Parameters.AddWithValue("$ItemName", name);

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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
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
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
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
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex WHERE Section = $Section";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, IcotakuSheetType sheetType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex WHERE Section = $Section AND ItemType = $ItemType";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$ItemType", (byte)sheetType);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
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
                sectionIndex: reader.GetOrdinal("Section"),
                typeIndex: reader.GetOrdinal("ItemType"),
                urlIndex: reader.GetOrdinal("Url"),
                nameIndex: reader.GetOrdinal("ItemName"),
                foundedPageIndex: reader.GetOrdinal("FoundedPage"));
        }
    }

    /// <summary>
    ///  Obtient un enregistrement de la table TsheetIndex à partir du lecteur de données.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="idIndex"></param>
    /// <param name="sectionIndex"></param>
    /// <param name="typeIndex"></param>
    /// <param name="urlIndex"></param>
    /// <param name="nameIndex"></param>
    /// <param name="foundedPageIndex"></param>
    /// <param name="sheetIdIndex"></param>
    /// <returns></returns>
    internal static TsheetIndex GetRecord(SqliteDataReader reader, int idIndex, int sheetIdIndex, int sectionIndex, int typeIndex,
        int urlIndex, int nameIndex,
        int foundedPageIndex)
    {
        var record = new TsheetIndex
        {
            Id = reader.GetInt32(idIndex),
            SheetId = reader.GetInt32(sheetIdIndex),
            Section = (IcotakuSection)reader.GetByte(sectionIndex),
            Type = (IcotakuSheetType)reader.GetByte(typeIndex),
            Url = reader.GetString(urlIndex),
            Name = reader.GetString(nameIndex),
            FoundedPage = (uint)reader.GetInt32(foundedPageIndex)
        };

        return record;
    }

    private const string SqlSelectScript =
        """
        SELECT
            Id,
            SheetId,
            Url,
            Section,
            ItemName,
            ItemType,
            FoundedPage
        FROM TsheetIndex
        """;
}