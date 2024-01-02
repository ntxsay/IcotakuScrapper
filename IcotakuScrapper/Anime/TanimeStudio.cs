using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public enum AnimeStudioIdSelector
{
    Id,
    IdAnime,
    IdStudio,
}

public class TanimeStudio
{
    public int Id { get; protected set; }
    public int IdAnime { get; set; }
    public TcontactBase Studio { get; set; } = new();

    public TanimeStudio()
    {
    }

    public TanimeStudio(int id)
    {
        Id = id;
    }

    public TanimeStudio(int idAnime, TcontactBase studio)
    {
        IdAnime = idAnime;
        Studio = studio;
    }


    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStudio
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStudio ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStudio ayant le nom spécifié
    /// </summary>
    /// <param name="idContact"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio WHERE IdAnime = $IdAnime AND IdStudio = $IdStudio";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeStudio WHERE IdAnime = $IdAnime AND IdStudio = $IdStudio";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
        => await CountAsync(idAnime, idContact, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TanimeStudio[]> SelectAsync(int id, AnimeStudioIdSelector selector, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + selector switch
        {
            AnimeStudioIdSelector.Id => "WHERE TanimeStudio.Id = $Id",
            AnimeStudioIdSelector.IdAnime => "WHERE TanimeStudio.IdAnime = $Id",
            AnimeStudioIdSelector.IdStudio => "WHERE TanimeStudio.IdStudio = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
        };

        command.CommandText += Environment.NewLine + $"ORDER BY Tcontact.DisplayName {orderBy}";

        command.AddLimitOffset(limit, skip);

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeStudio?> SingleAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeStudio.IdAnime = $IdAnime AND TanimeStudio.IdStudio = $IdStudio";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (IdAnime <= 0 || (!disableVerification && !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "L'anime n'existe pas.", 0);

        if (Studio.Id <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(Studio.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "Le studio n'existe pas.", 0);

        if (!disableVerification && await ExistsAsync(IdAnime, Studio.Id, cancellationToken))
            return new OperationState<int>(false, "Le lien existe déjà.", 0);

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TanimeStudio 
                (IdAnime, IdStudio) 
            VALUES ($IdAnime, $IdStudio);
            """;

        

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdStudio", Studio.Id);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Aucun enregistrement n'a été inséré.");

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

    public async Task<OperationState> UpdateAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {

        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        if (Studio.Id <= 0 || (!disableVerification && !await TcontactBase.ExistsAsync(Studio.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "Le studio n'existe pas.");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, Studio.Id, cancellationToken);
            if (existingId is not null && existingId != Id)
                return new OperationState(false, "Le lien existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TanimeStudio 
            SET 
                IdAnime = $IdAnime, 
                IdStudio = $IdStudio 
            WHERE Id = $Id;
            """;

        

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdStudio", Studio.Id);

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

    #region InsertOrUpdate

    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<int> idStudioArray,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (idStudioArray.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeStudio (IdAnime, IdStudio) VALUES";
        

        for (var i = 0; i < idStudioArray.Count; i++)
        {
            var idContact = idStudioArray.ElementAt(i);
            if (idContact <= 0)
            {
                LogServices.LogDebug($"L'identifiant du studio ne peut pas être égal ou inférieur à 0.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $IdStudio{i})";
            command.Parameters.AddWithValue($"$IdStudio{i}", idContact);

            if (i == idStudioArray.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0, $"{count} enregistrement(s) sur {idStudioArray.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState> AddOrUpdateAsync(TanimeStudio value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (!await TanimeBase.ExistsAsync(value.IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        if (!await TcontactBase.ExistsAsync(value.Studio.Id, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "Le studio n'existe pas.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.Studio.Id, cancellationToken);

        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState(false, "Le nom de l'item existe déjà");

            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
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
    /// Supprime les enregistrements de la table TanimeStudio qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(int idStudio, int idAnime)> actualValues, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeStudio WHERE IdStudio NOT IN (";
        
        var i = 0;
        foreach (var (idStudio, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdStudio{i}" : $", $IdStudio{i}";
            command.Parameters.AddWithValue($"$IdStudio{i}", idStudio);
            i++;
        }
        command.CommandText += ") AND IdAnime NOT IN (";
        i = 0;
        foreach (var (_, idAnime) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdAnime{i}" : $", $IdAnime{i}";
            command.Parameters.AddWithValue($"$IdAnime{i}", (byte)idAnime);
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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeStudio WHERE Id = $Id";

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

    private static async IAsyncEnumerable<TanimeStudio> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TanimeStudio()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdAnime = reader.GetInt32(reader.GetOrdinal("IdAnime")),
                Studio = new TcontactBase(reader.GetInt32(reader.GetOrdinal("SheetId")), reader.GetGuid(reader.GetOrdinal("Guid")))
                {
                    Type = (ContactType)reader.GetInt32(reader.GetOrdinal("Type")),
                    DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                    Presentation = reader.IsDBNull(reader.GetOrdinal("Presentation"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Presentation")),
                    Url = reader.GetString(reader.GetOrdinal("Url")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ThumbnailUrl"))
                }
            };
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeStudio.Id AS BaseId,
            TanimeStudio.IdAnime,
            TanimeStudio.IdStudio,
            
            Tcontact.SheetId,
            Tcontact.Guid,
            Tcontact.Type,
            Tcontact.DisplayName,
            Tcontact.Presentation,
            Tcontact.Url,
            Tcontact.ThumbnailUrl
        
        FROM TanimeStudio
        LEFT JOIN main.Tcontact Tcontact on Tcontact.Id = TanimeStudio.IdStudio
        """;
}