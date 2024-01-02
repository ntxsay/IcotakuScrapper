using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Contact;

public class TcontactWebSite
{
    public int Id { get; set; }
    public int IdContact { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }

    public override string ToString()
    {
        return $"{Url} ({Description})";
    }

    public TcontactWebSite()
    {
    }

    public TcontactWebSite(int id)
    {
        Id = id;
    }

    public TcontactWebSite(int id, int idContact)
    {
        Id = id;
        IdContact = idContact;
    }

    public TcontactWebSite(int idContact, string url, string? description)
    {
        IdContact = idContact;
        Url = url;
        Description = description;
    }

    public TcontactWebSite(int id, int idContact, string url, string? description)
    {
        Id = id;
        IdContact = idContact;
        Url = url;
        Description = description;
    }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactWebSite";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.IdContact,
        ]);

        if (!isColumnSelectValid)
            return 0;

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TcontactWebSite WHERE Id = $Id",
            IntColumnSelect.IdContact => "SELECT COUNT(Id) FROM TcontactWebSite WHERE IdContact = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TcontactWebSite ayant le nom spécifié
    /// </summary>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idContact, string url, CancellationToken? cancellationToken = null)
    {
        if (idContact <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TcontactWebSite WHERE IdContact = $IdContact AND Url = $Url COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$IdContact", idContact);
        command.Parameters.AddWithValue("$Url", url);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idContact, string url, CancellationToken? cancellationToken = null)
    {
        if (idContact <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TcontactWebSite WHERE IdContact = $IdContact AND Url = $Url COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$IdContact", idContact);
        command.Parameters.AddWithValue("$Url", url);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idContact, string url, CancellationToken? cancellationToken = null)
        => await CountAsync(idContact, url, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TcontactWebSite[]> SelectAsync(int idContact, OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdContact = $IdContact";

        command.Parameters.AddWithValue("$IdContact", idContact);
        command.CommandText += Environment.NewLine;
        command.CommandText += $" ORDER BY Url {orderBy}";

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TcontactWebSite?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TcontactWebSite?> SingleAsync(int idContact, string url, CancellationToken? cancellationToken = null)
    {
        if (idContact <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdContact = $IdContact AND Url = $Url COLLATE NOCASE";


        command.Parameters.AddWithValue("$IdContact", idContact);
        command.Parameters.AddWithValue("$Url", url);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le titre est invalide.");

        if (IdContact <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(IdContact, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "Le contact n'existe pas.");

        if (!disableVerification && await ExistsAsync(IdContact, Url, cancellationToken))
            return new OperationState<int>(false, "Le titre existe déjà.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TcontactWebSite 
                (IdContact, Url, Description) 
            VALUES 
                ($IdContact, $Url, $Description)
            """;

        

        command.Parameters.AddWithValue("$IdContact", IdContact);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result <= 0)
                return new OperationState<int>(false, "Une erreur inconnue est survenue.");
            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "Le titre alternatif a été ajouté.", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id est invalide.");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le titre est invalide.");

        if (IdContact <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(IdContact, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "Le contact n'existe pas.");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdContact, Url, cancellationToken);
            if (existingId != null && existingId != Id)
                return new OperationState(false, "Le titre existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TcontactWebSite 
            SET 
                IdContact = $IdContact, 
                Url = $Url, 
                Description = $Description
            WHERE Id = $Id
            """;

        

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdContact", IdContact);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "Une erreur inconnue est survenue.")
                : new OperationState(true, "Le titre alternatif a été modifié.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'id est invalide.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TcontactWebSite WHERE Id = $Id";

        

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "Une erreur inconnue est survenue.")
                : new OperationState(true, "Le titre alternatif a été supprimé.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    internal static TcontactWebSite GetRecord(SqliteDataReader reader, int idIndex, int idContactIndex, int urlIndex, int descriptionIndex)
    {
        return new TcontactWebSite()
        {
            Id = reader.GetInt32(idIndex),
            IdContact = reader.GetInt32(idContactIndex),
            Url = reader.GetString(urlIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }

    private static async IAsyncEnumerable<TcontactWebSite> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                idContactIndex: reader.GetOrdinal("IdContact"),
                urlIndex: reader.GetOrdinal("Url"),
                descriptionIndex: reader.GetOrdinal("Description"));
        }
    }


    private const string SqlSelectScript =
        """
        SELECT
            Id,
            IdContact,
            Url,
            Description
        FROM TcontactWebSite
        """;
}