using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Contact;

public enum GenreSortBy
{
    Id,
    Name
}

/// <summary>
/// Représente un format de diffusion d'un anime ou Manga ou autre
/// </summary>
public class TcontactGenre : ITableNameDescriptionBase<TcontactGenre>
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
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre";

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
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre WHERE Id = $Id";

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
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactGenre WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    static Task<int> ITableNameDescriptionBase<TcontactGenre>.CountAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static async Task<int?> GetIdOfAsync(string name,
        CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TcontactGenre WHERE Name = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    static Task<int?> ITableNameDescriptionBase<TcontactGenre>.GetIdOfAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null)
        => await CountAsync(name, cancellationToken) > 0;

    static Task<bool> ITableNameDescriptionBase<TcontactGenre>.ExistsAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }


    #endregion

    #region Select

    public static async Task<TcontactGenre[]> SelectAsync(GenreSortBy sortBy = GenreSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript;

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        
            

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TcontactGenre?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TcontactGenre?> SingleAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    static Task<TcontactGenre?> ITableNameDescriptionBase<TcontactGenre>.SingleAsync(string name, IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Insert

    /// <summary>
    /// Insert un nouvel enregistrement dans la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");
        if (!disableVerification && await ExistsAsync(Name, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TcontactGenre
                (Name, Description)
            VALUES
                ($Name, $Description)
            """;

        command.Parameters.AddWithValue("$Name", Name.Trim());
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Aucune insertion n'a été effectuée");

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
    /// <returns></returns>
    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(Name, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "Le nom de l'item existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TcontactGenre SET
                Name = $Name,
                Description = $Description
            WHERE Id = $Id
            """;

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

    #region AddOrUpdate
    static Task<OperationState> ITableNameDescriptionBase<TcontactGenre>.InsertOrReplaceAsync(IReadOnlyCollection<TcontactGenre> values, DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public static async Task<TcontactGenre?> SingleOrCreateAsync(TcontactGenre value, bool reloadIfExist = false, CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.Name, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.Name, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(TcontactGenre value, CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");    

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.Name, cancellationToken);

        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            /*
             * Si l'id de la item actuel n'est pas neutre c'est-à-dire que l'id n'est pas inférieur ou égal à 0
             * Et qu'il existe un Id correspondant à un enregistrement dans la base de données
             * mais que celui-ci ne correspond pas à l'id de l'item actuel
             * alors l'enregistrement existe déjà et on annule l'opération
             */
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState<int>(false, "Un item autre que celui-ci existe déjà");

            /*
             * Si l'id de l'item actuel est neutre c'est-à-dire que l'id est inférieur ou égal à 0
             * alors on met à jour l'id de l'item actuel avec l'id de l'enregistrement existant
             */
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;

            //On met à jour l'enregistrement
            return (await value.UpdateAsync(true, cancellationToken)).ToGenericState(value.Id);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAsync(true, cancellationToken);
        return addResult;
    }

    #endregion

    #region Delete

    /// <summary>
    /// Supprime cet enregistrement de la table TcontactGenre
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    /// <summary>
    /// Supprime un enregistrement de la table TcontactGenre ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Tcontact SET IdGenre = NULL WHERE IdGenre = $Id;
            DELETE FROM TcontactGenre WHERE Id = $Id;
            """;

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