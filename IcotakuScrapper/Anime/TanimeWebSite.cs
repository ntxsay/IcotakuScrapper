using System.Diagnostics;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

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
    
    public TanimeWebSite(int idAnime, string title, string? description)
    {
        IdAnime = idAnime;
        Url = title;
        Description = description;
    }
    
    public TanimeWebSite(int id, int idAnime, string title, string? description)
    {
        Id = id;
        IdAnime = idAnime;
        Url = title;
        Description = description;
    }
    
        #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeWebSite";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, SelectCountIdIdAnimeKind countIdAnimeKind, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = countIdAnimeKind switch
        {
            SelectCountIdIdAnimeKind.Id => "SELECT COUNT(Id) FROM TanimeWebSite WHERE Id = $Id",
            SelectCountIdIdAnimeKind.IdAnime => "SELECT COUNT(Id) FROM TanimeWebSite WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(countIdAnimeKind), countIdAnimeKind, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeWebSite ayant le nom spécifié
    /// </summary>
    /// <param Url="title"></param>
    /// <param Url="cancellationToken"></param>
    /// <param Url="cmd"></param>
    /// <param Url="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeWebSite WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", title);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int?> GetIdOfAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeWebSite WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", title);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    #endregion
    
    #region Exists
    
    public static async Task<bool> ExistsAsync(int id, SelectCountIdIdAnimeKind countIdAnimeKind, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, countIdAnimeKind, cancellationToken, cmd) > 0;
    
    public static async Task<bool> ExistsAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, title, cancellationToken, cmd) > 0;
    
    #endregion

    #region Select

    public static async Task<TanimeWebSite[]> SelectAsync(int idAnime, OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.CommandText += Environment.NewLine;
        command.CommandText += $" ORDER BY Url {orderBy}";

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeWebSite>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeWebSite?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<TanimeWebSite?> SingleAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime AND Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Url", title);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion
    
    #region Insert
    
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le titre est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, SheetIntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "L'anime n'existe pas.");
        
        if (await ExistsAsync(IdAnime, Url, cancellationToken, command))
            return new OperationState<int>(false, "Le titre existe déjà.");
        
        command.CommandText = 
            """
            INSERT INTO TanimeWebSite 
                (IdAnime, Url, Description) 
            VALUES 
                ($IdAnime, $Url, $Description)
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
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
    
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id est invalide.");
        
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le titre est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, SheetIntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");
        
        var existingId = await GetIdOfAsync(IdAnime, Url, cancellationToken, command);
        if (existingId != null && existingId != Id)
            return new OperationState(false, "Le titre existe déjà.");
        
        command.CommandText = 
            """
            UPDATE TanimeWebSite 
            SET 
                IdAnime = $IdAnime, 
                Url = $Url, 
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
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
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'id est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeWebSite WHERE Id = $Id";

        command.Parameters.Clear();

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