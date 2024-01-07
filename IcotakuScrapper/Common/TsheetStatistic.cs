using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public partial  class TsheetStatistic : ITableSheetBase<TsheetStatistic>, ITsheetStatistic
{
    /// <summary>
    /// Obtient ou définit l'id de l'anime.
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// Obtient ou définit l'id de la fiche Icotaku de l'anime.
    /// </summary>
    public int SheetId { get; set; }

    public IcotakuSection Section { get; set; }

    /// <summary>
    /// Obtient ou définit la date de création de l'animé
    /// </summary>
    public DateTime? CreatingDate { get; set; }
    
    /// <summary>
    /// Obtient ou définit la date de la dernière mise à jour de la fiche
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre Icotaku qui a créé la fiche
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre du site d'Icotaku qui a mis à jour la fiche pour la dernière fois
    /// </summary>
    public string? LastUpdatedBy { get; set; }
    
    /// <summary>
    /// Obtient ou définit l'âge moyen des membres ayant cet anime dans leur watchlist
    /// </summary>
    public float? InWatchListAverageAge { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nombre de visite qu'a eu cette fiche jusqu'à présent
    /// </summary>
    public uint VisitCount { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre qui a visité cette fiche pour la dernière fois
    /// </summary>
    public string? LastVisitedBy { get; set; }

    public TsheetStatistic()
    {
        
    }

    public void Copy(TsheetStatistic value)
    {
        Id = value.Id;
        SheetId = value.SheetId;
        Section = value.Section;
        CreatingDate = value.CreatingDate;
        LastUpdatedDate = value.LastUpdatedDate;
        CreatedBy = value.CreatedBy;
        LastUpdatedBy = value.LastUpdatedBy;
        InWatchListAverageAge = value.InWatchListAverageAge;
        VisitCount = value.VisitCount;
        LastVisitedBy = value.LastVisitedBy;
    }
    
    public TsheetStatistic Clone()
    {
        var clone = new TsheetStatistic();
        clone.Copy(this);
        return clone;
    }

    public override string ToString()
    {
        var section = Section switch
        {
            IcotakuSection.Anime => "Anime",
            IcotakuSection.Manga => "Manga",
            IcotakuSection.Drama => "Drama",
            IcotakuSection.LightNovel => "Light Novel",
            IcotakuSection.Community => "Communauté",
            _ => "Inconnue"
        };
        return $"Stats N°{Id} de la fiche {section} n°{SheetId} créée le {CreatingDate:dd/MM/yyyy à HH:mm} par {CreatedBy} et mise à jour le {LastUpdatedDate:dd/MM/yyyy à HH:mm} par {LastUpdatedBy}.";
    }

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetStatistic";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetStatistic WHERE Id = $Id";

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
        ]);

        if (!isColumnSelectValid)
        {
            return 0;
        }

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TsheetStatistic WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TsheetStatistic WHERE SheetId = $Id",
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
        command.CommandText = "SELECT COUNT(Id) FROM TsheetStatistic WHERE Section = $Section AND SheetId = $SheetId";

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
        command.CommandText = "SELECT Id FROM TsheetStatistic WHERE Section = $Section AND SheetId = $SheetId";

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
        => await ExistsAsync(id, IntColumnSelect.Id, cancellationToken);

    public static async Task<bool> ExistsBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await ExistsAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);

    public static async Task<bool> ExistsAsync(IcotakuSection section, int sheetId,
        CancellationToken? cancellationToken = null)
        => await CountAsync(section, sheetId, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    #endregion

    #region Single
    
    public static async Task<TsheetStatistic?> SingleAsync(int id, CancellationToken? cancellationToken = null)
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
    
    public static async Task<TsheetStatistic?> SingleAsync(IcotakuSection section, int sheetId, CancellationToken? cancellationToken = null)
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
    
    public static async Task<TsheetStatistic?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var section = IcotakuWebHelpers.GetIcotakuSection(sheetUri);
        if (section is null or IcotakuSection.Community)
            return null;
        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);

        return await SingleAsync((IcotakuSection)section, sheetId, cancellationToken);
    }

    static Task<TsheetStatistic?> ITableSheetBase<TsheetStatistic>.SingleByIdAsync(int id, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    static Task<TsheetStatistic?> ITableSheetBase<TsheetStatistic>.SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    static Task<TsheetStatistic?> ITableSheetBase<TsheetStatistic>.SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (!disableVerification && await ExistsAsync(Section, SheetId, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TsheetStatistic 
                (SheetId, Section, CreatingDate, LastUpdateDate, CreatedBy, LastUpdatedBy, InReadOrWatchListAverageAge, VisitCount, LastVisitedBy)
            VALUES
                ($SheetId, $Section, $CreatingDate, $LastUpdateDate, $CreatedBy, $LastUpdatedBy, $InReadOrWatchListAverageAge, $VisitCount, $LastVisitedBy)
            """;
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$CreatingDate", CreatingDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$LastUpdateDate", LastUpdatedDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$CreatedBy", CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$LastUpdatedBy", LastUpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$InReadOrWatchListAverageAge", InWatchListAverageAge ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$VisitCount", VisitCount);
        command.Parameters.AddWithValue("$LastVisitedBy", LastVisitedBy ?? (object)DBNull.Value);
        
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

    #endregion

    #region Update

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
            UPDATE TsheetStatistic SET 
                SheetId = $SheetId, 
                Section = $Section, 
                CreatingDate = $CreatingDate,
                LastUpdateDate = $LastUpdateDate,
                CreatedBy = $CreatedBy,
                LastUpdatedBy = $LastUpdatedBy,
                InReadOrWatchListAverageAge = $InReadOrWatchListAverageAge,
                VisitCount = $VisitCount,
                LastVisitedBy = $LastVisitedBy
            WHERE Id = $Id
            """;
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$CreatingDate", CreatingDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$LastUpdateDate", LastUpdatedDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$CreatedBy", CreatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$LastUpdatedBy", LastUpdatedBy ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$InReadOrWatchListAverageAge", InWatchListAverageAge ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$VisitCount", VisitCount);
        command.Parameters.AddWithValue("$LastVisitedBy", LastVisitedBy ?? (object)DBNull.Value);
        
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

    #endregion

    public static async Task<TsheetStatistic?> SingleOrCreateAsync(TsheetStatistic value, bool reloadIfExist = false,
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

    public static async Task<OperationState<int>> AddOrUpdateAsync(TsheetStatistic value,
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

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);
    
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetStatistic WHERE Id = $Id";

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
        command.CommandText = "DELETE FROM TsheetStatistic WHERE Section = $Section AND SheetId = $SheetId";

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
    
    static Task<OperationState> ITableSheetBase<TsheetStatistic>.DeleteAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    #endregion
    
    private static async IAsyncEnumerable<TsheetStatistic> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TsheetStatistic()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                Section = (IcotakuSection)reader.GetByte(reader.GetOrdinal("Section")),
                CreatingDate = reader.IsDBNull(reader.GetOrdinal("CreatingDate"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("CreatingDate")),
                LastUpdatedDate = reader.IsDBNull(reader.GetOrdinal("LastUpdateDate"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("LastUpdateDate")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("CreatedBy")),
                LastUpdatedBy = reader.IsDBNull(reader.GetOrdinal("LastUpdatedBy"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("LastUpdatedBy")),
                LastVisitedBy = reader.IsDBNull(reader.GetOrdinal("LastVisitedBy"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("LastVisitedBy")),
                InWatchListAverageAge = reader.IsDBNull(reader.GetOrdinal("InReadOrWatchListAverageAge"))
                    ? null
                    : reader.GetFloat(reader.GetOrdinal("InReadOrWatchListAverageAge")),
                VisitCount = (uint)reader.GetInt32(reader.GetOrdinal("VisitCount"))
            };
        }
    }

    private const string IcotakuSqlSelectScript =
        """
        SELECT 
            Id, 
            SheetId, 
            Section, 
            CreatingDate, 
            LastUpdateDate, 
            CreatedBy, 
            LastUpdatedBy, 
            InReadOrWatchListAverageAge, 
            VisitCount, 
            LastVisitedBy
        FROM TsheetStatistic
        """;
}