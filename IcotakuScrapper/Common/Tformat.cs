using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum FormatSortBy
{
    Id,
    Name
}

/// <summary>
/// Représente un format de diffusion d'un anime ou Manga ou autre
/// </summary>
public partial class Tformat
{
    public int Id { get; protected set; }
    public string Name { get; set; } = null!;
    public IcotakuSection Section { get; set; }
    public string? Description { get; set; }

    public Tformat()
    {
    }

    public Tformat(int id)
    {
        Id = id;
    }

    public Tformat(IcotakuSection section, string name, string? description = null)
    {
        Section = section;
        Name = name;
        Description = description;
    }

    public Tformat(int id, IcotakuSection section, string name, string? description = null)
    {
        Id = id;
        Section = section;
        Name = name;
        Description = description;
    }

    public override string ToString()
    {
        return Name;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tformat
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tformat";

        
            

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tformat ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tformat WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tformat ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tformat WHERE Name = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tformat WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(string name, IcotakuSection section,
        CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Tformat WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null)
        => await CountAsync(name, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
        => await CountAsync(name, section, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<Tformat[]> SelectAsync(FormatSortBy sortBy = FormatSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript;

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";


        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }


    public static async Task<Tformat[]> SelectAsync(IcotakuSection section, FormatSortBy sortBy = FormatSortBy.Name,
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

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<Tformat?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<Tformat?> SingleAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Section = $Section AND Name = $Name COLLATE NOCASE";

        

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
    /// Insert un nouvel enregistrement dans la table Tformat
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="disableVerification">Active ou désactive les opération de vérification de l'existence de l'item</param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");

        if (!disableVerification && await ExistsAsync(Name, Section, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Tformat
                (Name, Section, Description)
            VALUES
                ($Name, $Section, $Description)
            """;

        

        command.Parameters.AddWithValue("$Name", Name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)Section);
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

    #endregion

    #region Update

    /// <summary>
    /// Met à jour cet enregistrement de la table Tformat
    /// </summary>
    /// <param name="disableVerification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationState> UpdateAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
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
            UPDATE Tformat SET
                Name = $Name,
                Section = $Section,
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
                ? new OperationState(false, "Aucune mise à jour n'a été effectuée.")
                : new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region Single or Create or Update

    public static async Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<Tformat> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO Tformat (Name, Section, Description)";

        

        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);

            if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom de l'item ne peut pas être vide (id: {i}");
                continue;
            }

            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($Name{i}, $Section{i}, $Description{i})";

            command.Parameters.AddWithValue($"$Name{i}", value.Name.Trim());
            command.Parameters.AddWithValue($"$Section{i}", (byte)value.Section);
            command.Parameters.AddWithValue($"$Description{i}", value.Description ?? (object)DBNull.Value);

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
    /// Retourne l'enregistrement de la table Tformat ayant l'identifiant spécifié ou l'insert si l'enregistrement n'existe pas
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Tformat?> SingleOrCreateAsync(Tformat value, bool reloadIfExist = false, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        
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

    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState> AddOrUpdateAsync(Tformat value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.Name, value.Section, cancellationToken);

        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (existingId.Value != value.Id)
                return new OperationState(false, "Le nom de l'item existe déjà");

            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (value.Id > 0 && existingId.Value != value.Id)
                value.Id = existingId.Value;
            return await value.UpdateAsync(true, cancellationToken);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAsync(true, cancellationToken);
        if (addResult.IsSuccess)
            value.Id = addResult.Data;

        return addResult.ToBaseState();
    }

    #endregion

    #region Delete

    /// <summary>
    /// Supprime les enregistrements de la table Tformat qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(string formatName, IcotakuSection formatSection)> actualValues, CancellationToken? cancellationToken = null)
    {

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Tformat WHERE Name NOT IN (";
        
        var i = 0;
        foreach (var (formatName, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$Name{i}" : $", $Name{i}";
            command.Parameters.AddWithValue($"$Name{i}", formatName);
            i++;
        }
        command.CommandText += ") AND Section NOT IN (";
        i = 0;
        foreach (var (_, formatSection) in actualValues)
        {
            command.CommandText += i == 0 ? $"$Section{i}" : $", $Section{i}";
            command.Parameters.AddWithValue($"$Section{i}", (byte)formatSection);
            i++;
        }
        command.CommandText += ")";

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    /// <summary>
    /// Supprime cet enregistrement de la table Tformat
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    /// <summary>
    /// Supprime un enregistrement de la table Tformat ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Tformat WHERE Id = $Id";

        

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Tformat WHERE Section = $Section";

        

        command.Parameters.AddWithValue("$Section", (byte)section);

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

    internal static Tformat GetRecord(SqliteDataReader reader, int idIndex, int sectionIndex, int nameIndex, int descriptionIndex)
    {
        return new Tformat()
        {
            Id = reader.GetInt32(idIndex),
            Section = (IcotakuSection)reader.GetByte(sectionIndex),
            Name = reader.GetString(nameIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }


    private static async IAsyncEnumerable<Tformat> GetRecords(SqliteDataReader reader,
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
        FROM Tformat
        """;
}