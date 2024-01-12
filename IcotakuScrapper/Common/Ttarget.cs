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
public partial class Ttarget : ITableNameDescriptionBase<Ttarget>
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
    
    #region Copy/Clone

    public void Copy(Ttarget value)
    {
        Id = value.Id;
        Name = value.Name;
        Section = value.Section;
        Description = value.Description;
    }
    
    public Ttarget Clone()
    {
        var clone = new Ttarget();
        clone.Copy(this);
        return clone;
    }

    #endregion


    #region Count

    
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget";

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is long count)
                return (int)count;
            return 0;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return 0;
        }
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is long count)
                return (int)count;
            return 0;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return 0;
        }
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Ttarget ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Ttarget WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is long count)
                return (int)count;
            return 0;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return 0;
        }
    }

    public static async Task<int?> GetIdOfAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Ttarget WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is long count)
                return (int)count;
            return null;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
        => await CountAsync(name, section, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<Ttarget[]> SelectAsync(TargetSortBy sortBy = TargetSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript;

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            if (!reader.HasRows)
                return [];

            return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return [];
        }
    }
    
    public static async Task<Ttarget[]> SelectAsync(IcotakuSection section, TargetSortBy sortBy = TargetSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Section = $Section";

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            if (!reader.HasRows)
                return [];

            return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return [];
        }
    }

    #endregion

    #region Single

    public static async Task<Ttarget?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Id = $Id";
        try
        {
            command.Parameters.AddWithValue("$Id", id);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            if (!reader.HasRows)
                return null;

            return await GetRecords(reader, cancellationToken)
                .SingleOrDefaultAsync(cancellationToken ?? CancellationToken.None);

        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    public static async Task<Ttarget?> SingleAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            if (!reader.HasRows)
                return null;

            return await GetRecords(reader, cancellationToken)
                .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");
        if (!disableVerification && await ExistsAsync(Name, Section, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Ttarget
                (Name, Section, Description)
            VALUES
                ($Name, $Section, $Description)
            """;
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
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        
        command.CommandText += " INTO Ttarget (Name, Section, Description) VALUES ";
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
    /// <returns></returns>
    public static async Task<Ttarget?> SingleOrCreateAsync(Ttarget value, bool reloadIfExist= false, CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.Name, value.Section, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.Name, value.Section, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }
    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(Name, Section, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "Le nom de l'item existe déjà");
        }
        
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Ttarget SET
                Section = $Section,
                Name = $Name,
                Description = $Description
            WHERE Id = $Id
            """;
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
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    /// <summary>
    /// Supprime un enregistrement de la table Ttarget ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Ttarget WHERE Id = $Id";
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
    
    internal static async Task<OperationState> DeleteAsync(IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Ttarget WHERE Section = $Section";
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

    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Id,
            Section,
            Name,
            Description
        FROM Ttarget
        """;
}