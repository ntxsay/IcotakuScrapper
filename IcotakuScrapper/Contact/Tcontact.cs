using System.Diagnostics;
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
public class Tcontact : TcontactBase
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
            return Array.Empty<Tcontact>();
        
        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Single

    public new static async Task<Tcontact?> SingleAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect, new HashSet<IntColumnSelect>()
        {
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        });
        
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
    
    public new static async Task<Tcontact?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;
        
        command.CommandText += "WHERE Tcontact.DisplayName = $DisplayName COLLATE NOCASE";
        
        command.Parameters.Clear();
        
        command.Parameters.AddWithValue("$DisplayName", name);
        
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
        if (await ExistsAsync(DisplayName, SheetId, uri, cancellationToken, command))
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
                return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion du contact");

            Id = await command.GetLastInsertRowIdAsync();

            return new OperationState<int>(true, "Le contact a été inséré avec succès", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion du contact");
        }
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
        var existingId = await GetIdOfAsync(DisplayName, SheetId, uri, cancellationToken, command);
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
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour du contact");
        }
    }

    #endregion
    
    private static async Task<Tcontact[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<Tcontact> recordList = new();
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var id = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var record = recordList.FirstOrDefault(x => x.Id == id);
            if (record == null)
            {
                record = GetRecord(reader,
                    idIndex: reader.GetOrdinal("BaseId"),
                    sheetIdIndex: reader.GetOrdinal("SheetId"),
                    displayNameIndex: reader.GetOrdinal("DisplayName"),
                    presentationIndex: reader.GetOrdinal("Presentation"),
                    urlIndex: reader.GetOrdinal("Url"),
                    birthNameIndex: reader.GetOrdinal("BirthName"),
                    firstNameIndex: reader.GetOrdinal("FirstName"),
                    originalNameIndex: reader.GetOrdinal("OriginalName"),
                    birthDateIndex: reader.GetOrdinal("BirthDate"),
                    ageIndex: reader.GetOrdinal("Age"),
                    thumbnailIndex: reader.GetOrdinal("ThumbnailUrl"),
                    contactTypeIndex: reader.GetOrdinal("Type"),
                    idGenreIndex: reader.GetOrdinal("IdGenre"),
                    genreNameIndex: reader.GetOrdinal("GenreName"),
                    genreDescriptionIndex: reader.GetOrdinal("GenreDescription"));
                
                recordList.Add(record);
            }
        }
        
        return recordList.ToArray();
    }
    
    public static Tcontact GetRecord(SqliteDataReader reader, int idIndex, int sheetIdIndex, int displayNameIndex, int presentationIndex, int urlIndex, 
        int birthNameIndex, int firstNameIndex, int originalNameIndex, int birthDateIndex, int ageIndex, int contactTypeIndex, int thumbnailIndex,
        int idGenreIndex, int genreNameIndex, int genreDescriptionIndex)
    {
        var record = new Tcontact()
        {
            Id = reader.GetInt32(idIndex),
            SheetId = reader.GetInt32(sheetIdIndex),
            DisplayName = reader.GetString(displayNameIndex),
            Presentation = reader.IsDBNull(presentationIndex)
                ? null
                : reader.GetString(presentationIndex),
            Url = reader.GetString(urlIndex),
            BirthName = reader.IsDBNull(birthNameIndex)
                ? null
                : reader.GetString(birthNameIndex),
            FirstName = reader.IsDBNull(firstNameIndex)
                ? null
                : reader.GetString(firstNameIndex),
            OriginalName = reader.IsDBNull(originalNameIndex)
                ? null
                : reader.GetString(originalNameIndex),
            BirthDate = reader.IsDBNull(birthDateIndex)
                ? null
                : reader.GetString(birthDateIndex),
            Age = reader.IsDBNull(ageIndex)
                ? null
                : reader.GetByte(ageIndex),
            ThumbnailUrl = reader.IsDBNull(thumbnailIndex)
                ? null
                : reader.GetString(thumbnailIndex),
            Type = (ContactType)reader.GetByte(contactTypeIndex),
            Genre = reader.IsDBNull(idGenreIndex)
                ? null
                : TcontactGenre.GetRecord(reader,
                    idIndex: idGenreIndex,
                    nameIndex: genreNameIndex,
                    descriptionIndex: genreDescriptionIndex)
        };
        
        return record;
    }

    private const string SqlSelectScript =
        """
        SELECT
            Tcontact.Id AS BaseId,
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
            TcontactGenre.Description AS GenreDescription
        
        FROM Tcontact
        LEFT JOIN main.TcontactGenre on TcontactGenre.Id = Tcontact.IdGenre
        """;
    
    
}