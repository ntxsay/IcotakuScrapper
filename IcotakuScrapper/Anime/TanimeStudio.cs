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

    public TanimeStudio(int idAnime, Tcontact studio)
    {
        IdAnime = idAnime;
        Studio = studio;
    }


    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStudio
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

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
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio WHERE Id = $Id";

        command.Parameters.Clear();

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
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStudio WHERE IdAnime = $IdAnime AND IdStudio = $IdStudio";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeStudio WHERE IdAnime = $IdAnime AND IdStudio = $IdStudio";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
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

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, idContact, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeStudio[]> SelectAsync(int id, AnimeStudioIdSelector selector, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + selector switch
        {
            AnimeStudioIdSelector.Id => "WHERE TanimeStudio.Id = $Id",
            AnimeStudioIdSelector.IdAnime => "WHERE TanimeStudio.IdAnime = $Id",
            AnimeStudioIdSelector.IdStudio => "WHERE TanimeStudio.IdStudio = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
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

    public static async Task<TanimeStudio?> SingleAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeStudio.IdAnime = $IdAnime AND TanimeStudio.IdStudio = $IdStudio";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdStudio", idContact);
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
            return new OperationState<int>(false, "L'anime n'existe pas.", 0);


        if (Studio.Id <= 0 || !await Tcontact.ExistsAsync(Studio.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le studio n'existe pas.", 0);

        if (await ExistsAsync(IdAnime, Studio.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le lien existe déjà.", 0);

        command.CommandText = "INSERT INTO TanimeStudio (IdAnime, IdStudio) VALUES ($IdAnime, $IdStudio);";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdStudio", Studio.Id);

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

    public static async Task<OperationState> InsertAsync(int idAnime, IReadOnlyCollection<int> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");

        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeStudio (IdAnime, IdStudio) VALUES";
        command.Parameters.Clear();

        for (var i = 0; i < values.Count; i++)
        {
            var idContact = values.ElementAt(i);
            if (idContact <= 0)
            {
                LogServices.LogDebug($"L'identifiant  du studio ne peut pas être égal ou inférieur à 0.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $IdStudio{i})";
            command.Parameters.AddWithValue($"$IdStudio{i}", idContact);

            if (i == values.Count - 1)
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

        if (Studio.Id <= 0 || !await TcontactBase.ExistsAsync(Studio.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "Le studio n'existe pas.");

        var existingId = await GetIdOfAsync(IdAnime, Studio.Id, cancellationToken, command);
        if (existingId is not null && existingId != Id)
            return new OperationState(false, "Le lien existe déjà.");

        command.CommandText = "UPDATE TanimeStudio SET IdAnime = $IdAnime, IdStudio = $IdStudio WHERE Id = $Id;";

        command.Parameters.Clear();

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

    #region Delete
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeStudio WHERE Id = $Id";

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