using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects.Exceptions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public class TanimeSeasonalPlanningDistributor :ITableBase<TanimeSeasonalPlanningDistributor>
{
    public int Id { get; protected set; }
    public int IdSeasonalPlanning { get; set; }
    public TcontactBase Distributor { get; set; } = new ();

    public TanimeSeasonalPlanningDistributor()
    {
    }
    
    public TanimeSeasonalPlanningDistributor(int id, int idSeasonalPlanning, TcontactBase distributor)
    {
        Id = id;
        IdSeasonalPlanning = idSeasonalPlanning;
        Distributor = distributor;
    }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = "SELECT COUNT(Id) FROM TanimeSeasonalPlanningDistributor";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeSeasonalPlanningDistributor WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountByPlanningAsync(int idPlanning, CancellationToken? cancellationToken = null)
        => await CountAsync(idPlanning, IntColumnSelect.IdSeasonalPlanning, cancellationToken);
    
    public static async Task<int> CountByDistributorAsync(int idDistributor, CancellationToken? cancellationToken = null)
        => await CountAsync(idDistributor, IntColumnSelect.IdDistributor, cancellationToken);
    
    private static async Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        IntColumnSelectException.ThrowNotSupportedException(columnSelect, nameof(columnSelect), [IntColumnSelect.Id, IntColumnSelect.IdContact, IntColumnSelect.IdDistributor, IntColumnSelect.IdSeasonalPlanning]);
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TanimeSeasonalPlanningDistributor WHERE Id = $Id",
            IntColumnSelect.IdSeasonalPlanning => "SELECT COUNT(Id) FROM TanimeSeasonalPlanningDistributor WHERE IdSeasonalPlanning = $Id",
            IntColumnSelect.IdContact or IntColumnSelect.IdDistributor => "SELECT COUNT(Id) FROM TanimeSeasonalPlanningDistributor WHERE IdDistributor = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(int idPlanning, int idDistributor, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            SELECT
                COUNT(Id)
            FROM TanimeSeasonalPlanningDistributor
            WHERE IdSeasonalPlanning = $IdPlanning AND IdDistributor = $IdDistributor
            """;

        command.Parameters.AddWithValue("$IdPlanning", idPlanning);
        command.Parameters.AddWithValue("$IdDistributor", idDistributor);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idPlanning, int idDistributor,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            SELECT
                Id
            FROM TanimeSeasonalPlanningDistributor
            WHERE IdSeasonalPlanning = $IdPlanning AND IdDistributor = $IdDistributor
            """;

        command.Parameters.AddWithValue("$IdPlanning", idPlanning);
        command.Parameters.AddWithValue("$IdDistributor", idDistributor);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is int id)
            return id;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;
    
    public static async Task<bool> ExistsAsync(int idPlanning, int idDistributor, CancellationToken? cancellationToken = null)
        => await CountAsync(idPlanning, idDistributor, cancellationToken) > 0;
    
    public static async Task<bool> ExistsByPlanningAsync(int idPlanning, CancellationToken? cancellationToken = null)
        => await CountByPlanningAsync(idPlanning, cancellationToken) > 0;
    
    public static async Task<bool> ExistsByDistributorAsync(int idDistributor, CancellationToken? cancellationToken = null)
        => await CountByDistributorAsync(idDistributor, cancellationToken) > 0;

    #endregion

    #region Select

    /// <summary>
    /// Retourne les distributeurs de l'anime spécifié
    /// </summary>
    /// <param name="idPlanning"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<TcontactBase> GetDistributorsAsync(int idPlanning, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = "SELECT DISTINCT IdDistributor FROM TanimeSeasonalPlanningDistributor WHERE IdSeasonalPlanning = $IdPlanning";

        command.Parameters.AddWithValue("$IdPlanning", idPlanning);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            yield break;
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var id = reader.GetInt32(reader.GetOrdinal("IdDistributor"));
            var contact = await TcontactBase.SingleAsync(id, IntColumnSelect.Id, cancellationToken);
            if (contact == null)
                continue;

            yield return contact;
        }
    }

    public static async Task<TanimeSeasonalPlanningDistributor[]> SelectAsync(int id, IntColumnSelect columnSelect, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        IntColumnSelectException.ThrowNotSupportedException(columnSelect, nameof(columnSelect), [IntColumnSelect.Id, IntColumnSelect.IdContact, IntColumnSelect.IdDistributor, IntColumnSelect.IdSeasonalPlanning]);
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = IcotakuSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE TanimeSeasonalPlanningDistributor.Id = $Id",
            IntColumnSelect.IdAnime => "WHERE TanimeSeasonalPlanningDistributor.IdSeasonalPlanning = $Id",
            IntColumnSelect.IdContact or IntColumnSelect.IdDistributor => "WHERE TanimeSeasonalPlanningDistributor.IdDistributor = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.AddLimitOffset(limit, skip);

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeSeasonalPlanningDistributor?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = IcotakuSelectScript + Environment.NewLine + "WHERE TanimeSeasonalPlanningDistributor.Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeSeasonalPlanningDistributor?> SingleAsync(int idPlanning, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = IcotakuSelectScript + Environment.NewLine + "WHERE TanimeSeasonalPlanningDistributor.IdSeasonalPlanning = $IdSeasonalPlanning AND TanimeSeasonalPlanningDistributor.IdDistributor = $IdDistributor";

        command.Parameters.AddWithValue("$IdSeasonalPlanning", idPlanning);
        command.Parameters.AddWithValue("$IdDistributor", idContact);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (IdSeasonalPlanning <= 0 || (!disableVerification && !await TanimeSeasonalPlanning.ExistsAsync(IdSeasonalPlanning, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "Ce planning n'existe pas.");

        if (Distributor.Id <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(Distributor.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "Le distributeur n'existe pas.");
        
        if (!disableVerification && await ExistsAsync(IdSeasonalPlanning, Distributor.Id, cancellationToken))
            return new OperationState<int>(false, "Le lien existe déjà.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            INSERT INTO TanimeSeasonalPlanningDistributor 
                (IdSeasonalPlanning, IdDistributor) 
            VALUES 
                ($IdSeasonalPlanning, $IdDistributor);
            """;

        command.Parameters.AddWithValue("$IdSeasonalPlanning", IdSeasonalPlanning);
        command.Parameters.AddWithValue("$IdDistributor", Distributor.Id);
        
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

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (IdSeasonalPlanning <= 0 || (!disableVerification && !await TanimeSeasonalPlanning.ExistsAsync(IdSeasonalPlanning, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "Ce planning n'existe pas.");

        if (Distributor.Id <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(Distributor.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "Le distributeur n'existe pas.");
        
        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdSeasonalPlanning, Distributor.Id, cancellationToken);
            if (existingId is not null && existingId != Id)
                return new OperationState(false, "Le lien existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TanimeSeasonalPlanningDistributor SET 
                IdSeasonalPlanning = $IdSeasonalPlanning, 
                IdDistributor = $IdDistributor
            WHERE Id = $Id;
            """;

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdSeasonalPlanning", IdSeasonalPlanning);
        command.Parameters.AddWithValue("$IdDistributor", Distributor.Id);

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

    public static async Task<TanimeSeasonalPlanningDistributor?> SingleOrCreateAsync(TanimeSeasonalPlanningDistributor value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.IdSeasonalPlanning, value.Distributor.Id, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.IdSeasonalPlanning, value.Distributor.Id, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(TanimeSeasonalPlanningDistributor value, CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdSeasonalPlanning, value.Distributor.Id, cancellationToken);

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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeSeasonalPlanningDistributor WHERE Id = $Id";

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
    
    private static async IAsyncEnumerable<TanimeSeasonalPlanningDistributor> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var record = new TanimeSeasonalPlanningDistributor()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdSeasonalPlanning = reader.GetInt32(reader.GetOrdinal("IdSeasonalPlanning")),
                
            };

            var idContact = reader.GetInt32(reader.GetOrdinal("IdDistributor"));
            var contact = await TcontactBase.SingleAsync(idContact, IntColumnSelect.Id, cancellationToken);
            if (contact == null)
                continue;

            record.Distributor = contact;

            yield return record;
        }
    }


    private const string IcotakuSelectScript =
        """
        SELECT
            Id,
            IdSeasonalPlanning,
            IdDistributor
        FROM TanimeSeasonalPlanningDistributor
        """;
}