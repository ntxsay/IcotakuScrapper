using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace IcotakuScrapper.Anime;

public enum AnimeCategoryIdSelector
{
    Id,
    IdAnime,
    IdCategory,
}

public class TanimeCategory
{
    public int Id { get; protected set; }
    public int IdAnime { get; set; }
    public Tcategory Category { get; set; } = new();

    public TanimeCategory()
    {
    }

    public TanimeCategory(int id)
    {
        Id = id;
    }

    public TanimeCategory(int idAnime, Tcategory category)
    {
        IdAnime = idAnime;
        Category = category;
    }


    #region Count

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeCategory
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeCategory ayant l'identifiant spécifié
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    /// <summary>
    /// Compte le nombre d'entrées dans la table TanimeCategory ayant le nom spécifié
    /// </summary>
    /// <param name="idContact"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="cmd"></param>
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory WHERE IdAnime = $IdAnime AND IdCategory = $IdCategory";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdCategory", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeCategory WHERE IdAnime = $IdAnime AND IdCategory = $IdCategory";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdCategory", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(idAnime, idContact, cancellationToken, cmd) > 0;

    #endregion

    #region Select

    public static async Task<TanimeCategory[]> SelectAsync(int id, AnimeCategoryIdSelector selector,
        OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine + selector switch
        {
            AnimeCategoryIdSelector.Id => "WHERE TanimeCategory.Id = $Id",
            AnimeCategoryIdSelector.IdAnime => "WHERE TanimeCategory.IdAnime = $Id",
            AnimeCategoryIdSelector.IdCategory => "WHERE TanimeCategory.IdCategory = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
        };

        command.CommandText += Environment.NewLine + $"ORDER BY Tcontact.DisplayName {orderBy}";

        command.AddLimitOffset(limit, skip);

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return Array.Empty<TanimeCategory>();
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeCategory?> SingleAsync(int idAnime, int idContact,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine +
                              "WHERE TanimeCategory.IdAnime = $IdAnime AND TanimeCategory.IdCategory = $IdCategory";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdCategory", idContact);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;
        return await GetRecords(reader, cancellationToken)
            .FirstOrDefaultAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion


    #region Insert

    public async Task<OperationState<int>> InsertAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (IdAnime <= 0 || !await Tanime.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "L'anime n'existe pas.", 0);


        if (Category.Id <= 0 ||
            !await Tcategory.ExistsAsync(Category.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le studio n'existe pas.", 0);

        if (await ExistsAsync(IdAnime, Category.Id, cancellationToken, command))
            return new OperationState<int>(false, "Le lien existe déjà.", 0);

        command.CommandText = "INSERT INTO TanimeCategory (IdAnime, IdCategory) VALUES ($IdAnime, $IdCategory);";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdCategory", Category.Id);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");

            Id = await command.GetLastInsertRowIdAsync();
            return new OperationState<int>(true, "Insertion réussie", Id);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    public static async Task<OperationState> InsertAsync(int idAnime, IReadOnlyCollection<int> idCategoryValues,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        if (idAnime <= 0 || !await Tanime.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");

        if (idCategoryValues.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        List<OperationState<int>> results = [];
        command.CommandText = "INSERT OR IGNORE INTO TanimeCategory (IdAnime, IdCategory) VALUES";
        command.Parameters.Clear();

        for (var i = 0; i < idCategoryValues.Count; i++)
        {
            var idCategory = idCategoryValues.ElementAt(i);
            if (idCategory <= 0)
            {
                results.Add(new OperationState<int>(false, $"Une des catégories sélectionnées n'existe pas ({idCategory})."));
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $IdCategory{i})";
            command.Parameters.AddWithValue($"$IdCategory{i}", idCategory);

            if (i == idCategoryValues.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucun studio n'a été sélectionné.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState(false, "Une erreur est survenue lors de l'insertion");

            if (results.All(a => a.IsSuccess))
                return new OperationState(true, "Insertion réussie");
            if (results.All(a => !a.IsSuccess))
                return new OperationState(false, "Aucun studio n'a été inséré");
            return new OperationState(true, "Certains studios n'ont pas été insérés");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "L'anime n'existe pas.");

        if (Category.Id <= 0 ||
            !await Tcategory.ExistsAsync(Category.Id, IntColumnSelect.Id, cancellationToken, command))
            return new OperationState(false, "Le studio n'existe pas.");

        var existingId = await GetIdOfAsync(IdAnime, Category.Id, cancellationToken, command);
        if (existingId is not null && existingId != Id)
            return new OperationState(false, "Le lien existe déjà.");

        command.CommandText = "UPDATE TanimeCategory SET IdAnime = $IdAnime, IdCategory = $IdCategory WHERE Id = $Id;";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdAnime", IdAnime);
        command.Parameters.AddWithValue("$IdCategory", Category.Id);

        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState(false, "Une erreur est survenue lors de la mise à jour");

            return new OperationState(true, "Mise à jour réussie");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour");
        }
    }

    #endregion

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await DeleteAsync(Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "DELETE FROM TanimeCategory WHERE Id = $Id";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return result > 0
                ? new OperationState(true, "Le lien a été supprimé avec succès")
                : new OperationState(false, "Une erreur est survenue lors de la suppression du lien");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la suppression du lien");
        }
    }

    #endregion

    private static async IAsyncEnumerable<TanimeCategory> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            yield return GetRecord(reader,
                idIndex: reader.GetOrdinal("BaseId"),
                idAnimeIndex: reader.GetOrdinal("IdAnime"),
                idCategoryIndex: reader.GetOrdinal("IdCategory"),
                categoryTypeIndex: reader.GetOrdinal("CategoryType"),
                sheetCategoryIdIndex: reader.GetOrdinal("CategorySheetId"),
                sectionIndex: reader.GetOrdinal("CategorySection"),
                urlCategoryIndex: reader.GetOrdinal("CategoryUrl"),
                categoryNameIndex: reader.GetOrdinal("CategoryName"),
                categoryDescriptionIndex: reader.GetOrdinal("CategoryDescription"));
        }
    }


    public static TanimeCategory GetRecord(SqliteDataReader reader, int idIndex, int idAnimeIndex,
        int idCategoryIndex, int sheetCategoryIdIndex, int urlCategoryIndex, int sectionIndex,
        int categoryTypeIndex, int categoryNameIndex, int categoryDescriptionIndex)
    {
        var record = new TanimeCategory()
        {
            Id = reader.GetInt32(idIndex),
            IdAnime = reader.GetInt32(idAnimeIndex),
            Category = Tcategory.GetRecord(reader,
                idIndex: idCategoryIndex,
                sheetIdIndex: sheetCategoryIdIndex,
                urlIndex: urlCategoryIndex,
                sectionIndex: sectionIndex,
                typeIndex: categoryTypeIndex,
                nameIndex: categoryNameIndex,
                descriptionIndex: categoryDescriptionIndex)
        };

        return record;
    }

    private const string SqlSelectScript =
        """
        SELECT
            TanimeCategory.Id AS BaseId,
            TanimeCategory.IdAnime,
            TanimeCategory.IdCategory,
            
            Tcategory.Type AS CategoryType,
            Tcategory.SheetId AS CategorySheetId,
            Tcategory.Url AS CategoryUrl,
            Tcategory.Section AS CategorySection,
            Tcategory.Name AS CategoryName,
            Tcategory.Description AS CategoryDescription

        FROM TanimeCategory
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        """;
}