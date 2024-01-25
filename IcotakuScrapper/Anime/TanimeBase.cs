using System.Globalization;
using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Services.IOS;
using Microsoft.Data.Sqlite;
using IcotakuScrapper.Contact;
using IcotakuScrapper.Objects;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase : ITableSheetBase<TanimeBase>
{
    /// <summary>
    /// Obtient ou définit l'id de l'anime.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Obtient ou définit le guid de l'anime.
    /// </summary>
    public Guid Guid { get; protected set; } = Guid.Empty;
    
    /// <summary>
    /// Obtient ou définit l'id de la fiche Icotaku de l'anime.
    /// </summary>
    public int SheetId { get; set; }

    /// <summary>
    /// Obtient ou définit la date de sortie de l'anime au format yyyy-MM-dd.
    /// </summary>
    public string? ReleaseDate { get; set; }
    
    public MonthDate ReleaseMonth { get; set; }

    public DateOnly? ReleaseDateAsDateOnly => GetReleaseDate();
    public string? ReleaseDateAsLiteral => ReleaseDateAsDateOnly?.ToString("dddd dd MMMM yyyy");
    public string? MinimalReleaseMonthLiteral => ReleaseDateAsDateOnly?.ToString("MMM yyyy");


    /// <summary>
    /// Obtient ou définit la date de fin de l'anime au format yyyy-MM-dd.
    /// </summary>
    public string? EndDate { get; set; }

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
    /// Obtient ou définit la durée d'un épisode de l'anime (en minutes).
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

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
    
    public TsheetStatistic? Statistic { get; set; }
    public TuserSheetNotation? UserNotation { get; set; }


    /// <summary>
    /// Obtient ou définit la description de l'anime.
    /// </summary>
    public string? Description { get; set; }
    
    public string? Remark { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche de l'anime.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Obtient ou définit l'url de l'image de l'anime.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    public string? ThumbnailPath => GetThumbnailPath();

    /// <summary>
    /// Obtient ou définit la liste des  catégories de l'anime (genre et thèmes).
    /// </summary>
    public HashSet<Tcategory> Categories { get; } = [];
    
    /// <summary>
    /// Obtient ou définit la liste des titres alternatifs de l'anime.
    /// </summary>
    public HashSet<TanimeAlternativeTitle> AlternativeTitles { get; } = [];
    
    /// <summary>
    /// Obtient ou définit la liste des sites web de l'anime.
    /// </summary>
    public HashSet<TanimeWebSite> Websites { get; } = [];
    
    public HashSet<TanimeLicense> Licenses { get; } = [];
    /// <summary>
    /// Obtient ou définit la liste des studios de l'anime.
    /// </summary>
    public HashSet<TcontactBase> Studios { get; } = [];
    public HashSet<TanimeStaff> Staffs { get; } = [];

    public TanimeBase()
    {
    }

    public TanimeBase(int id)
    {
        Id = id;
    }

    public TanimeBase(int id, Guid guid)
    {
        Id = id;
        Guid = guid;
    }

    public void Copy(TanimeBase value)
    {
        Id = value.Id;
        Guid = value.Guid;
        SheetId = value.SheetId;
        ReleaseMonth = value.ReleaseMonth;
        ReleaseDate = value.ReleaseDate;
        EndDate = value.EndDate;
        Note = value.Note;
        VoteCount = value.VoteCount;
        IsAdultContent = value.IsAdultContent;
        IsExplicitContent = value.IsExplicitContent;
        Name = value.Name;
        DiffusionState = value.DiffusionState;
        EpisodesCount = value.EpisodesCount;
        Duration = value.Duration;
        Statistic = value.Statistic?.Clone() ?? null;
        UserNotation = value.UserNotation?.Clone() ?? null;
        Format = value.Format;
        Target = value.Target;
        OrigineAdaptation = value.OrigineAdaptation;
        Season = value.Season;
        Description = value.Description;
        Remark = value.Remark;
        Url = value.Url;
        ThumbnailUrl = value.ThumbnailUrl;
        Categories.ToObservableCollection(value.Categories, true);
        AlternativeTitles.ToObservableCollection(value.AlternativeTitles, true);
        Websites.ToObservableCollection(value.Websites, true);
        Licenses.ToObservableCollection(value.Licenses, true);
        Studios.ToObservableCollection(value.Studios, true);
        Staffs.ToObservableCollection(value.Staffs, true);
        
    }
    
    public TanimeBase Clone()
    {
        var clone = new TanimeBase();
        clone.Copy(this);
        return clone;
    }

    /// <summary>
    /// Converti l'objet <see cref="TanimeBase"/> en <see cref="Tanime"/>.
    /// </summary>
    /// <returns></returns>
    public Tanime ToFullAnime()
    {
        var value = new Tanime
        {
            Id = Id,
            Guid = Guid,
            Name = Name,
            Url = Url,
            IsAdultContent = IsAdultContent,
            IsExplicitContent = IsExplicitContent,
            VoteCount = VoteCount,
            SheetId = SheetId,
            Duration = Duration,
            DiffusionState = DiffusionState,
            ReleaseDate = ReleaseDate,
            ReleaseMonth = ReleaseMonth,
            EndDate = EndDate,
            EpisodesCount = EpisodesCount,
            Note = Note,
            Description = Description,
            Remark = Remark,
            ThumbnailUrl = ThumbnailUrl,
            Format = Format?.Clone(),
            Target = Target?.Clone(),
            OrigineAdaptation = OrigineAdaptation?.Clone(),
            Season = Season,
            Statistic = Statistic?.Clone()
        };
        
        value.AlternativeTitles.ToObservableCollection(AlternativeTitles, true);
        value.Websites.ToObservableCollection(Websites, true);
        value.Categories.ToObservableCollection(Categories, true);
        value.Studios.ToObservableCollection(Studios, true);
        value.Licenses.ToObservableCollection(Licenses, true);
        value.Staffs.ToObservableCollection(Staffs, true);

        return value;
    }
    public override string ToString() => $"{Name} ({Id}/{SheetId})";

    #region Folder et download

    /// <summary>
    /// Retourne la date de sortie de l'anime via l'objet <see cref="DateOnly"/>.
    /// </summary>
    /// <returns></returns>
    public DateOnly? GetReleaseDate()
    {
        if (ReleaseDate == null || ReleaseDate.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        var date = ReleaseDate.Split('-');
        if (date.Length != 3)
            return null;

        if (!ushort.TryParse(date[0], out var year))
            return null;

        if (!ushort.TryParse(date[1], out var month))
            return null;

        if (!ushort.TryParse(date[2], out var day))
            return null;

        if (DateOnly.TryParse($"{year}-{month}-{day}", CultureInfo.DefaultThreadCurrentCulture, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Télécharge le dossier complet de la fiche comprendant les fichiers et les sous dossiers
    /// </summary>
    /// <remarks>Ce dossier comprend généralement les vignettes de la fiche et les captures d'écran des épisodes.</remarks>
    /// <returns></returns>
    public async Task<bool> DownloadFolderAsync(CancellationToken? cancellationToken = null)
    {
        return await DownloadFolderAsync(Guid, SheetId, cancellationToken ?? CancellationToken.None);
    }

    public static async Task<bool> DownloadFolderAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var itemGuid = await GetGuidAsync(sheetUri, cancellationToken ?? CancellationToken.None);
        if (itemGuid == Guid.Empty)
            return false;

        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
        if (sheetId == 0)
            return false;

        return await IcotakuWebHelpers.DownloadFullSheetFolderAsync(IcotakuSheetType.Anime, sheetId, itemGuid,
            cancellationToken ?? CancellationToken.None);
    }

    public static async Task<bool> DownloadFolderAsync(Guid itemGuid, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        return await IcotakuWebHelpers.DownloadFullSheetFolderAsync(IcotakuSheetType.Anime, sheetId, itemGuid,
            cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Retourne le chemin d'accès du dossier de la fiche anime.
    /// </summary>
    /// <returns></returns>
    public string? GetFolderPath()
    {
        return GetFolderPath(Guid);
    }

    public static async Task<string?> GetFolderPathAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var itemGuid = await GetGuidAsync(sheetUri, cancellationToken ?? CancellationToken.None);
        if (itemGuid == Guid.Empty)
            return null;

        return GetFolderPath(itemGuid);
    }

    public static string? GetFolderPath(Guid itemGuid)
    {
        return !InputOutput.IsDirectoryExists(IcotakuDefaultFolder.Animes, itemGuid)
            ? null
            : InputOutput.GetDirectoryPath(IcotakuDefaultFolder.Animes, itemGuid);
    }

    #endregion

    #region Thumbnail operations

    /// <summary>
    /// Télécharge l'affiche de l'anime et retourne le chemin d'accès vers l'affiche.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string?> GetOrDownloadThumbnailAsync(CancellationToken? cancellationToken = null)
    {
        var thumbnailPath = GetThumbnailPath();
        if (thumbnailPath != null)
            return thumbnailPath;

        thumbnailPath = await DownloadThumbnailAsync(cancellationToken);
        return thumbnailPath ?? null;
    }

    /// <summary>
    /// Retourne le chemin d'accès vers l'affiche de l'anime.
    /// </summary>
    /// <returns></returns>
    public string? GetThumbnailPath()
    {
        return GetThumbnailPath(Guid);
    }

    public async Task<string?> DownloadThumbnailAsync(CancellationToken? cancellationToken = null)
    {
        if (ThumbnailUrl == null || ThumbnailUrl.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (!Uri.TryCreate(ThumbnailUrl, UriKind.Absolute, out var uri))
            return null;

        return await DownloadThumbnailAsync(Guid, uri, cancellationToken ?? CancellationToken.None);
    }

    public static async Task<string?> GetOrDownloadThumbnailAsync(Uri sheetUri,
        CancellationToken? cancellationToken = null)
    {
        var itemGuid = await GetGuidAsync(sheetUri, cancellationToken ?? CancellationToken.None);
        if (itemGuid == Guid.Empty)
            return null;

        var thumbnailPath = GetThumbnailPath(itemGuid);
        if (thumbnailPath != null)
            return thumbnailPath;

        thumbnailPath = await DownloadThumbnailAsync(sheetUri, cancellationToken);
        return thumbnailPath ?? null;
    }

    /// <summary>
    /// Retourne le chemin d'accès vers l'affiche de l'anime.
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string?> DownloadThumbnailAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                Guid,
                ThumbnailUrl
            FROM Tanime
            WHERE Url = $Url COLLATE NOCASE
            """;

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return null;

        await reader.ReadAsync(cancellationToken ?? CancellationToken.None);
        var itemGuid = reader.GetGuid(reader.GetOrdinal("Guid"));
        var thumbnailUrl = reader.IsDBNull(reader.GetOrdinal("ThumbnailUrl"))
            ? null
            : reader.GetString(reader.GetOrdinal("ThumbnailUrl"));

        if (itemGuid == Guid.Empty || thumbnailUrl == null || thumbnailUrl.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        return await DownloadThumbnailAsync(itemGuid, new Uri(thumbnailUrl),
            cancellationToken ?? CancellationToken.None);
    }

    public static async Task<string?> DownloadThumbnailAsync(Guid itemGuid, Uri thumbnailUri,
        CancellationToken? cancellationToken = null)
    {
        return await IcotakuWebHelpers.DownloadThumbnailAsync(IcotakuSheetType.Anime, itemGuid, thumbnailUri, false,
            cancellationToken ?? CancellationToken.None);
    }

    /// <summary>
    /// Retourne le chemin d'accès vers l'affiche de l'anime.
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string?> GetThumbnailPathAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        var itemGuid = await GetGuidAsync(sheetUri, cancellationToken ?? CancellationToken.None);
        return itemGuid == Guid.Empty
            ? null
            : GetThumbnailPath(itemGuid);
    }

    /// <summary>
    /// Retourne le chemin d'accès vers l'affiche de l'anime.
    /// </summary>
    /// <param name="itemGuid"></param>
    /// <returns></returns>
    public static string? GetThumbnailPath(Guid itemGuid)
    {
        //Retourne le chemin d'accès vers le dossier de l'affiche de l'anime
        var folderPath =
            InputOutput.GetDirectoryPath(IcotakuDefaultFolder.Animes, itemGuid, IcotakuDefaultSubFolder.Sheet);
        
        //Si le dossier n'existe pas, on retourne null
        if (folderPath == null || folderPath.IsStringNullOrEmptyOrWhiteSpace() || !Directory.Exists(folderPath))
            return null;

        //Retourne le chemin d'accès vers l'affiche origniale de l'anime
        var path = Directory.EnumerateFiles(folderPath, "affiche_*", SearchOption.TopDirectoryOnly)
            .FirstOrDefault(f =>
                !Path.GetFileNameWithoutExtension(f).Contains("mini", StringComparison.OrdinalIgnoreCase));

        return path;
    }

    #endregion

    #region Index

    /// <summary>
    /// Insère ou met à jour l'index de l'anime dans la base de données.
    /// </summary>
    /// <param name="animeName"></param>
    /// <param name="animeUri"></param>
    /// <param name="animeSheetId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<OperationState<int>> CreateOrUpdateIndexAsync(string animeName, Uri animeUri, int animeSheetId, CancellationToken? cancellationToken = null)
    {
        var record = await TsheetIndex.SingleAsync(animeUri, cancellationToken) ?? new TsheetIndex();

        if (!string.Equals(record.SheetName, animeName, StringComparison.OrdinalIgnoreCase))
            record.SheetName = animeName;

        if (record.SheetId != animeSheetId)
            record.SheetId = animeSheetId;
        
        return await record.AddOrUpdateAsync(cancellationToken);
    }

    #endregion
    
    #region Count

    public static async Task<int> CountAsync(CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime";

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    static Task<int> ITableBase<TanimeBase>.CountAsync(int id, CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }


    public static async Task<int> CountAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
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
            IntColumnSelect.Id => "SELECT COUNT(Id) FROM Tanime WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT COUNT(Id) FROM Tanime WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        
        command.Parameters.AddWithValue("$Id", id);
        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    private static async Task<int> CountAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return 0;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime WHERE Name = $Name COLLATE NOCASE";

        command.Parameters.AddWithValue("$Name", name);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    private static async Task<int> CountAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT COUNT(Id) FROM Tanime WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int> CountAsync(Uri sheetUri, int sheetId,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(Id) FROM Tanime WHERE Url = $Url COLLATE NOCASE OR SheetId = $SheetId";
        
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return 0;
    }

    public static async Task<int?> GetIdOfAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Tanime WHERE Url = $Url COLLATE NOCASE";

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(int sheetId, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Tanime WHERE SheetId = $SheetId";

        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }

    public static async Task<int?> GetIdOfAsync(Uri sheetUri, int sheetId, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Tanime WHERE Url = $Url COLLATE NOCASE OR SheetId = $SheetId";

        command.Parameters.AddWithValue("$Url", sheetUri.ToString());
        command.Parameters.AddWithValue("$SheetId", sheetId);

        var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
        if (result is long count)
            return (int)count;
        return null;
    }
    
    #endregion

    #region Select

    /// <summary>
    /// Retourne la liste des animés de la base de données.
    /// </summary>
    /// <param name="arrayId">Identifiants des animés à retourner</param>
    /// <param name="isAdultContent"></param>
    /// <param name="isExplicitContent"></param>
    /// <param name="sortBy"></param>
    /// <param name="orderBy"></param>
    /// <param name="limit"></param>
    /// <param name="skip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TanimeBase[]> SelectAsync(HashSet<int> arrayId, bool? isAdultContent = false, bool? isExplicitContent = false, AnimeSortBy sortBy = AnimeSortBy.Name, OrderBy orderBy = OrderBy.Asc, uint limit = 0, uint skip = 0, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript;

        command.CommandText += Environment.NewLine + $"WHERE Tanime.Id IN ({string.Join(',', arrayId)})";
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent", isAdultContent, isExplicitContent);

        command.AddOrderSort(sortBy, orderBy);

        command.AddLimitOffset(limit, skip);

        var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        if (!reader.HasRows)
            return [];

        return await GetRecords(reader, cancellationToken);
    }

    #endregion

    #region Exists

    static Task<bool> ITableBase<TanimeBase>.ExistsAsync(int id, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }
    
    public static async Task<bool> ExistsByIdAsync(int id,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, IntColumnSelect.Id, cancellationToken) > 0;

    public static async Task<bool> ExistsBySheetIdAsync(int sheetId,
        CancellationToken? cancellationToken = null)
        => await CountAsync(sheetId, IntColumnSelect.SheetId, cancellationToken) > 0;
    
    public static async Task<bool> ExistsAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
        => await CountAsync(id, columnSelect, cancellationToken) > 0;

    /// <summary>
    /// Vérifie si un animé existe dans la base de données.
    /// </summary>
    /// <param name="name">Nom de l'animé</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(string name, CancellationToken? cancellationToken = null)
        => await CountAsync(name, cancellationToken) > 0;

    /// <summary>
    /// Vérifie si un animé existe dans la base de données.
    /// </summary>
    /// <param name="sheetUri">Url complète de la fiche Icotaku de l'animé</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
        => await CountAsync(sheetUri, cancellationToken) > 0;

    /// <summary>
    /// Vériie si un animé existe dans la base de données.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sheetId"></param>
    /// <param name="sheetUri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> ExistsAsync(Uri sheetUri, int sheetId,
        CancellationToken? cancellationToken = null)
        => await CountAsync(sheetUri, sheetId, cancellationToken) > 0;

    #endregion

    #region Single

    public static async Task<TanimeBase?> SingleByIdAsync(int id, CancellationToken? cancellationToken = null)
        => await SingleAsync(id, IntColumnSelect.Id, cancellationToken);

    public static async Task<TanimeBase?> SingleBySheetIdAsync(int sheetId, CancellationToken? cancellationToken = null)
        => await SingleAsync(sheetId, IntColumnSelect.SheetId, cancellationToken);
    
    public static async Task<TanimeBase?> SingleAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
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

        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + columnSelect switch
        {
            IntColumnSelect.Id => "WHERE Tanime.Id = $Id",
            IntColumnSelect.SheetId => "WHERE Tanime.SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent",
            null, null);

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).FirstOrDefault();
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    public static async Task<TanimeBase?> SingleAsync(string name, CancellationToken? cancellationToken = null)
    {
        if (name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Tanime.Name = $Name COLLATE NOCASE";
        
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent",
            null, null);
        command.Parameters.AddWithValue("$Name", name);

        try
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).SingleOrDefault();
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    public static async Task<TanimeBase?> SingleAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = IcotakuSqlSelectScript + Environment.NewLine + "WHERE Tanime.Url = $Url COLLATE NOCASE";
        
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent",
            null, null);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        try
        {
            var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
            return !reader.HasRows ? null : (await GetRecords(reader, cancellationToken)).SingleOrDefault();
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return null;
        }
    }

    public static async Task<Guid> GetGuidAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        var isColumnSelectValid = command.IsIntColumnValidated(columnSelect,
        [
            IntColumnSelect.Id,
            IntColumnSelect.SheetId,
        ]);

        if (!isColumnSelectValid)
        {
            return Guid.Empty;
        }

        command.CommandText = columnSelect switch
        {
            IntColumnSelect.Id => "SELECT Guid FROM Tanime WHERE Id = $Id",
            IntColumnSelect.SheetId => "SELECT Guid FROM Tanime WHERE SheetId = $Id",
            _ => throw new ArgumentOutOfRangeException(nameof(columnSelect), columnSelect, null)
        };
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent",
            null, null);

        command.Parameters.AddWithValue("$Id", id);

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is string stringGuid)
                return Guid.Parse(stringGuid);
            return Guid.Empty;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return Guid.Empty;
        }
    }

    public static async Task<Guid> GetGuidAsync(Uri sheetUri, CancellationToken? cancellationToken = null)
    {
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = "SELECT Guid FROM Tanime WHERE Url = $Url COLLATE NOCASE";
        
        command.AddExplicitContentFilter(DbStartFilterMode.And, "Tanime.IsAdultContent", "Tanime.IsExplicitContent",
            null, null);
        command.Parameters.AddWithValue("$Url", sheetUri.ToString());

        try
        {
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is string stringGuid)
                return Guid.Parse(stringGuid);
            return Guid.Empty;
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e);
            return Guid.Empty;
        }
    }

    #endregion

    #region Insert

    public async Task<OperationState<int>> InsertAsync(bool disableVerification = false,
        CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "Le nom de l'anime ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState<int>(false, "L'url de la fiche de l'anime ne peut pas être vide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState<int>(false, "L'url de la fiche de l'anime n'est pas valide");

        if (SheetId <= 0)
            return new OperationState<int>(false, "L'id de la fiche icotaku n'est pas valide");

        if (!disableVerification && await ExistsAsync(uri, SheetId, cancellationToken))
            return new OperationState<int>(false, "L'anime existe déjà");


        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            INSERT OR REPLACE INTO Tanime
                (SheetId, Url, IsAdultContent, IsExplicitContent, Note, VoteCount, Name, DiffusionState, EpisodeCount, EpisodeDuration, ReleaseDate, ReleaseMonth , EndDate, Description, ThumbnailUrl, IdFormat, IdTarget, IdOrigine, IdSeason, Remark, IdStatistic)
            VALUES
                ($SheetId, $Url, $IsAdultContent, $IsExplicitContent, $Note, $VoteCount, $Name, $DiffusionState , $EpisodeCount, $EpisodeDuration, $ReleaseDate, $ReleaseMonth, $EndDate, $Description, $ThumbnailUrl, $IdFormat, $IdTarget, $IdOrigine, $IdSeason, $Remark, $IdStatistic)
            """;

        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$Note", Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$VoteCount", VoteCount);
        command.Parameters.AddWithValue("$Name", Name);
        command.Parameters.AddWithValue("$DiffusionState", (byte)DiffusionState);
        command.Parameters.AddWithValue("$EpisodeCount", EpisodesCount);
        command.Parameters.AddWithValue("$EpisodeDuration", Duration.TotalMinutes);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ReleaseMonth", ReleaseMonth.ToNumberedDate());
        command.Parameters.AddWithValue("$EndDate", EndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdFormat", Format?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdTarget", Target?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdSeason", Season?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Remark", Remark ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdStatistic", Statistic is { Id: > 0 } ? Statistic.Id : DBNull.Value);
        
        try
        {
            if (await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None) <= 0)
                return new OperationState<int>(false, "L'anime n'a pas été ajouté");
            
            Id = await command.GetLastInsertRowIdAsync();
            
            _ = await CreateOrUpdateIndexAsync(Name, uri, SheetId, cancellationToken);
            
            await this.AddOrReplaceAlternativeTitlesAsync(cancellationToken);
            await this.AddOrReplaceWebsitesAsync(cancellationToken);
            await this.AddOrReplaceStudiosAsync(cancellationToken);
            await this.AddOrReplaceCategoriesAsync(cancellationToken);
            await this.AddOrReplaceLicensesAsync(cancellationToken);
            await this.AddOrReplaceStaffsAsync(cancellationToken);


            return new OperationState<int>(true, "L'anime a été ajouté avec succès", Id);
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState<int>(false, "Une erreur est survenue lors de l'ajout de l'anime");
        }
    }

    #endregion

    #region Update

    public async Task<OperationState> UpdateAsync(bool disableVerification = false,
        CancellationToken? cancellationToken = null)
    {
        if (Name.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "Le nom de l'anime ne peut pas être vide");

        if (Url.IsStringNullOrEmptyOrWhiteSpace())
            return new OperationState(false, "L'url de la fiche de l'anime ne peut pas être vide");

        if (SheetId <= 0)
            return new OperationState(false, "L'id de la fiche de l'anime Icotaku n'est pas valide");

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri) || !uri.IsAbsoluteUri)
            return new OperationState(false, "L'url de la fiche de l'anime n'est pas valide");

        if (Id <= 0 || (!disableVerification &&
                        !await ExistsAsync(Id, IntColumnSelect.Id, cancellationToken)))
            return new OperationState(false, "L'id de l'anime ne peut pas être inférieur ou égal à 0");

        if (!disableVerification)
        {
            var existingId = await GetIdOfAsync(uri, SheetId, cancellationToken);
            if (existingId.HasValue && existingId.Value != Id)
                return new OperationState(false, "L'url de la fiche de l'anime existe déjà");
        }

        await using var command = Main.Connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Tanime SET
                SheetId = $SheetId,
                Url = $Url,
                IsAdultContent = $IsAdultContent,
                IsExplicitContent = $IsExplicitContent,
                Note = $Note,
                VoteCount = $VoteCount,
                Name = $Name,
                DiffusionState = $DiffusionState,
                EpisodeCount = $EpisodeCount,
                EpisodeDuration = $EpisodeDuration,
                ReleaseDate = $ReleaseDate,
                ReleaseMonth = $ReleaseMonth,
                EndDate = $EndDate,
                Description = $Description,
                ThumbnailUrl = $ThumbnailUrl,
                IdFormat = $IdFormat,
                IdTarget = $IdTarget,
                IdOrigine = $IdOrigine,
                IdSeason = $IdSeason,
                Remark = $Remark,
                IdStatistic = $IdStatistic
            WHERE Id = $Id
            """;
        command.Parameters.AddWithValue("$SheetId", SheetId);
        command.Parameters.AddWithValue("$Url", Url);
        command.Parameters.AddWithValue("$IsAdultContent", IsAdultContent);
        command.Parameters.AddWithValue("$IsExplicitContent", IsExplicitContent);
        command.Parameters.AddWithValue("$Note", Note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$VoteCount", VoteCount);
        command.Parameters.AddWithValue("$Name", Name);
        command.Parameters.AddWithValue("$DiffusionState", (byte)DiffusionState);
        command.Parameters.AddWithValue("$EpisodeCount", EpisodesCount);
        command.Parameters.AddWithValue("$EpisodeDuration", Duration.TotalMinutes);
        command.Parameters.AddWithValue("$ReleaseDate", ReleaseDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ReleaseMonth", ReleaseMonth.ToNumberedDate());
        command.Parameters.AddWithValue("$EndDate", EndDate ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Description", Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$ThumbnailUrl", ThumbnailUrl ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdFormat", Format?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdTarget", Target?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdOrigine", OrigineAdaptation?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$IdSeason", Season?.Id ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Remark", Remark ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$Id", Id);
        command.Parameters.AddWithValue("$IdStatistic", Statistic is { Id: > 0 } ? Statistic.Id : DBNull.Value);

        try
        {
            var result = await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);

            AlternativeTitles.ToObservableCollection(
                await TanimeAlternativeTitle.AddOrUpdateOrDeleteAsync(Id, AlternativeTitles, cancellationToken).ToArrayAsync(), true);
            
            return result > 0
                ? new OperationState(true, "La base de l'anime a été modifiée avec succès")
                : new OperationState(false, "La base de l'anime n'a pas été modifiée");
        }
        catch (Exception e)
        {
            LogServices.LogDebug(e.Message);
            return new OperationState(false, "Une erreur est survenue lors de la mise à jour de la base de l'animé.");
        }
    }

    /// <summary>
    /// Retourne la liste des animés à mettre à jour en fonction de la saison.
    /// </summary>
    /// <param name="season"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<(int Id, int SheetId, string Url)> FindAnimesToBeUpdate(WeatherSeason season, CancellationToken? cancellationToken = null)
    {
        if (season.ToIntSeason() <= 0)
            yield break;
        
        await using var command = Main.Connection.CreateCommand();
        command.CommandText = 
            """
            SELECT 
                Id, 
                SheetId, 
                Url
            FROM 
                Tanime 
            WHERE IdSeason = (SELECT Id FROM Tseason WHERE SeasonNumber = $SeasonNumber)
            """;
        
        command.Parameters.AddWithValue("$SeasonNumber", season.ToIntSeason());
        
        var reader = await command.ExecuteReaderAsync();
        if (!reader.HasRows)
            yield break;
        
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var id = reader.GetInt32(reader.GetOrdinal("Id"));
            var sheetId = reader.GetInt32(reader.GetOrdinal("SheetId"));
            var sheetUri = reader.GetString(reader.GetOrdinal("Url"));
            
            var statistics = await TsheetStatistic.ScrapStatisticAsync(IcotakuSection.Anime, sheetId);
            if (statistics == null)
                continue;
            
            var currentStatistics = await TsheetStatistic.SingleAsync(IcotakuSection.Anime, sheetId);
            if (currentStatistics == null)
                continue;
            
            if (statistics.LastUpdatedDate <= currentStatistics.LastUpdatedDate)
                continue;
            
            yield return (id, sheetId, sheetUri);
        }
    }
    
    #endregion

    #region SingleOrCreate

     static Task<TanimeBase?> ITableBase<TanimeBase>.SingleOrCreateAsync(TanimeBase value, bool reloadIfExist,
        CancellationToken? cancellationToken)
    {
        throw new NotImplementedException();
    }

    public static Task<TanimeBase?> SingleOrCreateAsync(TanimeBase value, bool reloadIfExist = false, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }


    Task<OperationState<int>> ITableBase<TanimeBase>.AddOrUpdateAsync(CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    static Task<OperationState<int>> ITableBase<TanimeBase>.AddOrUpdateAsync(TanimeBase value, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Delete

    public async Task<OperationState> DeleteAsync(CancellationToken? cancellationToken = null)
        => await DeleteAsync(Id, IntColumnSelect.Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, CancellationToken? cancellationToken = null)
        => await DeleteAsync(id, IntColumnSelect.Id, cancellationToken);

    public static async Task<OperationState> DeleteAsync(int id, IntColumnSelect columnSelect,
        CancellationToken? cancellationToken = null)
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

        int idAnime;
        if (columnSelect == IntColumnSelect.Id)
            idAnime = id;
        else if (columnSelect == IntColumnSelect.SheetId)
        {
            command.CommandText = "SELECT Id FROM Tanime WHERE SheetId = $Id";
            command.Parameters.AddWithValue("$Id", id);
            var result = await command.ExecuteScalarAsync(cancellationToken ?? CancellationToken.None);
            if (result is long count)
                idAnime = (int)count;

            return new OperationState(false, "L'id de l'anime n'a pas été trouvé");
        }
        else
            return new OperationState(false, "La colonne sélectionnée n'est pas valide");
        
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
        command.Parameters.AddWithValue("$Id", idAnime);

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

    public static async Task<OperationState> DeleteAsync(Uri uri, CancellationToken? cancellationToken = null)
    {
        var id = await GetIdOfAsync(uri, cancellationToken);
        if (!id.HasValue)
            return new OperationState(false, "L'anime n'a pas été trouvé");

        return await DeleteAsync(id.Value, IntColumnSelect.Id, cancellationToken);
    }

    #endregion


    private static async Task<TanimeBase[]> GetRecords(SqliteDataReader reader,
        CancellationToken? cancellationToken = null)
    {
        List<TanimeBase> records = [];
        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None))
        {
            var baseId = reader.GetInt32(reader.GetOrdinal("AnimeId"));
            var record = records.Find(f => f.Id == baseId);
            if (record == null)
            {
                record = new TanimeBase(baseId)
                {
                    Guid = reader.GetGuid(reader.GetOrdinal("AnimeGuid")),
                    Name = reader.GetString(reader.GetOrdinal("AnimeName")),
                    Url = reader.GetString(reader.GetOrdinal("AnimeUrl")),
                    IsAdultContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsAdultContent")),
                    IsExplicitContent = reader.GetBoolean(reader.GetOrdinal("AnimeIsExplicitContent")),
                    VoteCount = (uint)reader.GetInt32(reader.GetOrdinal("AnimeVoteCount")),
                    SheetId = reader.GetInt32(reader.GetOrdinal("AnimeSheetId")),
                    DiffusionState = (DiffusionStateKind)reader.GetByte(reader.GetOrdinal("DiffusionState")),
                    EpisodesCount = (ushort)reader.GetInt16(reader.GetOrdinal("EpisodeCount")),
                    Duration = TimeSpan.FromMinutes(reader.GetInt32(reader.GetOrdinal("EpisodeDuration"))),
                    ReleaseMonth = MonthDate.FromNumberedDate((uint)reader.GetInt64(reader.GetOrdinal("ReleaseMonth"))),
                    ReleaseDate = reader.IsDBNull(reader.GetOrdinal("ReleaseDate"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ReleaseDate")),
                    EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("EndDate")),
                    Note = reader.IsDBNull(reader.GetOrdinal("AnimeNote"))
                        ? null
                        : reader.GetDouble(reader.GetOrdinal("AnimeNote")),
                    ThumbnailUrl = reader.IsDBNull(reader.GetOrdinal("AnimeThumbnailUrl"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeThumbnailUrl")),
                    Description = reader.IsDBNull(reader.GetOrdinal("AnimeDescription"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeDescription")),
                    Remark = reader.IsDBNull(reader.GetOrdinal("AnimeRemark"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("AnimeRemark")),
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
            
            if (!reader.IsDBNull(reader.GetOrdinal("AlternativeTitleId")))
            {
                var alternativeTitleId = reader.GetInt32(reader.GetOrdinal("AlternativeTitleId"));
                var alternativeTitle = record.AlternativeTitles.FirstOrDefault(x => x.Id == alternativeTitleId);
                if (alternativeTitle == null)
                {
                    alternativeTitle = new TanimeAlternativeTitle(alternativeTitleId, baseId)
                    {
                        Title = reader.GetString(reader.GetOrdinal("AlternativeTitle")),
                        Description = reader.IsDBNull(reader.GetOrdinal("AlternativeTitleDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("AlternativeTitleDescription"))
                    };
                    record.AlternativeTitles.Add(alternativeTitle);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("WebSiteId")))
            {
                var webSiteId = reader.GetInt32(reader.GetOrdinal("WebSiteId"));
                var webSite = record.Websites.FirstOrDefault(x => x.Id == webSiteId);
                if (webSite == null)
                {
                    webSite = new TanimeWebSite(webSiteId, baseId)
                    {
                        Url = reader.GetString(reader.GetOrdinal("WebSiteUrl")),
                        Description = reader.IsDBNull(reader.GetOrdinal("WebSiteDescription"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("WebSiteDescription"))
                    };
                    record.Websites.Add(webSite);
                }
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
                        descriptionIndex: reader.GetOrdinal("CategoryDescription"),
                        isFullyScrapedIndex: reader.GetOrdinal("CategoryIsFullyScraped"));
                    record.Categories.Add(category);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("StudioId")))
            {
                var studioId = reader.GetInt32(reader.GetOrdinal("StudioId"));
                var studio = record.Studios.FirstOrDefault(x => x.Id == studioId);
                if (studio == null)
                {
                    studio = await TcontactBase.SingleAsync(studioId, IntColumnSelect.Id, cancellationToken);
                    if (studio != null)
                        record.Studios.Add(studio);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("LicenseId")))
            {
                var licenseId = reader.GetInt32(reader.GetOrdinal("LicenseId"));
                var license = record.Licenses.FirstOrDefault(x => x.Id == licenseId);
                if (license == null)
                {
                    license = await TanimeLicense.SingleAsync(licenseId, cancellationToken);
                    if (license != null)
                        record.Licenses.Add(license);
                }
            }

            if (!reader.IsDBNull(reader.GetOrdinal("StaffId")))
            {
                var staffId = reader.GetInt32(reader.GetOrdinal("StaffId"));
                var staff = record.Staffs.FirstOrDefault(x => x.Id == staffId);
                if (staff == null)
                {
                    staff = await TanimeStaff.SingleAsync(staffId, cancellationToken);
                    if (staff != null)
                        record.Staffs.Add(staff);
                }
            }
            
            if (!reader.IsDBNull(reader.GetOrdinal("IdStatistic")))
            {
                var statisticId = reader.GetInt32(reader.GetOrdinal("IdStatistic"));
                var statistic = await TsheetStatistic.SingleAsync(statisticId, cancellationToken);
                if (statistic != null)
                    record.Statistic = statistic;
            }
        }

        return [.. records];
    }


    private const string IcotakuSqlSelectScript =
        """
        SELECT
            Tanime.Id AS AnimeId,
            Tanime.Guid AS AnimeGuid,
            Tanime.Name AS AnimeName,
            Tanime.IdStatistic,
            Tanime.IdFormat,
            Tanime.IdTarget,
            Tanime.IdOrigine,
            Tanime.IdSeason,
            Tanime.ReleaseMonth,
            Tanime.ReleaseDate,
            Tanime.EndDate,
            Tanime.IsAdultContent AS AnimeIsAdultContent,
            Tanime.IsExplicitContent AS AnimeIsExplicitContent,
            Tanime.Note AS AnimeNote,
            Tanime.VoteCount AS AnimeVoteCount,
            Tanime.Url AS AnimeUrl,
            Tanime.SheetId AS AnimeSheetId,
            Tanime.DiffusionState,
            Tanime.EpisodeCount,
            Tanime.EpisodeDuration,
            Tanime.ThumbnailUrl AS AnimeThumbnailUrl,
            Tanime.Description AS AnimeDescription,
            Tanime.Remark AS AnimeRemark,
            
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
            
            TanimeAlternativeTitle.Id AS AlternativeTitleId,
            TanimeAlternativeTitle.Title AS AlternativeTitle,
            TanimeAlternativeTitle.Description AS AlternativeTitleDescription,
            
            TanimeWebSite.Id AS WebSiteId,
            TanimeWebSite.Url AS WebSiteUrl,
            TanimeWebSite.Description AS WebSiteDescription,
            
            TanimeCategory.IdCategory AS CategoryId,
            Tcategory.SheetId AS CategorySheetId,
            Tcategory.Type AS CategoryType,
            Tcategory.Url AS CategoryUrl,
            Tcategory.Section AS CategorySection,
            Tcategory.Name AS CategoryName,
            Tcategory.Description AS CategoryDescription,
            Tcategory.IsFullyScraped AS CategoryIsFullyScraped,
            
            TanimeStudio.IdStudio AS StudioId,
            contactStudio.Id AS ContactStudioId,
            TanimeLicense.Id AS LicenseId,
            contactDistributor.Id AS ContactDistributorId,
            TanimeStaff.Id AS StaffId,
            contactStaff.Id AS ContactStaffId
        
        FROM
            Tanime
                LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
                LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
                LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
                LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
                LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
                LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
                LEFT JOIN main.TanimeAlternativeTitle on Tanime.Id = TanimeAlternativeTitle.IdAnime
                LEFT JOIN main.TanimeWebSite on Tanime.Id = TanimeWebSite.IdAnime
                LEFT JOIN main.TanimeStudio on Tanime.Id = TanimeStudio.IdAnime
                LEFT JOIN main.Tcontact contactStudio on contactStudio.Id = TanimeStudio.IdStudio
                LEFT JOIN main.TanimeLicense on Tanime.Id = TanimeLicense.IdAnime
                LEFT JOIN main.Tcontact contactDistributor on contactDistributor.Id = TanimeLicense.IdDistributor
                LEFT JOIN main.TanimeStaff on Tanime.Id = TanimeStaff.IdAnime
                LEFT JOIN main.Tcontact contactStaff on contactStaff.Id = TanimeStaff.IdIndividu

        """;
    
    private const string IcotakuSqlCountScript =
        """
        SELECT
            COUNT(DISTINCT Tanime.Id)
        FROM
            Tanime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        LEFT JOIN main.TanimeAlternativeTitle on Tanime.Id = TanimeAlternativeTitle.IdAnime
        LEFT JOIN main.TanimeWebSite on Tanime.Id = TanimeWebSite.IdAnime
        LEFT JOIN main.TanimeStudio on Tanime.Id = TanimeStudio.IdAnime
        LEFT JOIN main.Tcontact contactStudio on contactStudio.Id = TanimeStudio.IdStudio
        LEFT JOIN main.TanimeLicense on Tanime.Id = TanimeLicense.IdAnime
        LEFT JOIN main.Tcontact contactDistributor on contactDistributor.Id = TanimeLicense.IdDistributor
        LEFT JOIN main.TanimeStaff on Tanime.Id = TanimeStaff.IdAnime
        LEFT JOIN main.Tcontact contactStaff on contactStaff.Id = TanimeStaff.IdIndividu

        """;

    private const string IcotakuSqlGetIdScript =
        """
        SELECT DISTINCT
            Tanime.Id AS AnimeId
        FROM
            Tanime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        LEFT JOIN main.TanimeAlternativeTitle on Tanime.Id = TanimeAlternativeTitle.IdAnime
        LEFT JOIN main.TanimeWebSite on Tanime.Id = TanimeWebSite.IdAnime
        LEFT JOIN main.TanimeStudio on Tanime.Id = TanimeStudio.IdAnime
        LEFT JOIN main.Tcontact contactStudio on contactStudio.Id = TanimeStudio.IdStudio
        LEFT JOIN main.TanimeLicense on Tanime.Id = TanimeLicense.IdAnime
        LEFT JOIN main.Tcontact contactDistributor on contactDistributor.Id = TanimeLicense.IdDistributor
        LEFT JOIN main.TanimeStaff on Tanime.Id = TanimeStaff.IdAnime
        LEFT JOIN main.Tcontact contactStaff on contactStaff.Id = TanimeStaff.IdIndividu
        
        """;
    
    private const string IcotakuSqlGetSheetIdScript =
        """
        SELECT DISTINCT
            Tanime.SheetId AS AnimeSheetId
        FROM
            Tanime
        LEFT JOIN main.Tformat  on Tformat.Id = Tanime.IdFormat
        LEFT JOIN main.Ttarget  on Ttarget.Id = Tanime.IdTarget
        LEFT JOIN main.TorigineAdaptation on TorigineAdaptation.Id = Tanime.IdOrigine
        LEFT JOIN main.Tseason  on Tseason.Id = Tanime.IdSeason
        LEFT JOIN main.TanimeCategory on Tanime.Id = TanimeCategory.IdAnime
        LEFT JOIN main.Tcategory on Tcategory.Id = TanimeCategory.IdCategory
        LEFT JOIN main.TanimeAlternativeTitle on Tanime.Id = TanimeAlternativeTitle.IdAnime
        LEFT JOIN main.TanimeWebSite on Tanime.Id = TanimeWebSite.IdAnime
        LEFT JOIN main.TanimeStudio on Tanime.Id = TanimeStudio.IdAnime
        LEFT JOIN main.Tcontact contactStudio on contactStudio.Id = TanimeStudio.IdStudio
        LEFT JOIN main.TanimeLicense on Tanime.Id = TanimeLicense.IdAnime
        LEFT JOIN main.Tcontact contactDistributor on contactDistributor.Id = TanimeLicense.IdDistributor
        LEFT JOIN main.TanimeStaff on Tanime.Id = TanimeStaff.IdAnime
        LEFT JOIN main.Tcontact contactStaff on contactStaff.Id = TanimeStaff.IdIndividu

        """;
}