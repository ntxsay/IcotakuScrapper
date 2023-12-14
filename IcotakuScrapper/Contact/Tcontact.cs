using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Contact;

public enum ContactSortBy
{
    Id,
    SheetId,
    DisplayName,
}

/// <summary>
/// Représente un contact avec des informations détaillées.
/// </summary>
public partial class Tcontact : TcontactBase
{
    /// <summary>
    /// Obtient ou définit le genre du contact.
    /// </summary>
    public TcontactGenre? Genre { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom de naissance du contact.
    /// </summary>
    public string? BirthName { get; set; }
    
    /// <summary>
    /// Obtient ou définit le prénom du contact.
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom d'origine du contact.
    /// </summary>
    public string? OriginalName { get; set; }
    
    /// <summary>
    /// Obtient ou définit la date de naissance du contact.
    /// </summary>
    public string? BirthDate { get; set; }
    
    /// <summary>
    /// Obtient ou définit l'âge du contact.
    /// </summary>
    public uint? Age { get; set; }
    
    /// <summary>
    /// Obtient ou définit la liste des sites web de l'anime.
    /// </summary>
    public HashSet<TcontactWebSite> WebSites { get; } = [];

    public Tcontact() { }

    public Tcontact(int id)
    {
        Id = id;
    }
    
    public Tcontact(int sheetId, ContactType contactType, Uri sheetUri, string displayName)
    {
        SheetId = sheetId;
        Type = contactType;
        Url = sheetUri.ToString();
        DisplayName = displayName;
    }
    
    public override string ToString() => $"{DisplayName} ({Type})";
    
    #region Select

    public static async Task<Tcontact[]> SelectAsync(ContactSortBy sortBy, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += $"ORDER BY {sortBy} {orderBy}";
        
        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        
        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Single

    public new static async Task<Tcontact?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);
        
        if (!isColumnSelectValid)
        {
            return null;
        }
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Tcontact.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tcontact.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", id);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public new static async Task<Tcontact?> SingleAsync(string displayName, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.DisplayName = $DisplayName COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", displayName);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    
    public new static async Task<Tcontact?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.Url = $Url COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    public new static async Task<Tcontact?> SingleAsync(string displayName, int sheetId, ContactType contactType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.DisplayName = $DisplayName COLLATE NOCASE AND Tcontact.Type = $Type AND Tcontact.SheetId = $SheetId";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", displayName);
        command.Parameters.AddWithValue("$Type", (byte)contactType);
        command.Parameters.AddWithValue("$SheetId", sheetId);
        
        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }
    #endregion
    
    #region Insert

    public async Task<OperationState<int>> InsertAync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom d'affichage du contact ne peut pas être vide");
        
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche du contact ne peut pas être vide");
        
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            return new OperationState<int>(false, "L'url de la fiche du contact est invalide");
        
        if (SheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche du contact ne peut pas être vide");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (await ExistsAsync(DisplayName, SheetId, Type, cancellationToken, command))
            return new OperationState<int>(false, "Le contact existe déjà dans la base de données");
        
        command.CommandText = 
            """
            INSERT INTO Tcontact 
                (SheetId, IdGenre, Type, DisplayName, Presentation, Url, BirthName, FirstName, OriginalName, BirthDate, Age) 
            VALUES 
                ($SheetId, $IdGenre, $Type, $DisplayName, $Presentation, $Url, $BirthName, $FirstName, $OriginalName, $BirthDate, $Age)
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$IdGenre", Genre?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Type", (byte)Type);
        command.Parameters.AddWithValue("$DisplayName", DisplayName);
        command.Parameters.AddWithValue("$Presentation", Presentation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$BirthName", BirthName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$FirstName", FirstName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$OriginalName", OriginalName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$BirthDate", BirthDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Age", Age ?? (object)DBNull.Value);
        
        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            if (result == 0)
                return new OperationState<int>(false, "Aucun contact n'a été inséré dans la base de données");

            Id = await command.GetLastInsertRowIdAsync();

            return new OperationState<int>(true, "Le contact a été inséré avec succès", Id);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion du contact");
        }
    }
    
    /// <summary>
    /// Retourne un contact existant ou crée un nouveau contact.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tcontact?> SingleOrCreateAsync(Tcontact value, bool reloadIfExist= false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.Parameters.Clear();
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.DisplayName, value.SheetId, value.Type, cancellationToken, command);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
            
            var result2 = await value.InsertAync(cancellationToken, command);
            return !result2.IsSuccess ? null : value;
        }

        var record = await SingleAsync(new Uri(value.Url), cancellationToken, command);
        if (record != null)
            return record;

        var result = await value.InsertAync(cancellationToken, command);
        return !result.IsSuccess ? null : value;
    }
    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Id <= 0)
            return new OperationState(false, "L'id du contact ne peut pas être vide");
        
        if (DisplayName.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom d'affichage du contact ne peut pas être vide");
        
        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url de la fiche du contact ne peut pas être vide");
        
        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            return new OperationState(false, "L'url de la fiche du contact est invalide");
        
        if (SheetId <= 0)
            return new OperationState(false, "L'id de la fiche du contact ne peut pas être vide");
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var existingId = await GetIdOfAsync(DisplayName, SheetId, Type, cancellationToken, command);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "Le contact existe déjà dans la base de données");
        
        command.CommandText = 
            """
            UPDATE Tcontact SET 
                SheetId = $SheetId, 
                IdGenre = $IdGenre, 
                Type = $Type, 
                DisplayName = $DisplayName, 
                Presentation = $Presentation, 
                Url = $Url, 
                BirthName = $BirthName, 
                FirstName = $FirstName, 
                OriginalName = $OriginalName, 
                BirthDate = $BirthDate, 
                Age = $Age
            WHERE Id = $Id
            """;

        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$IdGenre", Genre?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Type", (byte)Type);
        command.Parameters.AddWithValue("$DisplayName", DisplayName);
        command.Parameters.AddWithValue("$Presentation", Presentation ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Url", uri.ToString());
        command.Parameters.AddWithValue("$BirthName", BirthName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$FirstName", FirstName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$OriginalName", OriginalName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$BirthDate", BirthDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Age", Age ?? (object)DBNull.Value);
        
        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result == 0 
                ? new OperationState(false, "Une erreur est survenue lors de la mise à jour du contact") 
                : new OperationState(true, "Le contact a été mis à jour avec succès");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour du contact");
        }
    }

    #endregion
    
    private static async Task<Tcontact[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<Tcontact> recordList = [];
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var id = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var record = recordList.FirstOrDefault(x => x.Id == id);
            if (record == null)
            {

                record = new Tcontact()
                {
                    Id = id,
                    Guid = reader.GetGuid(reader.GetOrdinal("BaseGuid")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("SheetId")),
                    DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                    Url = reader.GetString(reader.GetOrdinal("Url")),
                    Presentation = reader.IsDBNull(reader.GetOrdinal("Presentation"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Presentation")),
                    BirthName = reader.IsDBNull(reader.GetOrdinal("BirthName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BirthName")),
                    FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("FirstName")),
                    OriginalName = reader.IsDBNull(reader.GetOrdinal("OriginalName"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("OriginalName")),
                    BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("BirthDate")),
                    Age = reader.IsDBNull(reader.GetOrdinal("Age"))
                        ? null
                        : reader.GetByte(reader.GetOrdinal("Age")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ThumbnailUrl")),
                    Type = (ContactType)reader.GetByte(reader.GetOrdinal("Type")),
                    Genre = reader.IsDBNull(reader.GetOrdinal("IdGenre"))
                        ? null
                        : TcontactGenre.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdGenre"),
                            nameIndex: reader.GetOrdinal("GenreName"),
                            descriptionIndex: reader.GetOrdinal("GenreDescription"))

                };
                
                recordList.Add(record);
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("WebSiteId")))
            {
                var webSiteId = reader.GetInt32(reader.GetOrdinal("WebSiteId"));
                var webSite = record.WebSites.FirstOrDefault(x => x.Id == webSiteId);
                if (webSite == null)
                {
                    webSite = new TcontactWebSite(webSiteId, id)
                    {
                        Url = reader.GetString(reader.GetOrdinal("WebSiteUrl")),
                        Description = reader.IsDBNull(reader.GetOrdinal("WebSiteDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("WebSiteDescription"))
                    };
                    record.WebSites.Add(webSite);
                }
            }
        }
        
        return recordList.ToArray();
    }
    

    private const string SqlSelectScript =
        """
        SELECT
            Tcontact.Id AS BaseId,
            Tcontact.Guid AS BaseGuid,
            Tcontact.SheetId,
            Tcontact.IdGenre,
            Tcontact.Type,
            Tcontact.DisplayName,
            Tcontact.Presentation,
            Tcontact.Url,
            Tcontact.BirthName,
            Tcontact.FirstName,
            Tcontact.OriginalName,
            Tcontact.BirthDate,
            Tcontact.Age,
            Tcontact.ThumbnailUrl,
            
            TcontactGenre.Name AS GenreName,
            TcontactGenre.Description AS GenreDescription,
            
            TcontactWebSite.Id AS WebSiteId,
            TcontactWebSite.Url AS WebSiteUrl,
            TcontactWebSite.Description AS WebSiteDescription
        
        FROM Tcontact
        LEFT JOIN main.TcontactGenre on TcontactGenre.Id = Tcontact.IdGenre
        LEFT JOIN main.TcontactWebSite on Tcontact.Id = TcontactWebSite.IdContact
        """;
    
    
}