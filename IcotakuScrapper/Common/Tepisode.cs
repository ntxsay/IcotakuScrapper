﻿using System.Diagnostics;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum AnimeEpisodeSortBy : byte
{
    Id,
    IdAnime,
    ReleaseDate,
    EpisodeNumber,
    EpisodeName,
    Day
}

public partial class Tepisode : ITableBase<Tepisode>
{
    public const string ReleaseDateFormat = "yyyy-MM-dd";

    public int Id { get; set; }
    public int IdAnime { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public ushort NoEpisode { get; set; }
    public string? EpisodeName { get; set; }
    public DayOfWeek Day { get; set; }
    public string? Description { get; set; }

    public Tepisode()
    {
    }

    public Tepisode(int id)
    {
        Id = id;
    }

    public Tepisode(int id, int idAnime)
    {
        Id = id;
        IdAnime = idAnime;
    }

    public Tepisode(int idAnime, ushort noEpisode)
    {
        IdAnime = idAnime;
        NoEpisode = noEpisode;
    }

    public Tepisode(int id, int idAnime, DateOnly releaseDate, ushort noEpisode, string episodeName,
        DayOfWeek day, string? description)
    {
        Id = id;
        IdAnime = idAnime;
        ReleaseDate = releaseDate;
        NoEpisode = noEpisode;
        EpisodeName = episodeName;
        Day = day;
        Description = description;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tepisode
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tepisode";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountByIdAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tepisode WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountByIdAnimeAsync(int idAnime, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tepisode WHERE IdAnime = $Id";

        command.Parameters.AddWithValue("$Id", idAnime);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    static Task<int> ITableBase<Tepisode>.CountAsync(int id, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tepisode ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="columnSelect"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.IdAnime,
        ]);

        if (!isColumnSelectValid)
        {
            return 0;
        }

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM Tepisode WHERE Id = $Id",
            IntColumnSelect.IdAnime => "SELECT COUNT(Id) FROM Tepisode WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect,
                "La valeur spécifiée est invalide")
        };

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tepisode
    /// </summary>
    /// <param name="releaseDate"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tepisode WHERE ReleaseDate = $ReleaseDate";

        command.Parameters.AddWithValue("$ReleaseDate", releaseDate.ToString(ReleaseDateFormat));

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tepisode
    /// </summary>
    /// <param name="noEpisode"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, ushort noEpisode, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM Tepisode WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM Tepisode WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    static Task<bool> ITableBase<Tepisode>.ExistsAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(DateOnly releaseDate, CancellationToken? cancellationToken = null)
        => await CountAsync(releaseDate, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null)
        => await CountAsync(idAnime, noEpisode, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<Tepisode[]> SelectAsync(AnimeEpisodeSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + sortBy switch
        {
            AnimeEpisodeSortBy.Id => $"ORDER BY Tepisode.Id {orderBy}",
            AnimeEpisodeSortBy.IdAnime => $"ORDER BY Tepisode.IdAnime {orderBy}",
            AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY Tepisode.ReleaseDate {orderBy}",
            AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY Tepisode.NoEpisode {orderBy}",
            AnimeEpisodeSortBy.EpisodeName => $"ORDER BY Tepisode.EpisodeName {orderBy}",
            AnimeEpisodeSortBy.Day => $"ORDER BY Tepisode.NoDay {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, "La valeur spécifiée est invalide")
        };
        command.AddLimitOffset(limit, offset);


        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<Tepisode[]> SelectAsync(int id, IcotakuSection section, AnimeEpisodeSortBy sortBy,
        OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        if (section != IcotakuSection.Anime && section != IcotakuSection.Drama)
            throw new NotSupportedException(
                $"Seules les section {nameof(IcotakuSection.Anime)} et {nameof(IcotakuSection.Drama)} sont supportées.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        command.CommandText += section switch
        {
            IcotakuSection.Anime => "WHERE IdAnime = $Id",
            IcotakuSection.Drama => "WHERE IdDrama = $Id",
            _ => throw new NotSupportedException(
                $"Seules les section {nameof(IcotakuSection.Anime)} et {nameof(IcotakuSection.Drama)} sont supportées.")
        };

        command.CommandText += Environment.NewLine + sortBy switch
        {
            AnimeEpisodeSortBy.Id => $"ORDER BY Tepisode.Id {orderBy}",
            AnimeEpisodeSortBy.IdAnime => $"ORDER BY Tepisode.IdAnime {orderBy}",
            AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY Tepisode.ReleaseDate {orderBy}",
            AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY Tepisode.NoEpisode {orderBy}",
            AnimeEpisodeSortBy.EpisodeName => $"ORDER BY Tepisode.EpisodeName {orderBy}",
            AnimeEpisodeSortBy.Day => $"ORDER BY Tepisode.NoDay {orderBy}",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy,
                "La valeur spécifiée est invalide")
        };

        command.AddLimitOffset(limit, offset);

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<Tepisode[]> SelectAsync(DateOnly minDate, DateOnly maxDate,
        AnimeEpisodeSortBy sortBy, OrderBy orderBy,
        uint limit = 0, uint offset = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE ReleaseDate BETWEEN $MinDate AND $MaxDate" + Environment.NewLine + sortBy switch
                              {
                                  AnimeEpisodeSortBy.Id => $"ORDER BY Tepisode.Id {orderBy}",
                                  AnimeEpisodeSortBy.IdAnime => $"ORDER BY Tepisode.IdAnime {orderBy}",
                                  AnimeEpisodeSortBy.ReleaseDate => $"ORDER BY Tepisode.ReleaseDate {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeNumber => $"ORDER BY Tepisode.NoEpisode {orderBy}",
                                  AnimeEpisodeSortBy.EpisodeName => $"ORDER BY Tepisode.EpisodeName {orderBy}",
                                  AnimeEpisodeSortBy.Day => $"ORDER BY Tepisode.NoDay {orderBy}",
                                  _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy,
                                      "La valeur spécifiée est invalide")
                              };

        command.AddLimitOffset(limit, offset);

        command.Parameters.AddWithValue("$MinDate", minDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$MaxDate", maxDate.ToString(ReleaseDateFormat));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<Tepisode?> SingleAsync(int id,
        CancellationToken? cancellationToken = null)
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

    public static async Task<Tepisode?> SingleAsync(int idAnime, ushort noEpisode,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE IdAnime = $IdAnime AND NoEpisode = $NoEpisode";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$NoEpisode", noEpisode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false,
        CancellationToken? cancellationToken = null)
    {
        if (NoEpisode <= 0)
            return new OperationState<int>(false, "Le numéro de l'épisode ne peut pas être inférieur ou égal à 0");

        if (IdAnime <= 0 || (!disableVerification &&
                             !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "L'identifiant de l'anime est invalide");

        if (!disableVerification && await ExistsAsync(IdAnime, NoEpisode, cancellationToken))
            return new OperationState<int>(false, "L'épisode existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Tepisode
                (IdAnime, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description)
            VALUES
                ($IdAnime, $ReleaseDate, $NoEpisode, $EpisodeName, $NoDay, $Description)
            """;

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", NoEpisode);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result <= 0)
                return new OperationState<int>(false, "L'insertion n'a pas été effectuée");

            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "L'insertion a été effectuée", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification = false,
        CancellationToken? cancellationToken = null)
    {
        if (NoEpisode <= 0)
            return new OperationState(false, "Le numéro de l'épisode ne peut pas être inférieur ou égal à 0");

        if (Id <= 0 || (!disableVerification && !await ExistsAsync(Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "L'identifiant de l'épisode est invalide");

        if (IdAnime <= 0 || (!disableVerification &&
                             !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "L'identifiant de l'anime est invalide");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, NoEpisode, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "L'épisode existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Tepisode SET
                IdAnime = $IdAnime,
                ReleaseDate = $ReleaseDate,
                NoEpisode = $NoEpisode,
                EpisodeName = $EpisodeName,
                NoDay = $NoDay,
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate.ToString(ReleaseDateFormat));
        command.Parameters.AddWithValue("$NoEpisode", NoEpisode);
        command.Parameters.AddWithValue("$EpisodeName", EpisodeName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$NoDay", (byte)Day);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Id", Id);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result <= 0
                ? new OperationState(false, "La mise à jour n'a pas été effectuée")
                : new OperationState(true, "La mise à jour a été effectuée");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region AddOrUpdate

    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<Tepisode> values,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "Aucun épisode n'a été trouvé.");

        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText +=
            " INTO Tepisode (IdAnime, ReleaseDate, NoEpisode, EpisodeName, NoDay, Description) VALUES";


        for (var i = 0; i < values.Count; i++)
        {
            var episode = values.ElementAt(i);
            if (episode.NoEpisode <= 0)
            {
                LogServices.LogDebug($"Un des épisode sélectionnés n'est pas valide ({episode}).");
                continue;
            }

            command.CommandText += Environment.NewLine +
                                   $"($IdAnime, $ReleaseDate{i}, $NoEpisode{i}, $EpisodeName{i}, $NoDay{i}, $Description{i})";

            command.Parameters.AddWithValue($"$ReleaseDate{i}", episode.ReleaseDate.ToString(ReleaseDateFormat));
            command.Parameters.AddWithValue($"$NoEpisode{i}", episode.NoEpisode);
            command.Parameters.AddWithValue($"$EpisodeName{i}", episode.EpisodeName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$NoDay{i}", (byte)episode.Day);
            command.Parameters.AddWithValue($"$Description{i}", episode.Description ?? (object)DBNull.Value);

            if (i == values.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun épisode n'a été trouvé.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0,
                $"{count} enregistrement(s) sur {values.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public static async Task<Tepisode?> SingleOrCreateAsync(Tepisode value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.IdAnime, value.NoEpisode, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.IdAnime, value.NoEpisode, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(Tepisode value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (!await TanimeBase.ExistsAsync(value.IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState<int>(false, "L'anime n'existe pas.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.NoEpisode, cancellationToken);

        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState<int>(false, "Le nom de l'item existe déjà");

            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;
            return (await value.UpdateAsync(true, cancellationToken)).ToGenericState(value.Id);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAsync(true, cancellationToken);
        return addResult;
    }

    #endregion

    #region Delete

    /// <summary>
    /// Supprime les enregistrements de la table Tepisode qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(ushort noEpisode, int idAnime)> actualValues,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM Tepisode WHERE NoEpisode NOT IN (";

        var i = 0;
        foreach (var (noEpisode, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$NoEpisode{i}" : $", $NoEpisode{i}";
            command.Parameters.AddWithValue($"$NoEpisode{i}", noEpisode);
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
        command.CommandText = "DELETE FROM Tepisode WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} ligne(s) ont été supprimée(s)");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    #endregion

    private static async IAsyncEnumerable<Tepisode> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new Tepisode()
            {
                Id = reader.GetInt32(reader.GetOrdinal("BaseId")),
                IdAnime = reader.GetInt32(reader.GetOrdinal("IdAnime")),
                ReleaseDate = DateHelpers.GetDateOnly(reader.GetString(reader.GetOrdinal("ReleaseDate")),
                    ReleaseDateFormat),
                NoEpisode = (ushort)reader.GetInt16(reader.GetOrdinal("NoEpisode")),
                EpisodeName = reader.IsDBNull(reader.GetOrdinal("EpisodeName"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("EpisodeName")),
                Day = (DayOfWeek)reader.GetByte(reader.GetOrdinal("NoDay")),
                Description = reader.IsDBNull(reader.GetOrdinal("BaseDescription"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("BaseDescription")),
            };
        }
    }


    private const string SqlSelectScript =
        """
        SELECT
            Tepisode.Id AS BaseId,
            Tepisode.IdAnime,
            Tepisode.ReleaseDate,
            Tepisode.NoEpisode,
            Tepisode.EpisodeName,
            Tepisode.NoDay,
            Tepisode.Description AS BaseDescription

        FROM
            Tepisode
        """;
}