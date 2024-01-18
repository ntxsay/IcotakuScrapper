using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public partial class TsheetMostAwaitedPopular : ITableSheetBase<TsheetMostAwaitedPopular>
{
    public int Id { get; protected set; }
    public int SheetId { get; set; }
    public int Rank { get; set; }
    public int VoteCount { get; set; }
    public double Note { get; set; }
    /// <summary>
    /// Obtient ou définit la section icotaku concerné (anime, manga, etc.)
    /// </summary>
    public IcotakuSection Section { get; set; }

    /// <summary>
    /// Obtient ou définit le type de la fiche (anime, manga, personnage, studio, etc.)
    /// </summary>
    public IcotakuSheetType SheetType { get; set; }
    
    public IcotakuListType ListType { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Obtient ou définit le nom de la fiche
    /// </summary>
    public string SheetName { get; set; } = string.Empty;

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetMostAwaitedPopular";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    static Task<int> ITableBase<TsheetMostAwaitedPopular>.CountAsync(int id, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
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
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TsheetMostAwaitedPopular WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TsheetMostAwaitedPopular WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetMostAwaitedPopular WHERE Url = $Url COLLATE NOCASE";
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int> CountAsync(Uri sheetUri, IcotakuListType listType,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetMostAwaitedPopular WHERE Url = $Url COLLATE NOCASE AND ListType = $ListType";
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$ListType", (byte)listType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }
    
    public static async Task<int?> GetIdOfAsync(Uri sheetUri, IcotakuListType listType,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetMostAwaitedPopular WHERE Url = $Url COLLATE NOCASE AND ListType = $ListType";
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$ListType", (byte)listType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, IntColumnSelect.Id, cancellationToken) > 0;

    public static async Task<bool> ExistsBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await CountAsync(sheetId, IntColumnSelect.SheetId, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;
    
    public static async Task<bool> ExistsAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
        => await CountAsync(sheetUri, cancellationToken) > 0;
    
    public static async Task<bool> ExistsAsync(Uri sheetUri, IcotakuListType listType, CancellationToken? cancellationToken = null)
        => await CountAsync(sheetUri, listType, cancellationToken) > 0;

    #endregion
    
    #region Single

    public static async Task<TsheetMostAwaitedPopular?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await SingleAsync(id, IntColumnSelect.Id, cancellationToken);

    public static async Task<TsheetMostAwaitedPopular?> SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);

    public static async Task<TsheetMostAwaitedPopular?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);

        if (!isColumnSelectValid)
        {
            return null;
        }

        command.CommandText = SqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Id = $Id",
            IntColumnSelect.SheetId => "SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TsheetMostAwaitedPopular?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        if (sheetUri.IsAbsoluteUri == false)
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<TsheetMostAwaitedPopular?> SingleAsync(Uri sheetUri, IcotakuListType listType,
        CancellationToken? cancellationToken = null)
    {
        
        if (sheetUri.IsAbsoluteUri == false)
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Url = $Url COLLATE NOCASE AND ListType = $ListType";

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$ListType", (byte)listType);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }
    
    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (SheetName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url ne peut pas être vide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            return new OperationState<int>(false, "L'url n'est pas valide");

        if (!disableVerification && await ExistsAsync(uri, ListType, cancellationToken))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TsheetMostAwaitedPopular
                (SheetId,  Rank,  VoteCount, Note, Section,  SheetType,  ListType,  Url, SheetName)
            VALUES
                ($SheetId,  $Rank,  $VoteCount,  $Note,  $Section,  $SheetType,  $ListType,  $Url,  $SheetName)
            """;
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Rank", Rank);
        command.Parameters.AddWithValue("$VoteCount", VoteCount);
        command.Parameters.AddWithValue("$Note", Note);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$SheetType", (byte)SheetType);
        command.Parameters.AddWithValue("$ListType", (byte)ListType);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$SheetName", SheetName);

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
    
    public static async Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<TsheetMostAwaitedPopular> values, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TsheetMostAwaitedPopular (SheetId,  Rank,  VoteCount, Note, Section,  SheetType,  ListType,  Url, SheetName)";

        

        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);

            if (value.SheetName.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom de l'item ne peut pas être vide (id: {i}");
                continue;
            }

            if (value.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            {
                LogServices.LogDebug($"L'url ne peut pas être vide. (name: {values.ElementAt((int)i).SheetName}, id: {i}");
                continue;
            }

            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($SheetId{i},  $Rank{i},  $VoteCount{i},  $Note{i},  $Section{i},  $SheetType{i},  $ListType{i},  $Url{i},  $SheetName{i})";

            command.Parameters.AddWithValue($"$SheetId{i}", value.SheetId);
            command.Parameters.AddWithValue($"$Rank{i}", value.Rank);
            command.Parameters.AddWithValue($"$VoteCount{i}", value.VoteCount);
            command.Parameters.AddWithValue($"$Note{i}", value.Note);
            command.Parameters.AddWithValue($"$Section{i}", (byte)value.Section);
            command.Parameters.AddWithValue($"$SheetType{i}", (byte)value.SheetType);
            command.Parameters.AddWithValue($"$ListType{i}", (byte)value.ListType);
            command.Parameters.AddWithValue($"$Url{i}", value.Url);
            command.Parameters.AddWithValue($"$SheetName{i}", value.SheetName);
            

            LogServices.LogDebug("Ajout de l'item " + value.SheetName + " à la commande.");
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

    public static async Task<TsheetMostAwaitedPopular?> SingleOrCreateAsync(TsheetMostAwaitedPopular value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        if (value.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(uri, value.ListType, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(uri, value.ListType, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }
    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (SheetName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'url ne peut pas être vide");

        if (!disableVerification)
        {
            var id = await GetIdOfAsync(uri, ListType, cancellationToken);
            if (id.HasValue && id.Value != Id)
                return new OperationState(false, "Le nom de l'item existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TsheetMostAwaitedPopular
            SET
                SheetId = $SheetId,
                Rank = $Rank,
                VoteCount = $VoteCount,
                Note = $Note,
                Section = $Section,
                SheetType = $SheetType,
                ListType = $ListType,
                Url = $Url,
                SheetName = $SheetName
            WHERE Id = $Id
            """;

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Rank", Rank);
        command.Parameters.AddWithValue("$VoteCount", VoteCount);
        command.Parameters.AddWithValue("$Note", Note);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$SheetType", (byte)SheetType);
        command.Parameters.AddWithValue("$ListType", (byte)ListType);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$SheetName", SheetName);
        command.Parameters.AddWithValue("$Id", Id);
        

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0
                ? new OperationState(false, "Une erreur est survenue lors de la mise à jour")
                : new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region AddOrUpdate
    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(TsheetMostAwaitedPopular value, CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Section == IcotakuSection.Community)
            return new OperationState<int>(false, "La section de la fiche est invalide.");

        if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url n'est pas valide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(uri, value.ListType, cancellationToken);

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
    #endregion

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, IntColumnSelect.Id, cancellationToken);
    
    public static async Task<OperationState> DeleteAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);

        if (!isColumnSelectValid)
        {
            return new OperationState(false, "La colonne sélectionnée n'est pas valide");
        }

        command.CommandText =  columnSelect switch
        {
            IntColumnSelect.Id => "DELETE FROM TsheetMostAwaitedPopular WHERE Id = $Id",
            IntColumnSelect.SheetId => "DELETE FROM TsheetMostAwaitedPopular WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        command.Parameters.AddWithValue("$Id", id);
        
        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    public static async Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetMostAwaitedPopular WHERE Url = $Url COLLATE NOCASE";
        command.Parameters.AddWithValue("$Url", uri.ToString());
        
        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }
    
    public static async Task<OperationState> DeleteAsync(Uri uri, IcotakuListType listType, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetMostAwaitedPopular WHERE Url = $Url COLLATE NOCASE AND ListType = $ListType";
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$ListType", (byte)listType);
        
        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    public static async Task<OperationState> DeleteAsync(IcotakuListType listType,
        CancellationToken? cancellationToken = null)
    {
        
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetMostAwaitedPopular WHERE ListType = $ListType";
        command.Parameters.AddWithValue("$ListType", (byte)listType);
        
        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }
    
    public static async Task<OperationState> DeleteAsync(IReadOnlyCollection<TsheetMostAwaitedPopular> values, CancellationToken? cancellationToken = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetMostAwaitedPopular WHERE Id IN (";
        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);
            command.CommandText += i == 0 ? "$Id" : ", $Id";
            command.Parameters.AddWithValue($"$Id{i}", value.Id);
        }

        command.CommandText += ")";
        
        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }
    
    public static async Task<OperationState> DeleteAllAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetMostAwaitedPopular";

        try
        {
            var countRowAffected = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{countRowAffected} enregistrement(s) supprimé(s) avec succès.");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    #endregion
    
    private static async IAsyncEnumerable<TsheetMostAwaitedPopular> GetRecords(SqliteDataReader reader, CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return new TsheetMostAwaitedPopular
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                Rank = reader.GetInt32(reader.GetOrdinal("Rank")),
                VoteCount = reader.GetInt32(reader.GetOrdinal("VoteCount")),
                Note = reader.GetDouble(reader.GetOrdinal("Note")),
                Section = (IcotakuSection)reader.GetInt32(reader.GetOrdinal("Section")),
                SheetType = (IcotakuSheetType)reader.GetInt32(reader.GetOrdinal("SheetType")),
                ListType = (IcotakuListType)reader.GetInt32(reader.GetOrdinal("ListType")),
                Url = reader.GetString(reader.GetOrdinal("Url")),
                SheetName = reader.GetString(reader.GetOrdinal("SheetName")),
            };
        }
    }

    

    private const string SqlSelectScript =
        """
        SELECT 
            Id, 
            SheetId, 
            Rank, 
            VoteCount, 
            Note, 
            Section, 
            SheetType, 
            ListType, 
            Url, 
            SheetName 
        FROM TsheetMostAwaitedPopular
        """;
}