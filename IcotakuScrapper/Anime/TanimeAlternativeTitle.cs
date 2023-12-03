using System.Diagnostics;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

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

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeAlternativeTitle";

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
            SelectCountIdIdAnimeKind.Id => "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE Id = $Id",
            SelectCountIdIdAnimeKind.IdAnime => "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE IdAnime = $Id",
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
    /// Compte le nombre d'entrées dans la table TanimeAlternativeTitle ayant le nom spécifié
    /// </summary>
    /// <param name="title"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeAlternativeTitle WHERE IdAnime = $IdAnime AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);
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
        command.CommandText = "SELECT Id FROM TanimeAlternativeTitle WHERE IdAnime = $IdAnime AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);
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

    public static async Task<TanimeAlternativeTitle[]> SelectAsync(int idAnime, OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.CommandText += Environment.NewLine;
        command.CommandText += $" ORDER BY Name {orderBy}";

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeAlternativeTitle>();

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeAlternativeTitle?> SingleAsync(int id, CancellationToken? cancellationToken = null,
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
    
    public static async Task<TanimeAlternativeTitle?> SingleAsync(int idAnime, string title, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (idAnime <= 0 || title.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + " WHERE IdAnime = $IdAnime AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$Name", title);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken).FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion
    
    #region Insert
    
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Title.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le titre est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "L'anime n'existe pas.");
        
        if (await ExistsAsync(IdAnime, Title, cancellationToken, command))
            return new OperationState<int>(false, "Le titre existe déjà.");
        
        command.CommandText = 
            """
            INSERT INTO TanimeAlternativeTitle 
                (IdAnime, Name, Description) 
            VALUES 
                ($IdAnime, $Name, $Description)
            """;

        command.Parameters.Clear();

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
    
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id est invalide.");
        
        if (Title.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le titre est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");
        
        var existingId = await GetIdOfAsync(IdAnime, Title, cancellationToken, command);
        if (existingId != null && existingId != Id)
            return new OperationState(false, "Le titre existe déjà.");
        
        command.CommandText = 
            """
            UPDATE TanimeAlternativeTitle 
            SET 
                IdAnime = $IdAnime, 
                Name = $Name, 
                Description = $Description
            WHERE Id = $Id
            """;

        command.Parameters.Clear();

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
    
    #region Delete
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (id <= 0)
            return new OperationState(false, "L'id est invalide.");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeAlternativeTitle WHERE Id = $Id";

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
                titleIndex: reader.GetOrdinal("Name"),
                descriptionIndex: reader.GetOrdinal("Description"));
        }
    }

 
    private const string SqlSelectScript =
        """
        SELECT
            Id,
            IdAnime,
            Name,
            Description
        FROM TanimeAlternativeTitle
        """;
}