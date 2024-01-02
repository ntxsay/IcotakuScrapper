using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Anime;





public class TanimeAlternativeTitle
{
    public int Id { get; set; }
    public int IdAnime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public override string ToString()
    {
        return $"{Id} - {Title}";
    }

    public TanimeAlternativeTitle()
    {
    }

    public TanimeAlternativeTitle(int id)
    {
        Id = id;
    }

    public TanimeAlternativeTitle(int id, int idAnime)
    {
        Id = id;
        IdAnime = idAnime;
    }

    public TanimeAlternativeTitle(int idAnime, string title, string? description)
    {
        IdAnime = idAnime;
        Title = title;
        Description = description;
    }

    public TanimeAlternativeTitle(int id, int idAnime, string title, string? description)
    {
        Id = id;
        IdAnime = idAnime;
        Title = title;
        Description = description;
    }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeAlternativeTitle";

        
            

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
            SelectCountIdIdAnimeKind.Id => "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE Id = $Id",
            SelectCountIdIdAnimeKind.IdAnime => "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(countIdAnimeKind), countIdAnimeKind, null)
        };

        

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeAlternativeTitle ayant le nom spécifié
    /// </summary>
    /// <param name="title"></param>
    /// <param name="cancellationToken"></param>

    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, string title, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE IdAnime = $IdAnime AND Title = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, string title, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeAlternativeTitle WHERE IdAnime = $IdAnime AND Title = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, SelectCountIdIdAnimeKind countIdAnimeKind, CancellationToken? cancellationToken = null)
        => await CountAsync(id, countIdAnimeKind, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, string title, CancellationToken? cancellationToken = null)
        => await CountAsync(idAnime, title, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TanimeAlternativeTitle[]> SelectAsync(int idAnime, OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime";

        

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.CommandText += Environment.NewLine;
        command.CommandText += $" ORDER BY Title {orderBy}";

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeAlternativeTitle?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE Id = $Id";

        

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeAlternativeTitle?> SingleAsync(int idAnime, string title, CancellationToken? cancellationToken = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime AND Title = $Name COLLATE NOCASE";

        

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Title.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le titre est invalide.");

        if (IdAnime <= 0 || (!disableVerification && !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "L'anime n'existe pas.");

        if (!disableVerification && await ExistsAsync(IdAnime, Title, cancellationToken))
            return new OperationState<int>(false, "Le titre existe déjà.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TanimeAlternativeTitle 
                (IdAnime, Title, Description) 
            VALUES 
                ($IdAnime, $Name, $Description)
            """;

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$Name", Title);
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

    public async Task<OperationState> UpdateAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id est invalide.");

        if (Title.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le titre est invalide.");

        if (IdAnime <= 0 || (!disableVerification && !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "L'anime n'existe pas.");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, Title, cancellationToken);
            if (existingId != null && existingId != Id)
                return new OperationState(false, "Le titre existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TanimeAlternativeTitle 
            SET 
                IdAnime = $IdAnime, 
                Title = $Name, 
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$Name", Title);
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

    #region AddOrUpdateOrSingle

    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<TanimeAlternativeTitle> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Il n'existe aucune valeur à insérer.");

        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeAlternativeTitle (IdAnime, Title, Description) VALUES";
        

        for (var i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt(i);
            if (value.Title.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom du titre ne doit pas être vide ou ne contenir que des espaces blancs.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $Title{i}, $Description{i})";
            command.Parameters.AddWithValue($"$Title{i}", value.Title);
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

    public static async Task<OperationState> AddOrUpdateAsync(TanimeAlternativeTitle value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Title.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.Title, cancellationToken);

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
    /// Supprime les enregistrements de la table TanimeAlternativeTitle qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(string title, int idAnime)> actualValues, CancellationToken? cancellationToken = null)
    {

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeAlternativeTitle WHERE Title NOT IN (";

        var i = 0;
        foreach (var (title, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$Title{i}" : $", $Title{i}";
            command.Parameters.AddWithValue($"$Title{i}", title);
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
        command.CommandText = "DELETE FROM TanimeAlternativeTitle WHERE Id = $Id";

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

    internal static TanimeAlternativeTitle GetRecord(SqliteDataReader reader, int idIndex, int idAnimeIndex, int titleIndex, int descriptionIndex)
    {
        return new TanimeAlternativeTitle()
        {
            Id = reader.GetInt32(idIndex),
            IdAnime = reader.GetInt32(idAnimeIndex),
            Title = reader.GetString(titleIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex)
        };
    }

    private static async IAsyncEnumerable<TanimeAlternativeTitle> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                idAnimeIndex: reader.GetOrdinal("IdAnime"),
                titleIndex: reader.GetOrdinal("Title"),
                descriptionIndex: reader.GetOrdinal("Description"));
        }
    }


    private const string SqlSelectScript =
        """
        SELECT
            Id,
            IdAnime,
            Title,
            Description
        FROM TanimeAlternativeTitle
        """;
}