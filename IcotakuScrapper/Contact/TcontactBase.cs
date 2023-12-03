using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Contact;

/// <summary>
/// Représente un contact sans les informations détaillées de <see cref="Tcontact"/>.
/// </summary>
public class TcontactBase
{
    /// <summary>
    /// Obtient ou définit l'id du contact.
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// Obtient ou définit l'id de la fiche Icotaku du contact.
    /// </summary>
    public int SheetId { get; set; }
    
    /// <summary>
    /// Obtient ou définit le type de contact.
    /// </summary>
    public ContactType Type { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom d'affichage du contact.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Obtient ou définit la description de l'anime.
    /// </summary>
    public string? Presentation { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche de l'anime.
    /// </summary>
    public string Url { get; set; } = null!;
    
    /// <summary>
    /// Obtient ou définit l'url de l'image miniature du contact.
    /// </summary>
    public string? ThumbnailUrl { get; set; }
    
    public TcontactBase()
    {
    }

    public TcontactBase(int id)
    {
        Id = id;
    }
    
        #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcontact";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect, new HashSet<IntColumnSelect>()
        {
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        });
        
        if (!isColumnSelectValid)
        {
            return 0;
        }
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM Tcontact WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM Tcontact WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(string displayName, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcontact WHERE DisplayName = $DisplayName COLLATE NOCASE";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", displayName);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcontact WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(string displayName, int sheetId, Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcontact WHERE DisplayName = $DisplayName COLLATE NOCASE OR Url = $Url COLLATE NOCASE OR SheetId = $SheetId";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$DisplayName", displayName);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int?> GetIdOfAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tcontact WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    
    public static async Task<int?> GetIdOfAsync(string displayName, int sheetId, Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tcontact WHERE DisplayName = $DisplayName COLLATE NOCASE OR Url = $Url COLLATE NOCASE OR SheetId = $SheetId";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$DisplayName", displayName);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(string displayName, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(displayName, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(sheetUri, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(string displayName, int sheetId, Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(displayName, sheetId, sheetUri, cancellationToken, cmd) > 0;

    #endregion

    
    #region Single

    public static async Task<TcontactBase?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect, new HashSet<IntColumnSelect>()
        {
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        });
        
        if (!isColumnSelectValid)
        {
            return null;
        }
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Tcontact.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tcontact.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
    }
    
    public static async Task<TcontactBase?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.DisplayName = $DisplayName COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", name);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
    }
    
    public static async Task<TcontactBase?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.Url = $Url COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : await GetRecords(reader, cancellationToken).FirstOrDefaultAsync();
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
            DELETE FROM TanimeStudio WHERE IdStudio = $Id;
            DELETE FROM TanimeLicense WHERE IdDistributor = $Id;
            DELETE FROM TanimeStaff WHERE IdIndividu = $Id;
            DELETE FROM TanimeCharacter WHERE IdCharacter = $Id;
            DELETE FROM Tcontact WHERE Id = $Id;
            """;
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} lignes supprimées");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression du contact");
        }
    }

    #endregion

    
    private static async IAsyncEnumerable<TcontactBase> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TcontactBase()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                Type = (ContactType)reader.GetInt32(reader.GetOrdinal("Type")),
                DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                Presentation = reader.IsDBNull(reader.GetOrdinal("Presentation"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Presentation")),
                Url = reader.GetString(reader.GetOrdinal("Url")),
                ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ThumbnailUrl"))
            };
        }
    }
    
    private const string SqlSelectScript =
        """
        SELECT
            Tcontact.Id AS BaseId,
            Tcontact.SheetId,
            Tcontact.Type,
            Tcontact.DisplayName,
            Tcontact.Presentation,
            Tcontact.Url,
            Tcontact.ThumbnailUrl

        FROM Tcontact
        """;

}