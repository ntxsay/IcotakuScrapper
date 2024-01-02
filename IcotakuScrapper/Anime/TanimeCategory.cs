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
    /// <returns></returns>
    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory";
        
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
    /// <returns></returns>
    public static async Task<int> CountAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory WHERE Id = $Id";

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
    /// <param name="idAnime"></param>
    /// <returns></returns>
    public static async Task<int> CountAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM TanimeCategory WHERE IdAnime = $IdAnime AND IdCategory = $IdCategory";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdCategory", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM TanimeCategory WHERE IdAnime = $IdAnime AND IdCategory = $IdCategory";

        command.Parameters.AddWithValue("$IdAnime", idAnime);
        command.Parameters.AddWithValue("$IdCategory", idContact);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, CancellationToken? cancellationToken = null)
        => await CountAsync(id, cancellationToken) > 0;

    public static async Task<bool> ExistsAsync(int idAnime, int idContact, CancellationToken? cancellationToken = null)
        => await CountAsync(idAnime, idContact, cancellationToken) > 0;

    #endregion

    #region Select

    public static async Task<TanimeCategory[]> SelectAsync(int id, AnimeCategoryIdSelector selector,
        OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + selector switch
        {
            AnimeCategoryIdSelector.Id => "WHERE TanimeCategory.Id = $Id",
            AnimeCategoryIdSelector.IdAnime => "WHERE TanimeCategory.IdAnime = $Id",
            AnimeCategoryIdSelector.IdCategory => "WHERE TanimeCategory.IdCategory = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(selector), selector, null)
        };

        command.CommandText += Environment.NewLine + $"ORDER BY Tcontact.DisplayName {orderBy}";

        command.AddLimitOffset(limit, skip);

        command.Parameters.AddWithValue("$Id", id);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];
        return await GetRecords(reader, cancellationToken).ToArrayAsync(cancellationToken ?? CancellationToken.None);
    }

    #endregion

    #region Single

    public static async Task<TanimeCategory?> SingleAsync(int idAnime, int idContact,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine +
                              "WHERE TanimeCategory.IdAnime = $IdAnime AND TanimeCategory.IdCategory = $IdCategory";

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

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState<int>(false, "L'anime n'existe pas.", 0);
        
        if (Category.Id <= 0 ||
            (!disableVerification && !await Tcategory.ExistsAsync(Category.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState<int>(false, "La catégorie n'existe pas.", 0);

        if (!disableVerification && await ExistsAsync(IdAnime, Category.Id, cancellationToken))
            return new OperationState<int>(false, "Le lien existe déjà.", 0);

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            INSERT INTO TanimeCategory 
                (IdAnime, IdCategory) 
            VALUES 
                ($IdAnime, $IdCategory);
            """;
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

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification = false, CancellationToken? cancellationToken = null)
    {
        if (IdAnime <= 0 || !await TanimeBase.ExistsAsync(IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");

        if (Category.Id <= 0 ||
            (!disableVerification && !await Tcategory.ExistsAsync(Category.Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "La catégorie n'existe pas.");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(IdAnime, Category.Id, cancellationToken);
            if (existingId is not null && existingId != Id)
                return new OperationState(false, "Le lien existe déjà.");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            UPDATE TanimeCategory 
            SET 
                IdAnime = $IdAnime, 
                IdCategory = $IdCategory 
            WHERE Id = $Id;
            """;

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
    
    #region InsertOrUpdate
    
    public static async Task<OperationState> InsertOrReplaceAsync(int idAnime, IReadOnlyCollection<int> idCategoryArray,
        DbInsertMode insertMode = DbInsertMode.InsertOrReplace, CancellationToken? cancellationToken = null)
    {
        if (idCategoryArray.Count == 0)
            return new OperationState(false, "Aucune catégorie n'a été trouvée.");

        if (idAnime <= 0 || !await TanimeBase.ExistsAsync(idAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");
        
        await using var command = Main.Connection.CreateCommand();
        command.StartWithInsertMode(insertMode);
        command.CommandText += " INTO TanimeCategory (IdAnime, IdCategory) VALUES";
        

        for (var i = 0; i < idCategoryArray.Count; i++)
        {
            var idCategory = idCategoryArray.ElementAt(i);
            if (idCategory <= 0)
            {
                LogServices.LogDebug($"Une des catégories sélectionnées n'existe pas ({idCategory}).");
                continue;
            }

            command.CommandText += Environment.NewLine + $"($IdAnime, $IdCategory{i})";
            command.Parameters.AddWithValue($"$IdCategory{i}", idCategory);

            if (i == idCategoryArray.Count - 1)
                command.CommandText += ";";
            else
                command.CommandText += ",";
        }

        if (command.Parameters.Count == 0)
            return new OperationState(false, "Aucune catégorie n'a été trouvée.");

        command.Parameters.AddWithValue("$IdAnime", idAnime);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(count > 0, $"{count} enregistrement(s) sur {idCategoryArray.Count} ont été insérés avec succès.");
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de l'insertion");
        }
    }
    
    public async Task<OperationState> AddOrUpdateAsync(CancellationToken? cancellationToken = null)
        => await AddOrUpdateAsync(this, cancellationToken);
    
    public static async Task<OperationState> AddOrUpdateAsync(TanimeCategory value,
        CancellationToken? cancellationToken = null)
    {
        //Si la validation échoue, on retourne le résultat de la validation
        if (!await TanimeBase.ExistsAsync(value.IdAnime, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "L'anime n'existe pas.");
        
        if (!await Tcategory.ExistsAsync(value.Category.Id, IntColumnSelect.Id, cancellationToken))
            return new OperationState(false, "La catégorie n'existe pas.");

        //Vérifie si l'item existe déjà
        var existingId = await GetIdOfAsync(value.IdAnime, value.Category.Id, cancellationToken);
        
        //Si l'item existe déjà
        if (existingId.HasValue)
        {
            //Si l'id n'appartient pas à l'item alors l'enregistrement existe déjà on annule l'opération
            if (value.Id > 0 && existingId.Value != value.Id)
                return new OperationState(false, "Le nom de l'item existe déjà");
            
            //Si l'id appartient à l'item alors on met à jour l'enregistrement
            if (existingId.Value != value.Id)
                value.Id = existingId.Value;
            return await value.UpdateAsync(true, cancellationToken);
        }

        //Si l'item n'existe pas, on l'ajoute
        var addResult = await value.InsertAsync(true, cancellationToken);
        if (addResult.IsSuccess)
            value.Id = addResult.Data;
        
        return addResult.ToBaseState();
    }
    #endregion

    #region Delete

    /// <summary>
    /// Supprime les enregistrements de la table TanimeWebSite qui ne sont pas dans la liste spécifiée
    /// </summary>
    /// <param name="actualValues">valeurs actuellement utilisées</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<OperationState> DeleteUnusedAsync(HashSet<(int idCategory, int idAnime)> actualValues, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeCategory WHERE IdCategory NOT IN (";

        var i = 0;
        foreach (var (idCategory, _) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdCategory{i}" : $", $IdCategory{i}";
            command.Parameters.AddWithValue($"$IdCategory{i}", idCategory);
            i++;
        }
        command.CommandText += ") AND IdAnime NOT IN (";
        i = 0;
        foreach (var (_, idAnime) in actualValues)
        {
            command.CommandText += i == 0 ? $"$IdAnime{i}" : $", $IdAnime{i}";
            command.Parameters.AddWithValue($"$IdAnime{i}", (byte)idAnime);
            i++;
        }
        command.CommandText += ")";

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
    
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "DELETE FROM TanimeCategory WHERE Id = $Id";


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
                categoryDescriptionIndex: reader.GetOrdinal("CategoryDescription"),
                categoryIsFullyScrapedIndex: reader.GetOrdinal("CategoryIsFullyScraped"));
        }
    }


    public static TanimeCategory GetRecord(SqliteDataReader reader, int idIndex, int idAnimeIndex,
        int idCategoryIndex, int sheetCategoryIdIndex, int urlCategoryIndex, int sectionIndex,
        int categoryTypeIndex, int categoryNameIndex, int categoryDescriptionIndex, int categoryIsFullyScrapedIndex)
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
                descriptionIndex: categoryDescriptionIndex,
                isFullyScrapedIndex: categoryIsFullyScrapedIndex)
        };

        return record;
    }

    private const string IcotakuSqlSelectScript =
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
            Tcategory.Description AS CategoryDescription,
            Tcategory.IsFullyScraped AS CategoryIsFullyScraped

        FROM TanimeCategory
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        """;
}