using IcotakuScrapper.Common;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public class TanimeStaff
{
    public int Id { get; protected set; }
    public int IdAnime { get; set; }

    public ToeuvreRole Role { get; set; } = new();
    public TcontactBase Person { get; set; } = new();

    public TanimeStaff()
    {
    }

    public TanimeStaff(int id)
    {
        Id = id;
    }

    public TanimeStaff(int idAnime, Tcontact person)
    {
        IdAnime = idAnime;
        Person = person;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStaff
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStaff";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStaff ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeStaff WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeStaff ayant le nom spécifié
    /// </summary>
    /// <param name="idContact"></param>
    /// <param name="idLicenseType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, int idLicenseType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM TanimeStaff WHERE IdAnime = $IdAnime AND IdIndividu = $IdIndividu AND IdRole = $IdRole";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdIndividu", idContact);
        command.Parameters.AddWithValue("$IdRole", idLicenseType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, int idRole,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT Id FROM TanimeStaff WHERE IdAnime = $IdAnime AND IdIndividu = $IdIndividu AND IdRole = $IdRole";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdIndividu", idContact);
        command.Parameters.AddWithValue("$IdRole", idRole);
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

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, int idLicenseType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, idContact, idLicenseType, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeStaff[]> SelectAsync(int id, IntColumnSelect columnSelect,
        OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.IdAnime,
            IntColumnSelect.IdContact,
            IntColumnSelect.IdRole,
        ]);

        if (!isColumnSelectValid)
        {
            return [];
        }

        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE TanimeStaff.Id = $Id",
            IntColumnSelect.IdAnime => "WHERE TanimeStaff.IdAnime = $Id",
            IntColumnSelect.IdContact => "WHERE TanimeStaff.IdIndividu = $Id",
            IntColumnSelect.IdRole => "WHERE TanimeStaff.IdRole = $Id",
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

    public static async Task<TanimeStaff?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE TanimeStaff.Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TanimeStaff?> SingleAsync(int idAnime, int idContact,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE TanimeStaff.IdAnime = $IdAnime AND TanimeStaff.IdIndividu = $IdIndividu";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdIndividu", idContact);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableExistenceVerification = false,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || (!disableExistenceVerification &&
                             !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command)))
            return new OperationState<int>(false, "L'anime n'existe pas.");

        if (Person.Id <= 0 || (!disableExistenceVerification &&
                               !await TcontactBase.ExistsAsync(Person.Id, IntColumnSelect.Id, cancellationToken,
                                   command)))
            return new OperationState<int>(false, "La personne n'existe pas.");

        if (Role.Id <= 0 || (!disableExistenceVerification &&
                             !await ToeuvreRole.ExistsAsync(Role.Id, cancellationToken, command)))
            return new OperationState<int>(false, "Le type de rôle n'existe pas.");

        if (!disableExistenceVerification && await ExistsAsync(IdAnime, Person.Id, Role.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le lien existe déjà.");

        command.CommandText =
            "INSERT INTO TanimeStaff (IdAnime, IdIndividu, IdRole) VALUES ($IdAnime, $IdIndividu, $IdRole);";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdIndividu", Person.Id);
        command.Parameters.AddWithValue("$IdRole", Role.Id);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Aucune insertion n'a été effectuée");

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

    public async Task<OperationState> UpdateAsync(bool disableExistenceVerification = false,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        if (IdAnime <= 0 || (!disableExistenceVerification &&
                             !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command)))
            return new OperationState(false, "L'anime n'existe pas.");

        if (Person.Id <= 0 || (!disableExistenceVerification &&
                               !await TcontactBase.ExistsAsync(Person.Id, IntColumnSelect.Id, cancellationToken,
                                   command)))
            return new OperationState(false, "La personne n'existe pas.");

        if (Role.Id <= 0 || (!disableExistenceVerification &&
                             !await ToeuvreRole.ExistsAsync(Role.Id, cancellationToken, command)))
            return new OperationState(false, "Le type de rôle n'existe pas.");

        if (!disableExistenceVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, Person.Id, Role.Id, cancellationToken, command);
            if (existingId is not null && existingId != Id)
                return new OperationState(false, "Le lien existe déjà.");
        }

        command.CommandText =
            """
            UPDATE TanimeStaff SET
                IdAnime = $IdAnime,
                IdIndividu = $IdIndividu,
                IdRole = $IdRole
            WHERE Id = $Id;
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdIndividu", Person.Id);
        command.Parameters.AddWithValue("$IdRole", Role.Id);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState(false, "Aucune mise à jour n'a été effectuée");

            return new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region Add or update

    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<TanimeStaff> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun staff n'a été trouvé.");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");

        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeStaff (IdAnime, IdIndividu, IdRole) VALUES";
        command.Parameters.Clear();

        for (var i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt(i);
            if (value.Person.Id <= 0)
            {
                LogServices.LogDebug($"L'identifiant  de la personne ne peut pas être égal ou inférieur à 0.");
                continue;
            }

            if (value.Role.Id <= 0)
            {
                LogServices.LogDebug($"L'identifiant  du type de rôle ne peut pas être égal ou inférieur à 0.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $IdIndividu{i}, $IdRole{i})";
            command.Parameters.AddWithValue($"$IdIndividu{i}", value.Person.Id);
            command.Parameters.AddWithValue($"$IdRole{i}", value.Role.Id);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun staff n'a été trouvé.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0,
                $"{count} enregistrement(s) sur {values.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await AddOrUpdateAsync(this, cancellationToken, cmd);

    public static async Task<OperationState> AddOrUpdateAsync(TanimeStaff value,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (!await TanimeBase.ExistsAsync(value.IdAnime, IntColumnSelect.Id, cancellationToken, cmd))
            return new OperationState(false, "L'anime n'existe pas.");

        if (!await TcontactBase.ExistsAsync(value.Person.Id, IntColumnSelect.Id, cancellationToken, cmd))
            return new OperationState(false, "La personne n'existe pas.");

        if (!await ToeuvreRole.ExistsAsync(value.Role.Id, cancellationToken, cmd))
            return new OperationState(false, "Le type de rôle n'existe pas.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.Person.Id, value.Role.Id, cancellationToken, cmd);

        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState(false, "Le nom de l'item existe déjà");

            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;
            return await value.UpdateAsync(true, cancellationToken, cmd);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAsync(true, cancellationToken, cmd);
        if (addResult.IsSuccess)
            value.Id = addResult.Data;

        return addResult.ToBaseState();
    }

    #endregion

    #region Delete

    public static async Task<OperationState> DeleteUnusedAsync(
        HashSet<(int idContact, int idRole, int idAnime)> actualValues, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeStaff WHERE IdIndividu NOT IN (";
        command.Parameters.Clear();
        var i = 0;
        foreach (var (idContact, _, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdIndividu{i}" : $", $IdIndividu{i}";
            command.Parameters.AddWithValue($"$IdIndividu{i}", idContact);
            i++;
        }

        command.CommandText += ") AND IdRole NOT IN (";
        i = 0;
        foreach (var (_, idLicenseType, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdRole{i}" : $", $IdRole{i}";
            command.Parameters.AddWithValue($"$IdRole{i}", idLicenseType);
            i++;
        }

        command.CommandText += ") AND IdAnime NOT IN (";
        i = 0;
        foreach (var (_, _, idAnime) in actualValues)
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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeStaff WHERE Id = $Id";

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

    private static async IAsyncEnumerable<TanimeStaff> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var record = new TanimeStaff()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdAnime = reader.GetInt32(reader.GetOrdinal("IdAnime")),
                Role = new ToeuvreRole(reader.GetInt32(reader.GetOrdinal("IdRole")))
                {
                    Name = reader.GetString(reader.GetOrdinal("RoleName")),
                    Description = reader.IsDBNull(reader.GetOrdinal("RoleDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("RoleDescription")),
                    Type = (RoleType)reader.GetByte(reader.GetOrdinal("RoleType"))
                }
            };

            var idContact = reader.GetInt32(reader.GetOrdinal("IdIndividu"));
            var contact = await TcontactBase.SingleAsync(idContact, IntColumnSelect.Id, cancellationToken);
            if (contact == null)
                continue;

            record.Person = contact;

            yield return record;
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeStaff.Id AS BaseId,
            TanimeStaff.IdAnime,
            TanimeStaff.IdIndividu,
            TanimeStaff.IdRole,
            
            ToeuvreRole.Name AS RoleName,
            ToeuvreRole.Description AS RoleDescription,
            ToeuvreRole.Type AS RoleType
        FROM TanimeStaff
            LEFT JOIN main.ToeuvreRole ON TanimeStaff.IdRole = ToeuvreRole.Id
        """;
}