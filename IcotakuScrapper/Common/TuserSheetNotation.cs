using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public partial class TuserSheetNotation : ITableSheetBase<TuserSheetNotation>
{
    public int Id { get; protected set; }
    public int SheetId { get; set; }
    public int? IdAnime { get; set; }
    public IcotakuSection Section { get; set; }
    public WatchStatusKind WatchStatus { get; set; }
    public float? Note { get; set; }
    public string? PublicComment { get; set; }
    public string? PrivateComment { get; set; }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TuserSheetNotation";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TuserSheetNotation WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
            IntColumnSelect.IdAnime,
        ]);

        if (!isColumnSelectValid)
        {
            return 0;
        }

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TuserSheetNotation WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TuserSheetNotation WHERE SheetId = $Id",
            IntColumnSelect.IdAnime => "SELECT COUNT(Id) FROM TuserSheetNotation WHERE IdAnime = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TuserSheetNotation WHERE Section = $Section AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$Section", section);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int?> GetIdOfAsync(IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TuserSheetNotation WHERE Section = $Section AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$Section", section);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    
    #endregion
    
    #region Exists

    public static async Task<bool> ExistsByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;
    
    public static async Task<bool> ExistsBySheetIdAsync(int sheetId, CancellationToken? cancellationToken)
        => await ExistsAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;
    
    public static async Task<bool> ExistsAsync(IcotakuSection section, int sheetId, CancellationToken? cancellationToken = null)
        => await CountAsync(section, sheetId, cancellationToken) > 0;

    #endregion

    #region Single

    public static async Task<TuserSheetNotation?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<TuserSheetNotation?> SingleAsync(IcotakuSection section, int sheetId, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE section = $Section AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }
    
    public static async Task<TuserSheetNotation?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var section = IcotakuWebHelpers.GetIcotakuSection(sheetUri);
        if (section is null or IcotakuSection.Community)
            return null;
        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);

        return await SingleAsync((IcotakuSection)section, sheetId, cancellationToken);
    }

    static Task<TuserSheetNotation?> ITableSheetBase<TuserSheetNotation>.SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    static Task<TuserSheetNotation?> ITableSheetBase<TuserSheetNotation>.SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    
    #endregion

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        //Vérifie si l'item existe déjà
        if (!disableVerification && await ExistsAsync(Section, SheetId, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        //Initialise la commande SQLite
        await using var command = Main.Connection.CreateCommand();
        
        //Définit la commande SQL
        command.CommandText =
            """
            INSERT INTO TuserSheetNotation 
                (IdAnime, Section, SheetId, WatchingStatus, Note, PublicComment, PrivateComment) 
            VALUES 
                ($IdAnime, $Section, $SheetId, $WatchingStatus, $Note, $PublicComment, $PrivateComment)
            """;
        
        //Ajoute les paramètres
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$WatchingStatus", (byte)WatchStatus);
        command.Parameters.AddWithValue("$Note", Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$PublicComment", PublicComment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$PrivateComment", PrivateComment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdAnime", IdAnime ?? (object)DBNull.Value);
        
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
        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(Section, SheetId, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "L'item existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TuserSheetNotation SET
                IdAnime = $IdAnime,
                Section = $Section,
                SheetId = $SheetId,
                WatchingStatus = $WatchingStatus,
                Note = $Note,
                PublicComment = $PublicComment,
                PrivateComment = $PrivateComment
            WHERE Id = $Id
            """;
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$WatchingStatus", (byte)WatchStatus);
        command.Parameters.AddWithValue("$Note", Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$PublicComment", PublicComment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$PrivateComment", PrivateComment ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdAnime", IdAnime ?? (object)DBNull.Value);
        
        
        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0
                ? new OperationState(false, "Aucune mise à jour n'a été effectuée.")
                : new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<TuserSheetNotation?> SingleOrCreateAsync(TuserSheetNotation value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.Section, value.SheetId, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.Section, value.SheetId, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }
    
    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(TuserSheetNotation value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Section == IcotakuSection.Community)
            return new OperationState<int>(false, "La section de la fiche est invalide.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.Section, value.SheetId, cancellationToken);

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
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TuserSheetNotation WHERE Id = $Id";

        command.Parameters.AddWithValue("$Id", id);

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
    
    public static async Task<OperationState> DeleteAsync(IcotakuSection section, int sheetId, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TuserSheetNotation WHERE Section = $Section AND SheetId = $SheetId";

        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$SheetId", sheetId);

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
    
    public static async Task<OperationState> DeleteAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var section = IcotakuWebHelpers.GetIcotakuSection(sheetUri);
        if (section is null or IcotakuSection.Community)
            return new OperationState(false, "La section de la fiche est invalide.");
        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
        
        return await DeleteAsync((IcotakuSection)section, sheetId, cancellationToken);
    }
    
    static Task<OperationState> ITableSheetBase<TuserSheetNotation>.DeleteAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    private static async IAsyncEnumerable<TuserSheetNotation> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TuserSheetNotation()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                Section = (IcotakuSection)reader.GetByte(reader.GetOrdinal("Section")),
                WatchStatus = (WatchStatusKind)reader.GetByte(reader.GetOrdinal("WatchingStatus")),
                Note = reader.IsDBNull(reader.GetOrdinal("Note"))
                    ? null
                    : reader.GetFloat(reader.GetOrdinal("Note")),
                PublicComment = reader.IsDBNull(reader.GetOrdinal("PublicComment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PublicComment")),
                PrivateComment = reader.IsDBNull(reader.GetOrdinal("PrivateComment"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("PrivateComment")),
                IdAnime = reader.IsDBNull(reader.GetOrdinal("IdAnime"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("IdAnime")),
            };
        }
    }
    
    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Id,
            IdAnime,
            SheetId,
            Section,
            WatchingStatus,
            Note,
            PublicComment,
            PrivateComment
        FROM TuserSheetNotation
        """;
}