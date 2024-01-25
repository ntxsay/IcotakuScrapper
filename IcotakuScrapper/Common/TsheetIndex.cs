using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using IcotakuScrapper.Objects.Exceptions;

namespace IcotakuScrapper.Common;

public enum SheetSortBy
{
    Id,
    Name,
    SheetId,
    Section,
    Type,
    Url,
    FoundedPage
}

/// <summary>
/// Représente un index d'une fiche. Cet index permet de retrouver une fiche à partir de son url.
/// </summary>
public partial class TsheetIndex : ITableSheetBase<TsheetIndex>
{
    /// <summary>
    /// Obtient ou définit l'identifiant de l'index
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Obtient ou définit l'identifiant de la fiche associée
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>
    /// Obtient ou définit la section icotaku concerné (anime, manga, etc.)
    /// </summary>
    public IcotakuSection Section { get; set; }

    /// <summary>
    /// Obtient ou définit le type de la fiche (anime, manga, personnage, studio, etc.)
    /// </summary>
    public IcotakuSheetType SheetType { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Obtient ou définit le nom de la fiche
    /// </summary>
    public string SheetName { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit le numéro de page où l'url a été trouvée
    /// </summary>
    public uint FoundedPage { get; set; }


    public TsheetIndex()
    {
    }

    public TsheetIndex(int id)
    {
        Id = id;
    }

    public TsheetIndex(int id, int sheetId, IcotakuSection section, string url, uint foundedPage = 0)
    {
        Id = id;
        SheetId = sheetId;
        Section = section;
        Url = url;
        FoundedPage = foundedPage;
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TsheetIndex
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    static Task<int> ITableBase<TsheetIndex>.CountAsync(int id, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    public static async Task<int> CountByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, IntColumnSelect.Id, cancellationToken);
    
    public static async Task<int> CountBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await CountAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);
    
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        IntColumnSelectException.ThrowNotSupportedException(columnSelect, nameof(columnSelect), [IntColumnSelect.Id, IntColumnSelect.SheetId]);
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM TsheetIndex WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM TsheetIndex WHERE SheetId = $Id",
            _ => null
        };
        
        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'objet <see cref="TsheetIndex"/> dans la base de données ayant la section spécifiée.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Section = $Section";

        command.Parameters.AddWithValue("$Section", (byte)section);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'objet <see cref="TsheetIndex"/> dans la base de données ayant l'url spécifiée.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'objet <see cref="TsheetIndex"/> dans la base de données ayant l'url et la section spécifiée.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(Uri uri, IcotakuSection section,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TsheetIndex WHERE Section == $Section AND Url = $Url COLLATE NOCASE";
        
        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        command.Parameters.AddWithValue("$Section", (byte)section);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Retourne l'id de l'objet <see cref="TsheetIndex"/> dans la base de données ayant l'url spécifiée.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>l'id sinon null</returns>
    public static async Task<int?> GetIdOfAsync(Uri uri,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetIndex WHERE Url = $Url COLLATE NOCASE";
        
        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    /// <summary>
    /// Retourne l'id de l'objet <see cref="TsheetIndex"/> dans la base de données ayant l'url et la section spécifiée.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>l'id sinon null</returns>
    public static async Task<int?> GetIdOfAsync(Uri uri, IcotakuSection section,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TsheetIndex WHERE Section == $Section AND Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        command.Parameters.AddWithValue("$Section", (byte)section);
        
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    static Task<bool> ITableBase<TsheetIndex>.ExistsAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    public static async Task<bool> ExistsByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await ExistsAsync(id, IntColumnSelect.Id, cancellationToken);

    public static Task<bool> ExistsBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => ExistsAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);
    
    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    /// <summary>
    /// Retourne true si l'objet <see cref="TsheetIndex"/> existe dans la base de données.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(IcotakuSection section,
        CancellationToken? cancellationToken = null)
        => await CountAsync(section, cancellationToken) > 0;

    /// <summary>
    /// Retourne true si l'objet <see cref="TsheetIndex"/> existe dans la base de données.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(Uri uri, CancellationToken? cancellationToken = null)
        => await CountAsync(uri, cancellationToken) > 0;

    /// <summary>
    /// Retourne true si l'objet <see cref="TsheetIndex"/> existe dans la base de données.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="contentSection"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(Uri uri, IcotakuSection contentSection,
        CancellationToken? cancellationToken = null)
        => await CountAsync(uri, contentSection, cancellationToken) > 0;

    #endregion

    #region Select

    /// <summary>
    /// Retourne tous les enregistrements de la table TsheetIndex
    /// </summary>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TsheetIndex[]> SelectAsync(SheetSortBy sortBy, OrderBy orderBy, uint limit = 0,
        uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine;

        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY SheetType",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            SheetSortBy.Name => " ORDER BY SheetName",
            SheetSortBy.Section => " ORDER BY Section",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };

