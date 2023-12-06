using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum TargetSortBy
{
    Id,
    Name
}

/// <summary>
/// Représente le public visé (ou cible démographique)  d'un anime ou Manga ou autre
/// </summary>
public partial class Ttarget
{
    public int Id { get; protected set; }
    public IcotakuSection Section { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public Ttarget()
    {
    }

    public Ttarget(int id)
    {
        Id = id;
    }

    public Ttarget(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    public Ttarget(int id, string name, string? description = null)
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
    /// Compte le nombre d'entrées dans la table Ttarget
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Ttarget ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Ttarget ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Ttarget WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
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

    public static async Task<bool> ExistsAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, section, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<Ttarget[]> SelectAsync(TargetSortBy sortBy = TargetSortBy.Name,
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
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<Ttarget[]> SelectAsync(IcotakuSection section, TargetSortBy sortBy = TargetSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Section = $Section";

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Section", (byte)section);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<Ttarget?> SingleAsync(int id, CancellationToken? cancellationToken = null,
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

    public static async Task<Ttarget?> SingleAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    /// <summary>
    /// Insert un nouvel enregistrement dans la table Ttarget
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");
        if (await ExistsAsync(Name, Section, cancellationToken, cmd))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            INSERT INTO Ttarget
                (Name, Section, Description)
            VALUES
                ($Name, $Section, $Description)
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", (byte)Section);
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
            LogServices.LogDebug(e);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public static async Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<Ttarget> values, 
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.StartWithInsertMode(insertMode);
        
        command.CommandText += " INTO Ttarget (Name, Section, Description) VALUES ";

        command.Parameters.Clear();

        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);

            if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom de l'item ne peut pas être vide (id: {i}");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($Name{i}, $Section{i}, $Description{i})";

            command.Parameters.AddWithValue($"$Name{i}", value.Name.Trim());
            command.Parameters.AddWithValue($"$Section{i}", (byte)value.Section);
            command.Parameters.AddWithValue($"$Description{i}", value.Description ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
            
            LogServices.LogDebug("Ajout de l'item " + value.Name + " à la commande.");
        }

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0, $"{count} enregistrement(s) sur {values.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }
    
    /// <summary>
    /// Retourne l'enregistrement de la table Ttarget ayant l'identifiant spécifié ou l'insert si l'enregistrement n'existe pas
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Ttarget?> SingleOrCreateAsync(Ttarget value, bool reloadIfExist= false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.Parameters.Clear();
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.Name, value.Section, cancellationToken, command);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.Name, value.Section, cancellationToken, command);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(cancellationToken, command);
        return !result.IsSuccess ? null : value;
    }
    #endregion

    #region Update

    /// <summary>
    /// Met à jour cet enregistrement de la table Ttarget
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        var existingId = await GetIdOfAsync(Name, Section, cancellationToken, cmd);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            UPDATE Ttarget SET
                Section = $Section,
                Name = $Name,
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$Section", (byte)Section);
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
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region Delete

    /// <summary>
    /// Supprime cet enregistrement de la table Ttarget
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    /// <summary>
    /// Supprime un enregistrement de la table Ttarget ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM Ttarget WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }
    
    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM Ttarget WHERE Section = $Section";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", section);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    #endregion

    internal static Ttarget GetRecord(SqliteDataReader reader, int idIndex, int sectionIndex, int nameIndex, int descriptionIndex)
    {
        return new Ttarget()
        {
            Id = reader.GetInt32(idIndex),
            Section = (IcotakuSection)reader.GetByte(sectionIndex),
            Name = reader.GetString(nameIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }


    private static async IAsyncEnumerable<Ttarget> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idIndex = reader.GetOrdinal("Id");
            var sectionIndex = reader.GetOrdinal("Section");
            var nameIndex = reader.GetOrdinal("Name");
            var descriptionIndex = reader.GetOrdinal("Description");
            yield return GetRecord(reader, idIndex, sectionIndex, nameIndex, descriptionIndex);
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            Id,
            Section,
            Name,
            Description
        FROM Ttarget
        """;
}