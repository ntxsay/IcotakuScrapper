using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Helpers;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper.Anime;

public class TanimeBase
{
    /// <summary>
    /// Obtient ou définit l'id de l'anime.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Obtient ou définit l'id de la fiche Icotaku de l'anime.
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>
    /// Obtient ou définit la note de l'anime sur 10.
    /// </summary>
    public double? Note { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre de votes de l'anime.
    /// </summary>
    public uint VoteCount { get; set; }

    /// <summary>
    /// Obtient ou définit une valeur indiquant si l'anime est réservé à un public adulte.
    /// </summary>
    public bool IsAdultContent { get; set; }

    /// <summary>
    /// Obtient ou définit une valeur indiquant si l'anime est réservé à un public averti.
    /// </summary>
    public bool IsExplicitContent { get; set; }

    /// <summary>
    /// Obtient ou définit le nom (principal) de l'anime.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtient ou définit l'état de diffusion de l'anime.
    /// </summary>
    public DiffusionStateKind DiffusionState { get; set; }

    /// <summary>
    /// Obtient ou définit le nombre d'épisodes de l'anime.
    /// </summary>
    public ushort EpisodesCount { get; set; }

    /// <summary>
    /// Obtient ou définit le format de l'anime (Série Tv, Oav).
    /// </summary>
    public Tformat? Format { get; set; }

    /// <summary>
    /// Obtient ou définit le public visé de l'anime.
    /// </summary>
    public Ttarget? Target { get; set; }

    /// <summary>
    /// Obtient ou définit l'origine de l'anime.
    /// </summary>
    public TorigineAdaptation? OrigineAdaptation { get; set; }

    public Tseason? Season { get; set; }

    /// <summary>
    /// Obtient ou définit la description de l'anime.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche de l'anime.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Obtient ou définit l'url de l'image miniature de l'anime.
    /// </summary>
    public string? ThumbnailMiniUrl { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de l'image de l'anime.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Obtient ou définit la liste des  catégories de l'anime (genre et thèmes).
    /// </summary>
    public HashSet<Tcategory> Categories { get; } = new();

    public TanimeBase()
    {
    }

    public TanimeBase(int id)
    {
        Id = id;
    }

    public override string ToString() => $"{Name} ({Id}/{SheetId})";

    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime";

        if (command.Parameters.Count > 0)
            command.Parameters.Clear();

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
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
            return 0;
        }
        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM Tanime WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM Tanime WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(string name, int sheetId, Uri sheetUri,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM Tanime WHERE Name = $Name COLLATE NOCASE OR Url = $Url COLLATE NOCASE OR SheetId = $SheetId";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$Name", name);
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tanime WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(int sheetId, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = "SELECT Id FROM Tanime WHERE SheetId = $SheetId";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(string name, int sheetId, Uri sheetUri,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            "SELECT Id FROM Tanime WHERE Name = $Name COLLATE NOCASE OR Url = $Url COLLATE NOCASE OR SheetId = $SheetId";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$Name", name);
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    #endregion

    #region Exists

    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(id, columnSelect, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(sheetUri, cancellationToken, cmd) > 0;

    public static async Task<bool> ExistsAsync(string name, int sheetId, Uri sheetUri,
        CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
        => await CountAsync(name, sheetId, sheetUri, cancellationToken, cmd) > 0;

    #endregion

    #region Single

    public static async Task<TanimeBase?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null,
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
            IntColumnSelect.Id => "WHERE Tanime.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tanime.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    public static async Task<TanimeBase?> SingleAsync(string name, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;

        command.CommandText += "WHERE Tanime.Name = $Name COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Name", name);

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    public static async Task<TanimeBase?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText = SqlSelectScript + Environment.NewLine;

        command.CommandText += "WHERE Tanime.Url = $Url COLLATE NOCASE";

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
    }

    #endregion

    #region Delete
    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        => await DeleteAsync(Id, SheetIntColumnSelect.Id, cancellationToken, cmd);

    public static async Task<OperationState> DeleteAsync(int id, SheetIntColumnSelect columnSelect, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();
        command.CommandText =
            """
            DELETE FROM TanimeAlternativeTitle WHERE IdAnime = $Id;
            DELETE FROM TanimeWebSite WHERE IdAnime = $Id;
            DELETE FROM TanimeStudio WHERE IdAnime = $Id;
            DELETE FROM TanimeLicense WHERE IdAnime = $Id;
            DELETE FROM TanimeStaff WHERE IdAnime = $Id;
            DELETE FROM TanimeCharacter WHERE IdAnime = $Id;
            DELETE FROM Tanime WHERE Id = $Id;
            """;

        command.Parameters.Clear();

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var count = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
            return new OperationState(true, $"{count} lignes supprimées");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return new OperationState(false, "Une erreur est survenue lors de la suppression de l'anime");
        }
    }

    public static async Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null,
        SqliteCommand? cmd = null)
    {
        await using var command = cmd ?? (await Main.GetSqliteConnectionAsync()).CreateCommand();

        var id = await GetIdOfAsync(uri, cancellationToken, command);
        if (!id.HasValue)
            return new OperationState(false, "L'anime n'a pas été trouvé");

        return await DeleteAsync(id.Value, SheetIntColumnSelect.Id, cancellationToken, command);
    }

    #endregion


    private static async Task<TanimeBase[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<TanimeBase> records = new();
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var idPlanning = reader.GetInt32(reader.GetOrdinal("BaseId"));
            var record = records.Find(f => f.Id == idPlanning);
            if (record == null)
            {
                record = new TanimeBase(reader.GetInt32(reader.GetOrdinal("IdAnime")))
                {
                    Name = reader.GetString(reader.GetOrdinal("AnimeName")),
                    Url = reader.GetString(reader.GetOrdinal("AnimeUrl")),
                    IsAdultContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsAdultContent")),
                    IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsExplicitContent")),
                    VoteCount = (uint)reader.GetInt32(reader.GetOrdinal("AnimeVoteCount")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("AnimeSheetId")),
                    DiffusionState = (DiffusionStateKind)reader.GetByte(reader.GetOrdinal("DiffusionState")),
                    EpisodesCount = (ushort)reader.GetInt16(reader.GetOrdinal("EpisodeCount")),
                    Note = reader.IsDBNull(reader.GetOrdinal("AnimeNote"))
                        ? null
                        : reader.GetDouble(reader.GetOrdinal("AnimeNote")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("AnimeThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeThumbnailUrl")),
                    Description = reader.IsDBNull(reader.GetOrdinal("AnimeDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeDescription")),
                    Format = reader.IsDBNull(reader.GetOrdinal("IdFormat"))
                        ? null
                        : Tformat.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdFormat"),
                            sectionIndex: reader.GetOrdinal("FormatSection"),
                            nameIndex: reader.GetOrdinal("FormatName"),
                            descriptionIndex: reader.GetOrdinal("FormatDescription")),
                    Target = reader.IsDBNull(reader.GetOrdinal("IdTarget"))
                        ? null
                        : Ttarget.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdTarget"),
                            sectionIndex: reader.GetOrdinal("TargetSection"),
                            nameIndex: reader.GetOrdinal("TargetName"),
                            descriptionIndex: reader.GetOrdinal("TargetDescription")),
                    OrigineAdaptation = reader.IsDBNull(reader.GetOrdinal("IdOrigine"))
                        ? null
                        : TorigineAdaptation.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdOrigine"),
                            sectionIndex: reader.GetOrdinal("OrigineAdaptationSection"),
                            nameIndex: reader.GetOrdinal("OrigineAdaptationName"),
                            descriptionIndex: reader.GetOrdinal("OrigineAdaptationDescription")),
                    Season = reader.IsDBNull(reader.GetOrdinal("IdSeason"))
                        ? null
                        : Tseason.GetRecord(reader,
                            idIndex: reader.GetOrdinal("IdSeason"),
                            displayNameIndex: reader.GetOrdinal("SeasonDisplayName"),
                            seasonNumberIndex: reader.GetOrdinal("SeasonNumber")),
                };