        command.CommandText += orderBy switch
        {
            OrderBy.Asc => " ASC",
            OrderBy.Desc => " DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records;
    }

    /// <summary>
    /// Retourne tous les enregistrements de la table TsheetIndex ayant le type de contenu spécifié
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="sheetTypes"></param>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<TsheetIndex[]> SelectAsync(HashSet<IcotakuSection> sections, HashSet<IcotakuSheetType> sheetTypes, SheetSortBy sortBy,
        OrderBy orderBy, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine;

        if (sections.Count > 0)
            command.CommandText += Environment.NewLine + $"WHERE Section IN ({string.Join(", ", sections.Select(s => (byte)s))})";

        if (sheetTypes.Count > 0)
        {
            command.CommandText += sections.Count > 0 ? " AND " : Environment.NewLine + "WHERE ";
            command.CommandText += $"SheetType IN ({string.Join(", ", sheetTypes.Select(s => (byte)s))})";
        }

        command.CommandText += sortBy switch
        {
            SheetSortBy.Id => " ORDER BY Id",
            SheetSortBy.SheetId => " ORDER BY SheetId",
            SheetSortBy.Type => " ORDER BY SheetType",
            SheetSortBy.Url => " ORDER BY Url",
            SheetSortBy.FoundedPage => " ORDER BY FoundedPage",
            SheetSortBy.Name => " ORDER BY SheetName",
            SheetSortBy.Section => " ORDER BY Section",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };

        command.CommandText += orderBy switch
        {
            OrderBy.Asc => " ASC",
            OrderBy.Desc => " DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
        };

        command.AddLimitOffset(limit, skip);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records;
    }

    #endregion

    #region Single

    public static async Task<TsheetIndex?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await SingleAsync(id, IntColumnSelect.Id, cancellationToken);

