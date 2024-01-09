using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Common;

public enum SeasonSortBy
{
    /// <summary>
    /// Tri par défaut (Id)
    /// </summary>
    Default = 0,
    /// <summary>
    /// Tri par numéro de saison
    /// </summary>
    SeasonNumber = 2,
    /// <summary>
    /// Tri par nom
    /// </summary>
    Name = 3
}

public partial class Tseason
{
    public int Id { get; protected set; }
    public string DisplayName { get; set; } = string.Empty;
    public uint SeasonNumber { get; set; }

    public Tseason()
    {
    }

    public Tseason(int id)
    {
        Id = id;
    }

    public Tseason(int id, string displayName, uint seasonNumber)
    {
        Id = id;
        DisplayName = displayName;
        SeasonNumber = seasonNumber;
    }

    public void Copy(Tseason season)
    {
        Id = season.Id;
        DisplayName = season.DisplayName;
        SeasonNumber = season.SeasonNumber;
    }

    public WeatherSeason ToWeatherSeason()
        => DateHelpers.GetWeatherSeason(SeasonNumber);

    public override string ToString()
        => DateHelpers.GetSeasonLiteral(SeasonNumber) ?? $"{DisplayName} ({SeasonNumber})";

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason";




        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason WHERE Id = $Id";



        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tseason ayant le nom spécifié
    /// </summary>
    /// <param name="seasonNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(uint seasonNumber, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tseason WHERE Tseason.SeasonNumber = $SeasonNumber";



        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(uint seasonNumber, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Tseason WHERE Tseason.SeasonNumber = $SeasonNumber";



        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion

    #region Exists
    public async Task<bool> ExistsAsync(CancellationToken? cancellationToken = null)
        => await ExistsAsync(SeasonNumber, cancellationToken);

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
     => await CountAsync(id, cancellationToken) > 0;


    public static async Task<bool> ExistsAsync(uint seasonNumber, CancellationToken? cancellationToken = null)
        => await CountAsync(seasonNumber, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<Tseason[]> SelectAsync(SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine;

        command.CommandText += sortBy switch
        {
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };

        command.AddLimitOffset(limit, skip);



        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }
    public static async Task<Tseason[]> SelectAsync(uint seasonNumber, SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE SeasonNumber = $SeasonNumber";

        command.CommandText += sortBy switch
        {
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };

        command.AddLimitOffset(limit, skip);



        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }
    public static async Task<TseasonStruct[]> SelectStructAsync(SeasonSortBy sortBy = SeasonSortBy.Default, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine;

        command.CommandText += sortBy switch
        {
            SeasonSortBy.SeasonNumber => $"ORDER BY SeasonNumber {orderBy}",
            SeasonSortBy.Name => $"ORDER BY DisplayName {orderBy}",
            SeasonSortBy.Default => $"ORDER BY Id {orderBy}",
            _ => $"ORDER BY Id {orderBy}"
        };

        command.AddLimitOffset(limit, skip);



        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetStructRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }



    #endregion

    #region Single

    public static async Task<Tseason?> SingleAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Id = $Id";



        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }

    public static async Task<Tseason?> SingleAsync(uint seasonNumber, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE SeasonNumber = $SeasonNumber";



        command.Parameters.AddWithValue("$SeasonNumber", seasonNumber);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }

    public static async Task<Tseason?> SingleAsync(string displayName, CancellationToken? cancellationToken = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE DisplayName = $DisplayName COLLATE NOCASE";



        command.Parameters.AddWithValue("$DisplayName", displayName.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        if (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
            return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        return null;
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableExistenceVerification = false, CancellationToken? cancellationToken = null)
    {
        if (!DateHelpers.IsSeasonValidated(SeasonNumber))
            return new OperationState<int>(false, "Le numéro de la saison est invalide");
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            DisplayName = DateHelpers.GetSeasonLiteral(SeasonNumber) ?? string.Empty;

        await using var command = Main.Connection.CreateCommand();

        if (!disableExistenceVerification && await ExistsAsync(SeasonNumber, cancellationToken))
            return new OperationState<int>(false, "Une saison avec le même nom existe déjà");

        command.CommandText =
            """
            INSERT INTO Tseason
                (DisplayName, SeasonNumber)
            VALUES
                ($DisplayName, $SeasonNumber)
            """;



        command.Parameters.AddWithValue("$DisplayName", DisplayName.Trim());
        command.Parameters.AddWithValue("$SeasonNumber", SeasonNumber);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");

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

    public async Task<OperationState> UpdateAsync(bool disableExistenceVerification = false, CancellationToken? cancellationToken = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");

        if (!DateHelpers.IsSeasonValidated(SeasonNumber))
            return new OperationState(false, "Le numéro de la saison est invalide");
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            DisplayName = DateHelpers.GetSeasonLiteral(SeasonNumber) ?? string.Empty;

        await using var command = Main.Connection.CreateCommand();

        if (!disableExistenceVerification)
        {
            var existingId = await GetIdOfAsync(SeasonNumber, cancellationToken);
            if (existingId is not null && existingId != Id)
                return new OperationState(false, "Une saison avec le même nom existe déjà");
        }

        command.CommandText =
            """
            UPDATE Tseason
            SET
                DisplayName = $DisplayName,
                SeasonNumber = $SeasonNumber
            WHERE Id = $Id
            """;



        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$DisplayName", DisplayName.Trim());
        command.Parameters.AddWithValue("$SeasonNumber", SeasonNumber);

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

    #region Single or Create or Update

    public static async Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<Tseason> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO Tseason (DisplayName, SeasonNumber) VALUES";



        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);

            if (value.DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"L'item {i} n'a pas de nom, il sera ignoré.");
                continue;
            }

            if (!DateHelpers.IsSeasonValidated(value.SeasonNumber))
            {
                LogServices.LogDebug($"L'item {i} n'a pas de numéro de saison valide, il sera ignoré.");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($DisplayName{i}, $SeasonNumber{i})";

            command.Parameters.AddWithValue($"$DisplayName{i}", value.DisplayName.Trim());
            command.Parameters.AddWithValue($"$SeasonNumber{i}", value.SeasonNumber);

            if (i < values.Count - 1)
                command.CommandText += ",";
            else
                command.CommandText += ";";
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
    /// Retourne l'enregistrement de la table Tseason ayant l'identifiant spécifié ou l'insert si l'enregistrement n'existe pas
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Tseason?> SingleOrCreateAsync(Tseason value, bool reloadIfExist = false, CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.SeasonNumber, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.SeasonNumber, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }

    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState> AddOrUpdateAsync(Tseason value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (!DateHelpers.IsSeasonValidated(value.SeasonNumber))
            return new OperationState(false, "Le numéro de la saison est invalide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.SeasonNumber, cancellationToken);

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

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
     => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");

        await using var command = Main.Connection.CreateCommand();

        command.CommandText =
            """
            UPDATE Tanime SET IdSeason = NULL WHERE IdSeason = $Id;
            DELETE FROM TanimeSeasonalPlanning WHERE IdSeason = $Id;
            DELETE FROM Tseason  WHERE Id = $Id;
            """;



        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} ligne(s) supprimée(s)");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    public static async Task<OperationState> DeleteAsync(uint seasonNumber, CancellationToken? cancellationToken = null)
    {
        if (seasonNumber <= 0)
            return new OperationState(false, "L'identifiant de la saison est invalide");

        await using var command = Main.Connection.CreateCommand();
        var id = await GetIdOfAsync(seasonNumber, cancellationToken);
        if (id is null)
            return new OperationState(false, "La saison n'existe pas");

        return await DeleteAsync(id.Value, cancellationToken);
    }

    #endregion

    internal static Tseason GetRecord(SqliteDataReader reader, int idIndex, int seasonNumberIndex, int displayNameIndex)
    {
        return new Tseason()
        {
            Id = reader.GetInt32(idIndex),
            DisplayName = reader.GetString(displayNameIndex),
            SeasonNumber = (uint)reader.GetInt32(seasonNumberIndex)
        };
    }

    internal static TseasonStruct GetStructRecord(SqliteDataReader reader, int idIndex, int seasonNumberIndex, int displayNameIndex)
    {
        return new TseasonStruct()
        {
            Id = reader.GetInt32(idIndex),
            DisplayName = reader.GetString(displayNameIndex),
            SeasonNumber = (uint)reader.GetInt32(seasonNumberIndex)
        };
    }


    private static async IAsyncEnumerable<Tseason> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        }
    }

    private static async IAsyncEnumerable<TseasonStruct> GetStructRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetStructRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                seasonNumberIndex: reader.GetOrdinal("SeasonNumber"),
                displayNameIndex: reader.GetOrdinal("DisplayName"));
        }
    }


    private const string IcotakuSqlSelectScript =
        """
        SELECT 
            Id, 
            DisplayName, 
            SeasonNumber 
        FROM Tseason
        """;
}

public readonly struct TseasonStruct
{
    public TseasonStruct()
    {
    }

    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public uint SeasonNumber { get; init; }

    public WeatherSeason ToWeatherSeason()
        => DateHelpers.GetWeatherSeason(SeasonNumber);
}