                records.Add(record);
            }

            if (!reader.IsDBNull(reader.GetOrdinal("CategoryId")))
            {
                var categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                var category = record.Categories.FirstOrDefault(x => x.Id == categoryId);
                if (category == null)
                {
                    category = Tcategory.GetRecord(reader,
                        idIndex: reader.GetOrdinal("CategoryId"),
                        sheetIdIndex: reader.GetOrdinal("CategorySheetId"),
                        typeIndex: reader.GetOrdinal("CategoryType"),
                        urlIndex: reader.GetOrdinal("CategoryUrl"),
                        sectionIndex: reader.GetOrdinal("CategorySection"),
                        nameIndex: reader.GetOrdinal("CategoryName"),
                        descriptionIndex: reader.GetOrdinal("CategoryDescription"));
                    record.Categories.Add(category);
                }
            }
        }

        return records.ToArray();
    }

    private const string SqlSelectScript =
        """
        SELECT
            Tanime.Id AS BaseId,
            Tanime.Name AS AnimeName,
            Tanime.IsAdultContent AS AnimeIsAdultContent,
            Tanime.IsExplicitContent AS AnimeIsExplicitContent,
            Tanime.Note AS AnimeNote,
            Tanime.VoteCount AS AnimeVoteCount,
            Tanime.Url AS AnimeUrl,
            Tanime.SheetId AS AnimeSheetId,
            Tanime.DiffusionState,
            Tanime.ThumbnailUrl AS AnimeThumbnailUrl,
            Tanime.Description AS AnimeDescription,
            
            Tformat.Name as FormatName,
            Tformat.Section as FormatSection,
            Tformat.Description as FormatDescription,
            
            Ttarget.Name as TargetName,
            Ttarget.Section as TargetSection,
            Ttarget.Description as TargetDescription,
            
            TorigineAdaptation.Name as OrigineAdaptationName,
            TorigineAdaptation.Section as OrigineAdaptationSection,
            TorigineAdaptation.Description as OrigineAdaptationDescription,
            
            Tseason.DisplayName as SeasonDisplayName,
            Tseason.SeasonNumber as SeasonNumber,
            
            TanimeCategory.IdCategory AS CategoryId,
            Tcategory.SheetId AS CategorySheetId,
            Tcategory.Type AS CategoryType,
            Tcategory.Url AS CategoryUrl,
            Tcategory.Section AS CategorySection,
            Tcategory.Name AS CategoryName,
            Tcategory.Description AS CategoryDescription

        FROM
            Tanime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory


        """;
}