    public static async Task<TsheetIndex?> SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);
    
    public static async Task<TsheetIndex?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        IntColumnSelectException.ThrowNotSupportedException(columnSelect, nameof(columnSelect), [IntColumnSelect.Id, IntColumnSelect.SheetId]);
        await using var command = Main.Connection.CreateCommand();
        
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => " WHERE Id = $Id",
            IntColumnSelect.SheetId => " WHERE SheetId = $Id",
            _ => null
        };
        
        command.Parameters.AddWithValue("$Id", id);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        var record = await GetRecordsAsync(reader, cancellationToken).SingleOrDefaultAsync();
        return record;
    }

    public static async Task<TsheetIndex?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + " WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", sheetUri.AbsoluteUri);
        
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        var record = await GetRecordsAsync(reader, cancellationToken).SingleOrDefaultAsync();
        return record;
    }

    public static async Task<TsheetIndex?> SingleAsync(string url, CancellationToken? cancellationToken = null)
    {
        if (url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + " WHERE Url = $Url COLLATE NOCASE";



        command.Parameters.AddWithValue("$Url", url.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        var records = await GetRecordsAsync(reader, cancellationToken).ToArrayAsync();
        return records.Length > 0 ? records[0] : null;
    }

    #endregion

    #region Insert
    
    /// <summary>
    /// Insère un enregistrement dans la table TsheetIndex
    /// </summary>
    /// <param name="disableVerification"></param>
    /// <param name="section"></param>
    /// <param name="sheetType"></param>
    /// <param name="name"></param>
    /// <param name="url"></param>
    /// <param name="sheetId"></param>
    /// <param name="foundedPage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState<int>> InsertAsync(bool disableVerification, IcotakuSection section, IcotakuSheetType sheetType, string name, string url,
        int sheetId, uint foundedPage, CancellationToken? cancellationToken = null)
    {
       var record = new TsheetIndex(0, sheetId, section, url, foundedPage)
       {
           SheetType = sheetType,
           SheetName = name
       };

       return await record.InsertAsync(disableVerification, cancellationToken);
    }
    
    public async Task<OperationState<int>> InsertAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (SheetName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");

        if (SheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");

        if (!disableVerification && await ExistsAsync(uri, cancellationToken))
            return new OperationState<int>(false, "L'url existe déjà dans la base de données.");

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO TsheetIndex 
                (SheetId, Url, Section, SheetType, SheetName, FoundedPage) 
            VALUES 
                ($SheetId, $Url, $Section, $SheetType, $SheetName, $FoundedPage)
            """;
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$SheetType", (byte)SheetType);
        command.Parameters.AddWithValue("$SheetName", SheetName);
        command.Parameters.AddWithValue("$FoundedPage", FoundedPage);
        
        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Impossible d'insérer l'enregistrement dans la base de données.");
            
            Id = await command.GetLastInsertRowIdAsync();
            
            return new OperationState<int>(true, "L'index a été ajouté avec succès", Id);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
        }
    }

    /// <summary>
    /// Insère une collection d'index dans la table TsheetIndex
    /// </summary>
    /// <param name="records"></param>
    /// <param name="insertMode"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> InsertAsync(IReadOnlyCollection<TsheetIndex> records,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (records.Count == 0)
            return new OperationState(false, "La liste ne peut pas être vide.");
        await using var command = Main.Connection.CreateCommand();

        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TsheetIndex (SheetId, Url, Section, SheetType, SheetName, FoundedPage)" + Environment.NewLine;
        
        for (uint i = 0; i < records.Count; i++)
        {
            var record = records.ElementAt((int)i);
            if (record.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(record.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
                return new OperationState(false, "L'url n'est pas valide.");

            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($SheetId{i}, $Url{i}, $Section{i}, $SheetType{i}, $SheetName{i}, $FoundedPage{i})";

            command.Parameters.AddWithValue($"$SheetId{i}", record.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", uri.ToString());
            command.Parameters.AddWithValue($"$Section{i}", (byte)record.Section);
            command.Parameters.AddWithValue($"$SheetType{i}", (byte)record.SheetType);
            command.Parameters.AddWithValue($"$SheetName{i}", record.SheetName);
            command.Parameters.AddWithValue($"$FoundedPage{i}", record.FoundedPage);

            Debug.WriteLine(
                $"SheetId: {record.SheetId}, Type: {record.Section}, Url: {record.Url}, FoundedPage: {record.FoundedPage}, Name: {record.SheetName}");
        }
        
        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return count == 0
                ? new OperationState(false, "Impossible d'insérer l'enregistrement dans la base de données.")
                : new OperationState(true,
                    $"{count} enregistrement(s) sur {records.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion de l'enregistrement.");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification, CancellationToken? cancellationToken = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id de l'enregistrement n'est pas valide");

        if (SheetName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de la fiche ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url de la fiche ne peut pas être vide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'url de la fiche n'est pas valide");

        if (SheetId <= 0)
            return new OperationState(false, "L'id de la fiche n'est pas valide");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(uri, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "Cet index existe déjà dans la base de données.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE TsheetIndex
            SET
                SheetId = $SheetId,
                Section = $Section,
                SheetType = $SheetType,
                SheetName = $SheetName,
                Url = $Url,
                FoundedPage = $FoundedPage
            WHERE
                Id = $Id
            """;
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        command.Parameters.AddWithValue("$FoundedPage", FoundedPage);
        command.Parameters.AddWithValue("$SheetType", (byte)SheetType);
        command.Parameters.AddWithValue("$SheetName", SheetName);

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0
                ? new OperationState(false, "Impossible de mettre à jour l'enregistrement dans la base de données.")
                : new OperationState(true, "L'enregistrement a été mis à jour avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour de l'enregistrement.");
        }
    }

    #endregion

    #region AddOrUpdate

    public static async Task<TsheetIndex?> SingleOrCreateAsync(TsheetIndex value, bool reloadIfExist = false,
        CancellationToken? cancellationToken = null)
    {
        if (value.Url.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return null;
        
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(uri, value.Section, cancellationToken);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(uri, cancellationToken);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(false, cancellationToken);
        return !result.IsSuccess ? null : value;
    }

    public async Task<OperationState<int>> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);

    public static async Task<OperationState<int>> AddOrUpdateAsync(TsheetIndex value, CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (value.Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche ne peut pas être vide");

        if (!Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche n'est pas valide");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(uri, value.Section, cancellationToken);

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

    static Task<OperationState> ITableSheetBase<TsheetIndex>.DeleteAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText =
            "DELETE FROM TsheetIndex WHERE Id = $Id";
        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TsheetIndex WHERE Url = $Url COLLATE NOCASE";
        
        command.Parameters.AddWithValue("$Url", uri.AbsoluteUri);
        
        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    
    public static async Task<OperationState> DeleteAllAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex";

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex WHERE Section = $Section";
        
        command.Parameters.AddWithValue("$Section", (byte)section);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, IcotakuSheetType sheetType, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();

        command.CommandText = "DELETE FROM TsheetIndex WHERE Section = $Section AND SheetType = $SheetType";
        
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$SheetType", (byte)sheetType);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} enregistrement(s) ont été supprimés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'enregistrement.");
        }
    }
    #endregion

    private static async IAsyncEnumerable<TsheetIndex> GetRecordsAsync(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                sheetIdIndex: reader.GetOrdinal("SheetId"),
                sectionIndex: reader.GetOrdinal("Section"),
                typeIndex: reader.GetOrdinal("SheetType"),
                urlIndex: reader.GetOrdinal("Url"),
                nameIndex: reader.GetOrdinal("SheetName"),
                foundedPageIndex: reader.GetOrdinal("FoundedPage"));
        }
    }

    /// <summary>
    ///  Obtient un enregistrement de la table TsheetIndex à partir du lecteur de données.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="idIndex"></param>
    /// <param name="sectionIndex"></param>
    /// <param name="typeIndex"></param>
    /// <param name="urlIndex"></param>
    /// <param name="nameIndex"></param>
    /// <param name="foundedPageIndex"></param>
    /// <param name="sheetIdIndex"></param>
    /// <returns></returns>
    internal static TsheetIndex GetRecord(SqliteDataReader reader, int idIndex, int sheetIdIndex, int sectionIndex, int typeIndex,
        int urlIndex, int nameIndex,
        int foundedPageIndex)
    {
        var record = new TsheetIndex
        {
            Id = reader.GetInt32(idIndex),
            SheetId = reader.GetInt32(sheetIdIndex),
            Section = (IcotakuSection)reader.GetByte(sectionIndex),
            SheetType = (IcotakuSheetType)reader.GetByte(typeIndex),
            Url = reader.GetString(urlIndex),
            SheetName = reader.GetString(nameIndex),
            FoundedPage = (uint)reader.GetInt32(foundedPageIndex)
        };

        return record;
    }

    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Id,
            SheetId,
            Url,
            Section,
            SheetName,
            SheetType,
            FoundedPage
        FROM TsheetIndex
        """;
    
    
}