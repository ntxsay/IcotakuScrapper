using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;


public class TanimeLicense
{
    public int Id { get; protected set; }
    public int IdAnime { get; set; }

    public TlicenseType Type { get; set; } = new();
    public TcontactBase Distributor { get; set; } = new ();
    
    public TanimeLicense()
    {
    }

    public TanimeLicense(int id)
    {
        Id = id;
    }
    
    public TanimeLicense(int idAnime, Tcontact distributor)
    {
        IdAnime = idAnime;
        Distributor = distributor;
    }
    
     #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeLicense
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeLicense";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeLicense ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeLicense WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeLicense ayant le nom spécifié
    /// </summary>
    /// <param name="idContact"></param>
    /// <param name="idLicenseType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, int idLicenseType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeLicense WHERE IdAnime = $IdAnime AND IdDistributor = $IdDistributor AND IdLicenseType = $IdLicenseType";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdDistributor", idContact);
        command.Parameters.AddWithValue("$IdLicenseType", idLicenseType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, int idLicenseType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeLicense WHERE IdAnime = $IdAnime AND IdDistributor = $IdDistributor AND IdLicenseType = $IdLicenseType";

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdDistributor", idContact);
        command.Parameters.AddWithValue("$IdLicenseType", idLicenseType);
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

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, int idLicenseType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, idContact, idLicenseType, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeLicense[]> SelectAsync(int id, IntColumnSelect columnSelect, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0 , CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.IdAnime,
            IntColumnSelect.IdDistributor,
            IntColumnSelect.IdLicenseType,
        ]);
        
        if (!isColumnSelectValid)
        {
            return [];
        }

        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE TanimeLicense.Id = $Id",
            IntColumnSelect.IdAnime => "WHERE TanimeLicense.IdAnime = $Id",
            IntColumnSelect.IdDistributor => "WHERE TanimeLicense.IdDistributor = $Id",
            IntColumnSelect.IdLicenseType => "WHERE TanimeLicense.IdLicenseType = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.CommandText += Environment.NewLine + $"ORDER BY Tcontact.DisplayName {orderBy}";
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeLicense?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeLicense.Id = $Id";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<TanimeLicense?> SingleAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeLicense.IdAnime = $IdAnime AND TanimeLicense.IdDistributor = $IdDistributor";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdDistributor", idContact);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "L'anime n'existe pas.");
        
        if (Distributor.Id <= 0 || !await Tcontact.ExistsAsync(Distributor.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le distributor n'existe pas.");
        
        if (Type.Id <= 0 || !await TlicenseType.ExistsAsync(Type.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le type de licence n'existe pas.");
        
        if (await ExistsAsync(IdAnime, Distributor.Id, Type.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le lien existe déjà.");
        
        command.CommandText = "INSERT INTO TanimeLicense (IdAnime, IdDistributor, IdLicenseType) VALUES ($IdAnime, $IdDistributor, $IdLicenseType);";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdDistributor", Distributor.Id);
        command.Parameters.AddWithValue("$IdLicenseType", Type.Id);
        
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

    public static async Task<OperationState> InsertAsync(int idAnime, IReadOnlyCollection<TanimeLicense> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun distributor n'a été sélectionné.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");
        
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeLicense (IdAnime, IdDistributor, IdLicenseType) VALUES";
        command.Parameters.Clear();
        
        for (var i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt(i);
            if (value.Distributor.Id <= 0)
            {
                LogServices.LogDebug($"L'identifiant  du distributor ne peut pas être égal ou inférieur à 0.");
                continue;
            }
            
            if (value.Type.Id <= 0)
            {
                LogServices.LogDebug($"L'identifiant  du type de licence ne peut pas être égal ou inférieur à 0.");
                continue;
            }
            
            command.CommandText += Environment.NewLine + $"($IdAnime, $IdDistributor{i}, $IdLicenseType{i})";
            command.Parameters.AddWithValue($"$IdDistributor{i}", value.Distributor.Id);
            command.Parameters.AddWithValue($"$IdLicenseType{i}", value.Type.Id);
            
            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }
        
        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun distributeur n'a été sélectionné.");
        
        command.Parameters.AddWithValue("$IdAnime", idAnime);
        
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
    #endregion
    
    #region Update
    
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");
        
        if (Distributor.Id <= 0 || !await Tcontact.ExistsAsync(Distributor.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "Le distributor n'existe pas.");
        
        if (Type.Id <= 0 || !await TlicenseType.ExistsAsync(Type.Id, cancellationToken, command))
            return new OperationState(false, "Le type de licence n'existe pas.");
        
        var existingId = await GetIdOfAsync(IdAnime, Distributor.Id, Type.Id, cancellationToken, command);
        if (existingId is not null && existingId != Id)
            return new OperationState(false, "Le lien existe déjà.");
        
        command.CommandText = 
            """
            UPDATE TanimeLicense SET 
                IdAnime = $IdAnime, 
                IdDistributor = $IdDistributor,
                IdLicenseType = $IdLicenseType
            WHERE Id = $Id;
            """;
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdDistributor", Distributor.Id);
        command.Parameters.AddWithValue("$IdLicenseType", Type.Id);
        
        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState(false, "Une erreur est survenue lors de la mise à jour");

            return new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
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
        command.CommandText = "DELETE FROM TanimeLicense WHERE Id = $Id";
        
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
            return new OperationState(false, "Une erreur est survenue lors de la suppression du lien");
        }
    }

    #endregion

    private static async IAsyncEnumerable<TanimeLicense> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var record = new TanimeLicense()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdAnime = reader.GetInt32(reader.GetOrdinal("IdAnime")),
                Distributor = new TcontactBase(reader.GetOrdinal("IdDistributor"), reader.GetGuid(reader.GetOrdinal("Guid")))
                {
                    SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                    Type = (ContactType)reader.GetByte(reader.GetOrdinal("Type")),
                    Url = reader.GetString(reader.GetOrdinal("Url")),
                    DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                    Presentation = reader.IsDBNull(reader.GetOrdinal("Presentation")) ? null : reader.GetString(reader.GetOrdinal("Presentation")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl")) ? null : reader.GetString(reader.GetOrdinal("ThumbnailUrl")),
                },
                Type = new TlicenseType(reader.GetInt32(reader.GetOrdinal("IdLicenseType")))
                {
                    Name = reader.GetString(reader.GetOrdinal("LicenceTypeName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("LicenceTypeDescription")) ? null : reader.GetString(reader.GetOrdinal("LicenceTypeDescription")),
                    Section = (IcotakuSection)reader.GetByte(reader.GetOrdinal("LicenceTypeSection"))
                }
            };

            yield return record;
        }
    }
    
    private const string SqlSelectScript =
        """
        SELECT
            TanimeLicense.Id AS BaseId,
            TanimeLicense.IdAnime,
            TanimeLicense.IdDistributor,
            TanimeLicense.IdLicenseType,
            
            TlicenseType.Name AS LicenceTypeName,
            TlicenseType.Description AS LicenceTypeDescription,
            TlicenseType.Section AS LicenceTypeSection,
            
            Tcontact.SheetId,
            Tcontact.Guid,
            Tcontact.Type,
            Tcontact.DisplayName,
            Tcontact.Presentation,
            Tcontact.Url,
            Tcontact.ThumbnailUrl
        
        FROM TanimeLicense
            LEFT JOIN main.TlicenseType ON TanimeLicense.IdLicenseType = TlicenseType.Id
        LEFT JOIN main.Tcontact Tcontact on Tcontact.Id = TanimeLicense.IdDistributor
        """;
}