using IcotakuScrapper.Extensions;

using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Common;

public enum CategorySortBy
{
    Id,
    Name,
    Type
}

/// <summary>
/// Représente un format de diffusion d'un anime ou Manga ou autre
/// </summary>
public partial class Tcategory
{
    public int Id { get; protected set; }
    public int SheetId { get; set; }
    public CategoryType Type { get; set; }
    public IcotakuSection Section { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Url { get; set; } = null!;

    public Tcategory()
    {
    }

    public Tcategory(int id)
    {
        Id = id;
    }

    public Tcategory(IcotakuSection section, CategoryType categoryType, int sheetId, Uri sheetUri, string name, string? description = null)
    {
        SheetId = sheetId;
        Url = sheetUri.ToString();
        Section = section;
        Type = categoryType;
        Name = name;
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} ({Type})";
    }

    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tcategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcategory";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tcategory ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
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
            return 0;
        }

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM Tcategory WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM Tcategory WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tcategory ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcategory WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table Tcategory ayant le nom et le type spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="categoryType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(string name, CategoryType categoryType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcategory WHERE Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(string name, IcotakuSection section, CategoryType categoryType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tcategory WHERE Section = $Section AND Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(string name, CategoryType categoryType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tcategory WHERE Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(string name, IcotakuSection section, CategoryType categoryType,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tcategory WHERE Section = $Section AND Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, CategoryType categoryType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, categoryType, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, IcotakuSection section, CategoryType categoryType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, section, categoryType, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    /// <summary>
    /// Sélectionne tout ou une partie des enregistrements de la table Tcategory
    /// </summary>
    /// <param name="sections"></param>
    /// <param name="categoryType"></param>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tcategory[]> SelectAsync(HashSet<IcotakuSection> sections, HashSet<CategoryType> categoryType, FormatSortBy sortBy = FormatSortBy.Name,
        OrderBy orderBy = OrderBy.Asc,
        uint limit = 0, uint skip = 0,
        CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript;

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        if (sections.Count > 0)
        {
            command.CommandText += Environment.NewLine + "WHERE Section IN (";
            for (var i = 0; i < sections.Count; i++)
            {
                command.CommandText += i == 0 ? $"$Section{i}" : $", $Section{i}";
                command.Parameters.AddWithValue($"$Section{i}", (byte)sections.ElementAt(i));
            }
            command.CommandText += ")";
        }

        if (categoryType.Count > 0)
        {
            if (sections.Count > 0)
                command.CommandText += Environment.NewLine + "AND Type IN (";
            else
                command.CommandText += Environment.NewLine + "WHERE Type IN (";

            for (var i = 0; i < categoryType.Count; i++)
            {
                command.CommandText += i == 0 ? $"$CategoryType{i}" : $", $CategoryType{i}";
                command.Parameters.AddWithValue($"$CategoryType{i}", (byte)categoryType.ElementAt(i));
            }
            command.CommandText += ")";
        }

        command.CommandText += Environment.NewLine + $"ORDER BY {sortBy} {orderBy}";

        if (limit > 0)
            command.CommandText += Environment.NewLine + $"LIMIT {limit} OFFSET {skip}";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<Tcategory?> SingleAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Retourne un enregistrement de la table Tcategory ayant le nom spécifié
    /// </summary>
    /// <param name="name"></param>
    /// <param name="section"></param>
    /// <param name="categoryType"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tcategory?> SingleAsync(string name, IcotakuSection section, CategoryType categoryType, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Section = $Section COLLATE NOCASE AND Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<Tcategory?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (sheetUri.IsAbsoluteUri == false)
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    public static async Task<Tcategory?> SingleOrCreateAsync(string name, IcotakuSection section, CategoryType categoryType, Uri sheetUri, string? description, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        var existingId = await GetIdOfAsync(name, section, categoryType, cancellationToken, command);
        if (existingId.HasValue)
            return await SingleAsync(existingId.Value, cancellationToken, command);

        Tcategory tcategory = new()
        {
            Name = name,
            Section = section,
            Type = categoryType,
            Url = sheetUri.ToString(),
            Description = description
        };
        command.CommandText = SqlSelectScript + Environment.NewLine + "WHERE Section = $Section COLLATE NOCASE AND Type = $Type AND Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)section);
        command.Parameters.AddWithValue("$Type", (byte)categoryType);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Insert

    /// <summary>
    /// Insert un nouvel enregistrement dans la table Tcategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'item ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url ne peut pas être vide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
            return new OperationState<int>(false, "L'url n'est pas valide");

        if (await ExistsAsync(Name, cancellationToken, cmd))
            return new OperationState<int>(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            INSERT INTO Tcategory
                (Name, Section, Type, Description, SheetId, Url)
            VALUES
                ($Name, $Section, $Type, $Description, $SheetId, $Url)
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", Name);
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$Type", (byte)Type);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", uri.ToString());

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

    public static async Task<OperationState> InsertOrReplaceAsync(IReadOnlyCollection<Tcategory> values, DbInsertMode insertMode = DbInsertMode.InsertOrReplace,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (values.Count == 0)
            return new OperationState(false, "La liste des valeurs ne peut pas être vide");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO Tcategory (Name, Section, Type, Description, SheetId, Url)";

        command.Parameters.Clear();

        for (uint i = 0; i < values.Count; i++)
        {
            var value = values.ElementAt((int)i);

            if (value.Name.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug($"Le nom de l'item ne peut pas être vide (id: {i}");
                continue;
            }

            if (value.Url.IsStringNullOrEmptyOrWhiteSpace() || !Uri.TryCreate(value.Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            {
                LogServices.LogDebug($"L'url ne peut pas être vide. (name: {values.ElementAt((int)i).Name}, id: {i}");
                continue;
            }

            command.CommandText += i == 0 ? "VALUES" : "," + Environment.NewLine;
            command.CommandText += $"($Name{i}, $Section{i}, $Type{i}, $Description{i}, $SheetId{i}, $Url{i})";

            command.Parameters.AddWithValue($"$Name{i}", value.Name.Trim());
            command.Parameters.AddWithValue($"$Section{i}", (byte)value.Section);
            command.Parameters.AddWithValue($"$Type{i}", (byte)value.Type);
            command.Parameters.AddWithValue($"$Description{i}", value.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue($"$SheetId{i}", value.SheetId);
            command.Parameters.AddWithValue($"$Url{i}", uri.ToString());

            LogServices.LogDebug("Ajout de l'item " + value.Name + " à la commande.");
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
    /// Retourne l'enregistrement de la table Tcategory ayant l'identifiant spécifié ou l'insert si l'enregistrement n'existe pas
    /// </summary>
    /// <param name="value"></param>
    /// <param name="reloadIfExist"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<Tcategory?> SingleOrCreateAsync(Tcategory value, bool reloadIfExist = false, CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.Parameters.Clear();
        if (!reloadIfExist)
        {
            var id = await GetIdOfAsync(value.Name, value.Section, value.Type, cancellationToken, command);
            if (id.HasValue)
            {
                value.Id = id.Value;
                return value;
            }
        }
        else
        {
            var record = await SingleAsync(value.Name, value.Section, value.Type, cancellationToken, command);
            if (record != null)
                return record;
        }

        var result = await value.InsertAsync(cancellationToken, command);
        return !result.IsSuccess ? null : value;
    }

    #endregion

    #region Update

    /// <summary>
    /// Met à jour cet enregistrement de la table Tcategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'item ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url ne peut pas être vide");

        var existingId = await GetIdOfAsync(Name, Type, cancellationToken, cmd);
        if (existingId.HasValue && existingId.Value != Id)
            return new OperationState(false, "Le nom de l'item existe déjà");

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            UPDATE Tcategory SET
                Name = $Name,
                Section = $Section,
                Type = $Type,
                Description = $Description,
                SheetId = $SheetId,
                Url = $Url
            WHERE Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$Name", Name.Trim());
        command.Parameters.AddWithValue("$Section", (byte)Section);
        command.Parameters.AddWithValue("$Type", (byte)Type);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url.Trim());

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

    #region Delete

    /// <summary>
    /// Supprime cet enregistrement de la table Tcategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    /// <summary>
    /// Supprime un enregistrement de la table Tcategory ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            DELETE FROM TanimeCategory WHERE IdCategory = $Id;
            DELETE FROM Tcategory WHERE Id = $Id
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            return await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0
                ? new OperationState(false, "Une erreur est survenue lors de la suppression")
                : new OperationState(true, "Suppression réussie");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression");
        }
    }

    public static async Task<OperationState> DeleteAllAsync(IcotakuSection section, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM Tcategory WHERE Section = $Section";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Section", (byte)section);

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

    /// <summary>
    /// Supprime tous les enregistrements de la table Tcategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteAllAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM Tcategory";

        command.Parameters.Clear();

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

    internal static Tcategory GetRecord(SqliteDataReader reader, int idIndex, int sectionIndex, int typeIndex, int nameIndex, int descriptionIndex,
        int sheetIdIndex, int urlIndex)
    {
        return new Tcategory()
        {
            Id = reader.GetInt32(idIndex),
            Name = reader.GetString(nameIndex),
            Type = (CategoryType)reader.GetByte(typeIndex),
            Section = (IcotakuSection)reader.GetByte(sectionIndex),
            Description = reader.IsDBNull(descriptionIndex) ? null : reader.GetString(descriptionIndex),
            SheetId = reader.GetInt32(sheetIdIndex),
            Url = reader.GetString(urlIndex)
        };
    }


    private static async IAsyncEnumerable<Tcategory> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("Id"),
                sectionIndex: reader.GetOrdinal("Section"),
                typeIndex: reader.GetOrdinal("Type"),
                nameIndex: reader.GetOrdinal("Name"),
                descriptionIndex: reader.GetOrdinal("Description"),
                sheetIdIndex: reader.GetOrdinal("SheetId"),
                urlIndex: reader.GetOrdinal("Url"));
        }
    }

    private const string SqlSelectScript =
        """
        SELECT
            Id,
            SheetId,
            Url,
            Name,
            Section,
            Type,
            Description
        FROM Tcategory
        """;
}