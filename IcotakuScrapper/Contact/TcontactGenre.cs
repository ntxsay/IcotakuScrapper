using System.Diagnostics;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Contact;

public enum GenreSortBy
{
    Id,
    Name
}

/// <summary>
/// Représente un format de diffusion d'un anime ou Manga ou autre
/// </summary>
public class TcontactGenre
{
    public int Id { get; protected set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public TcontactGenre()
    {
    }

    public TcontactGenre(int id)
    {
        Id = id;
    }

    public TcontactGenre(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }
    
    public TcontactGenre(int id, string name, string? description = null)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public override string ToString()
    {
        return Name;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TcontactGenre ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TcontactGenre ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(string name,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TcontactGenre WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TcontactGenre[]> SelectAsync(GenreSortBy sortBy = GenreSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript;

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TcontactGenre>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TcontactGenre?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TcontactGenre?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    /// <summary>
    /// Insert un nouvel enregistrement dans la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");
        if (await ExistsAsync(Name, cancellationToken, cmd))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            INSERT INTO TcontactGenre
                (Id, Name, Description)
            VALUES
                ($Id, $Name, $Description)
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$Name", Name.Trim());
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

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

    /// <summary>
    /// Met à jour cet enregistrement de la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        var existingId = await GetIdOfAsync(Name, cancellationToken, cmd);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            UPDATE TcontactGenre SET
                Name = $Name,
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$Name", Name.Trim());
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

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

    /// <summary>
    /// Supprime cet enregistrement de la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    /// <summary>
    /// Supprime un enregistrement de la table TcontactGenre ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TcontactGenre WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0
                ? new OperationState(false, "Une erreur est survenue lors de la suppression")
                : new OperationState(true, "Suppression réussie");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    #endregion

    internal static TcontactGenre GetRecord(SqliteDataReader reader, int idIndex, int nameIndex, int descriptionIndex)
    {
        return new TcontactGenre()
        {
            Id = reader.GetInt32(idIndex),
            Name = reader.GetString(nameIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }


    private static async IAsyncEnumerable<TcontactGenre> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idIndex = reader.GetOrdinal("Id");
            var nameIndex = reader.GetOrdinal("Name");
            var descriptionIndex = reader.GetOrdinal("Description");
            yield return GetRecord(reader, idIndex, nameIndex, descriptionIndex);
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            Id,
            Name,
            Description
        FROM TcontactGenre
        """;
}