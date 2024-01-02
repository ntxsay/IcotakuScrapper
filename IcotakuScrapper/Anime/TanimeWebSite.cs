using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Anime;

public class TanimeWebSite
{
    public int Id { get; set; }
    public int IdAnime { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }

    public override string ToString()
    {
        return $"{Url} ({Description})";
    }

    public TanimeWebSite()
    {
    }

    public TanimeWebSite(int id)
    {
        Id = id;
    }

    public TanimeWebSite(int id, int idAnime)
    {
        Id = id;
        IdAnime = idAnime;
    }

    public TanimeWebSite(int idAnime, string url, string? description)
    {
        IdAnime = idAnime;
        Url = url;
        Description = description;
    }

    public TanimeWebSite(int id, int idAnime, string url, string? description)
    {
        Id = id;
        IdAnime = idAnime;
        Url = url;
        Description = description;
    }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeWebSite";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, SelectCountIdIdAnimeKind countIdAnimeKind, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = countIdAnimeKind switch
        {
            SelectCountIdIdAnimeKind.Id => "SELECT COUNT(Id) FROM TanimeWebSite WHERE Id = $Id",
            SelectCountIdIdAnimeKind.IdAnime => "SELECT COUNT(Id) FROM TanimeWebSite WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(countIdAnimeKind), countIdAnimeKind, null)
        };

        
        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeWebSite ayant le nom spécifié
    /// </summary>
    /// <param Url="url"></param>
    /// <param Url="cancellationToken"></param>
    /// <param Url="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, string url, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeWebSite WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", url);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, string url, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeWebSite WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", url);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, SelectCountIdIdAnimeKind countIdAnimeKind, CancellationToken? cancellationToken = null)
        => await CountAsync(id, countIdAnimeKind, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, string url, CancellationToken? cancellationToken = null)
        => await CountAsync(idAnime, url, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TanimeWebSite[]> SelectAsync(int idAnime, OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime";

        

        command.Parameters.AddWithValue("$IdAnime", idAnime);
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

    public static async Task<TanimeWebSite?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeWebSite?> SingleAsync(int idAnime, string url, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", url);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le site web est invalide.");

        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState<int>(false, "L'anime n'existe pas.");

        if (!disableVerification && await ExistsAsync(IdAnime, Url, cancellationToken))
            return new OperationState<int>(false, "Le site web existe déjà.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TanimeWebSite 
                (IdAnime, Url, Description) 
            VALUES 
                ($IdAnime, $Url, $Description)
            """;

        

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result <= 0)
                return new OperationState<int>(false, "Aucun enregistrement n'a été ajouté.");
            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "Le site web alternatif a été ajouté.", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id est invalide.");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le site web est invalide.");

        if (IdAnime <= 0 || (!disableVerification && !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "L'anime n'existe pas.");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, Url, cancellationToken);
            if (existingId != null && existingId != Id)
                return new OperationState(false, "Le site web existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TanimeWebSite 
            SET 
                IdAnime = $IdAnime, 
                Url = $Url, 
                Description = $Description
            WHERE Id = $Id
            """;

      
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "Aucun enregistrement n'a été modifié.")
                : new OperationState(true, "Le site web alternatif a été modifié.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    #region InsertOrReplace

    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<TanimeWebSite> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Il n'existe aucune valeur à insérer.");

        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeWebSite (IdAnime, Url, Description) VALUES";
        

        for (var i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt(i);
            if (value.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(value.Url, UriKind.Absolute, out var uriResult) ||
                !uriResult.IsAbsoluteUri)
            {
                LogServices.LogDebug($"L'url de l'item {i} est invalide.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $Url{i}, $Description{i})";
            command.Parameters.AddWithValue($"$Url{i}", uriResult.ToString());
            command.Parameters.AddWithValue($"$Description{i}", value.Description ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Il n'existe aucune valeur à insérer.");

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

    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState> AddOrUpdateAsync(TanimeWebSite value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uriResult) ||
            !uriResult.IsAbsoluteUri)
            return new OperationState(false, "Le site web est invalide.");

        if (!await TanimeBase.ExistsAsync(value.IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.Url, cancellationToken);

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
    /// Supprime les enregistrements de la table TanimeWebSite qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(string url, int idAnime)> actualValues, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeWebSite WHERE Url NOT IN (";
        
        var i = 0;
        foreach (var (url, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$Url{i}" : $", $Url{i}";
            command.Parameters.AddWithValue($"$Url{i}", url);
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
        if (id <= 0)
            return new OperationState(false, "L'id est invalide.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeWebSite WHERE Id = $Id";

        

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "Une erreur inconnue est survenue.")
                : new OperationState(true, "Le site web alternatif a été supprimé.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur inconnue est survenue.");
        }
    }

    #endregion

    internal static TanimeWebSite GetRecord(SqliteDataReader reader, int idIndex, int idAnimeIndex, int urlIndex, int descriptionIndex)
    {
        return new TanimeWebSite()
        {
            Id = reader.GetInt32(idIndex),
            IdAnime = reader.GetInt32(idAnimeIndex),
            Url = reader.GetString(urlIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }

    private static async IAsyncEnumerable<TanimeWebSite> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                idAnimeIndex: reader.GetOrdinal("IdAnime"),
                urlIndex: reader.GetOrdinal("Url"),
                descriptionIndex: reader.GetOrdinal("Description"));
        }
    }


    private const string SqlSelectScript =
        """
        SELECT
            Id,
            IdAnime,
            Url,
            Description
        FROM TanimeWebSite
        """;
}