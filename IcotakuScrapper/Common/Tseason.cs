using System.Diagnostics;
using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum SeasonSortBy
{
    /// <summary>
    /// Tri par défaut (Id)
    /// </summary>
    Default = 0,
    /// <summary>
    /// Tri par année
    /// </summary>
    Year = 1,
    /// <summary>
    /// Tri par numéro de saison
    /// </summary>
    SeasonNumber = 2,
    /// <summary>
    /// Tri par nom
    /// </summary>
    Name = 3
}

public class Tseason
{
    public int Id { get; protected set; }
    public string DisplayName { get; set; } = string.Empty;
    public uint SeasonNumber { get; set; }
    
    public Tseason()
    {
    }
    
    public Tseason(int id)
    {
        Id = id;
    }
    
    public Tseason(int id, string displayName, uint seasonNumber)
    {
        Id = id;
        DisplayName = displayName;
        SeasonNumber = seasonNumber;
    }
    
    
    public override string ToString()
        => DateHelpers.GetSeasonLiteral(SeasonNumber) ?? $"{DisplayName} ({SeasonNumber})";

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tseason ayant le nom spécifié
    /// </summary>
    /// <param name="displayName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string displayName, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason WHERE DisplayName = $DisplayName COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$DisplayName", displayName.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tseason ayant le nom spécifié
    /// </summary>
    /// <param name="seasonNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(uint seasonNumber, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason WHERE Tseason.SeasonNumber = $SeasonNumber";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int?> GetIdOfAsync(uint seasonNumber, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tseason WHERE Tseason.SeasonNumber = $SeasonNumber";

        command.Parameters.Clear();
        
       command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
     => await CountAsync(id, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(string displayName, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(displayName, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(uint seasonNumber, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await CountAsync(seasonNumber, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<Tseason[]> SelectAsync(SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += sortBy switch
        {
            SeasonSortBy.Year => $"ORDER BY Year {orderBy}",
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<Tseason[]> SelectAsync(uint year, SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Year = $Year";
        
        command.CommandText += sortBy switch
        {
            SeasonSortBy.Year => $"ORDER BY Year {orderBy}",
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Year", year);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<Tseason[]> SelectAsync(byte seasonNumber, SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE SeasonNumber = $SeasonNumber";
        
        command.CommandText += sortBy switch
        {
            SeasonSortBy.Year => $"ORDER BY Year {orderBy}",
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<Tseason?> SingleAsync(int id, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        
        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader, 
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }
    
    public static async Task<Tseason?> SingleAsync(uint seasonNumber, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE SeasonNumber = $SeasonNumber";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        
        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader, 
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }
    
    public static async Task<Tseason?> SingleAsync(string displayName, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE DisplayName = $DisplayName COLLATE NOCASE";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", displayName.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        
        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader, 
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (!DateHelpers.IsSeasonValidated(SeasonNumber))
            return new OperationState<int>(false, "Le numéro de la saison est invalide");
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            DisplayName = DateHelpers.GetSeasonLiteral(SeasonNumber) ?? string.Empty;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        if (await ExistsAsync(SeasonNumber, cancellationToken, command))
            return new OperationState<int>(false, "Une saison avec le même nom existe déjà");
        
        command.CommandText = 
            """
            INSERT INTO Tseason
                (DisplayName, SeasonNumber)
            VALUES
                ($DisplayName, $SeasonNumber)
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", DisplayName.Trim());
        command.Parameters.AddWithValue("$SeasonNumber", SeasonNumber);
        
        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");

            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "Insertion réussie", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");
        }
    }


    #endregion
    
    #region Update
    
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");
        
        if (!DateHelpers.IsSeasonValidated(SeasonNumber))
            return new OperationState(false, "Le numéro de la saison est invalide");
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            DisplayName = DateHelpers.GetSeasonLiteral(SeasonNumber) ?? string.Empty;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        var existingId = await GetIdOfAsync(SeasonNumber, cancellationToken, command);
        if (existingId is not null && existingId != Id)
            return new OperationState(false, "Une saison avec le même nom existe déjà");
        
        command.CommandText = 
            """
            UPDATE Tseason
            SET
                DisplayName = $DisplayName,
                SeasonNumber = $SeasonNumber
            WHERE Id = $Id
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$DisplayName", DisplayName.Trim());
        command.Parameters.AddWithValue("$SeasonNumber", SeasonNumber);
        
        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0 
                ? new OperationState(false, "Une erreur est survenue lors de la mise à jour") 
                : new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }
    
    #endregion
    
    #region Delete
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
     => await DeleteAsync(Id, cancellationToken, cmd);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        
        command.CommandText = 
            """
            UPDATE Tanime SET IdSeason = NULL WHERE IdSeason = $Id;
            DELETE FROM TanimeSeasonalPlanning WHERE IdSeason = $Id;
            DELETE FROM Tseason  WHERE Id = $Id;
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} ligne(s) supprimée(s)");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }
    
    public static async Task<OperationState> DeleteAsync(uint seasonNumber, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (seasonNumber <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var id = await GetIdOfAsync(seasonNumber, cancellationToken, command);
        if (id is null)
            return new OperationState(false, "La saison n'existe pas");
        
        return await DeleteAsync(id.Value, cancellationToken, command);
    }
    
    #endregion
    
    internal static Tseason GetRecord(SqliteDataReader reader, int idIndex, int seasonNumberIndex, int displayNameIndex)
    {
        return new Tseason()
        {
            Id = reader.GetInt32(idIndex),
            DisplayName = reader.GetString(displayNameIndex),
            SeasonNumber = (uint)reader.GetInt32(seasonNumberIndex)
        };
    }
    


    private static async IAsyncEnumerable<Tseason> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader, 
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        }
    }

    
    private const string SqlSelectScript = 
        """
        SELECT 
            Id, 
            DisplayName, 
            SeasonNumber 
        FROM Tseason
        """;
}