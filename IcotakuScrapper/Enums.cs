
namespace IcotakuScrapper;

/// <summary>
/// Enumération des colonnes de type entier permettant de sélectionner ou de compter des enregistrements via celles-ci.
/// </summary>
public enum IntColumnSelect
{
    /// <summary>
    /// Représente la colonne "Id".
    /// </summary>
    Id,

    /// <summary>
    /// Représente la colonne "SheetId".
    /// </summary>
    SheetId,

    /// <summary>
    /// Représente la colonne "IdAnime".
    /// </summary>
    IdAnime,

    /// <summary>
    /// Représente la colonne "IdOrigine".
    /// </summary>
    IdOrigine,

    /// <summary>
    /// Représente la colonne "IdSeason".
    /// </summary>
    IdSeason,
    
    SeasonNumber,

    /// <summary>
    /// Représente la colonne "IdTarget".
    /// </summary>
    IdTarget,

    /// <summary>
    /// Représente la colonne "IdStudio".
    /// </summary>
    IdStudio,

    /// <summary>
    /// Représente la colonne Id pointant vers un contact.
    /// </summary>
    IdContact,
    IdDistributor,
    IdSeasonalPlanning,
    /// <summary>
    /// Représente la colonne "IdLicenseType".
    /// </summary>
    IdLicenseType,
    IdRole,
}

/// <summary>
/// Afin d'éviter de créer plusieurs méthodes de sélection, cette énumération permet de sélectionner la colonne à utiliser pour la sélection.
/// </summary>
[Obsolete("Utilisez l'énumération IntColumnSelect à la place")]
public enum SheetIntColumnSelect
{
    Id,
    SheetId,
}

/// <summary>
/// Enumération des colonnes d'id permettant de sélectionner ou de compter  des enrehistrements via l'id ou l'idAnime.
/// </summary>
public enum SelectCountIdIdAnimeKind
{
    Id,
    IdAnime,
}



/// <summary>
/// Enumération des sections du site icotaku.com et exploité par l'API.
/// <para>
/// Les sections sont utilisées aussi pour sélectionner la bonne base Url du site lors d'un scraping :
/// <list type="bullet">La base url de la section "<see cref="IcotakuSection.Anime"/>" sera "<see cref="IcotakuWebHelpers.IcotakuAnimeUrl"/>"</list>
/// <list type="bullet">La base url de la section "<see cref="IcotakuSection.Manga"/>" sera "<see cref="IcotakuWebHelpers.IcotakuMangaUrl"/>"</list>
/// </para>
/// </summary>
/// <example>
/// </example>
public enum IcotakuSection : byte
{
    /// <summary>
    /// Représente la section des animés du site d'Icotaku.
    /// </summary>
    /// <remarks>Les animés sur Icotaku proviennent presqu'exclusivement du Japon</remarks>
    Anime,
    Manga,
    LightNovel,
    Drama,
    Community,
}

/// <summary>
/// Enumération des types de fiches Icotaku.
/// </summary>
public enum IcotakuSheetType : byte
{
    Unknown,
    Anime,
    Manga,
    LightNovel,
    Drama,
    Person,
    Character,
    Studio,
    Distributor,
}

public enum IcotakuAnimeSearchType
{
    StatusDiffusion,
    Distributor,
    Format,
    OrigineAdaptation,
    Target,
    Genre,
    Theme
}


public enum IcotakuDefaultSubFolder : byte
{
    /// <summary>
    /// Si le type de téléchargement est inconnu ou inexistant.
    /// </summary>
    None,
    /// <summary>
    /// Représente le dossier des épisodes d'un animé.
    /// </summary>
    /// <remarks>Attention il ne s'agit pas d'épisodes au format vidéo mais des captures d'écran de l'épisode en question.</remarks>
    Episod,

    /// <summary>
    /// Représente le dossier des converture du tome d'un manga ou d'un roman (lightnovel).
    /// </summary>
    Tome,

    /// <summary>
    /// Représente le dossier d'une fiche d'un animé ou d'un drama et contient généralement la vignette de la fiche.
    /// </summary>
    Sheet,
}



public enum IcotakuDefaultFolder : byte
{
    Animes,
    Mangas,
    LightNovels,
    Dramas,
    Contacts,
    Community,
}

public enum IcotakuListType
{
    MostAwaited,
    MostPopular,
}

/// <summary>
/// Enumère les options de scraping.
/// </summary>
[Flags]
public enum AnimeScrapingOptions : int
{
    None = -1,
    
    /// <summary>
    /// Inclut les informations de base de l'animé :
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// Inclut le format de l'animé (série, film, OAV, ONA, etc...)
    /// </summary>
    Format = 1,
    
    /// <summary>
    /// Inclut l'origine de l'animé (manga, light novel, jeu vidéo, etc...)
    /// </summary>
    OriginAdaptation = 2,
    
    /// <summary>
    /// Inclut le public cible de l'animé (shonen, seinen, etc...)
    /// </summary>
    Target = 4,
    
    /// <summary>
    /// Inclut la saison de diffusion de l'animé
    /// </summary>
    Season = 8,
    
    /// <summary>
    /// Inclut les titres alternatifs de l'animé
    /// </summary>
    AlternativeTitles = 16,
    
    /// <summary>
    /// Inclut les sites web de l'animé
    /// </summary>
    WebSites = 32,
    
    /// <summary>
    /// Inclut le ou les épisodes dans le scraping
    /// </summary>
    /// <remarks>Les fiches Icotaku ne contiennent pas la date complète de diffusion sur la page principale de l'animé ou du drama
    /// mais sur la page dédiée aux épisodes, ce qui signifie que vous n'obtiendrez la date complète de diffusion que si vous incluez cette énumération.</remarks>
    Episodes = 64,
    
    /// <summary>
    /// Inclut juste le noms des studios sans la description
    /// </summary>
    Studios = 128,
    
    /// <summary>
    /// Inclut les studios avec la description
    /// </summary>
    FullStudios = 256,
    
    /// <summary>
    /// Inclut juste le noms des personnages sans la description
    /// </summary>
    Characters = 512,
    
    /// <summary>
    /// Inclut les personnages avec la description
    /// </summary>
    FullCharacters = 1024,
    
    /// <summary>
    /// Inclut juste le noms du staff sans la description
    /// </summary>
    Staff = 2048,
    
    /// <summary>
    /// Inclut les membres du staff avec la description
    /// </summary>
    FullStaff = 4096,
    
    /// <summary>
    /// Inclut les catégories sans la description 
    /// </summary>
    Categories = 8192,
    
    /// <summary>
    /// Inclut les catégories avec la description 
    /// </summary>
    /// <remarks>La fiche anime n'inclut pas naturellement la description de la catégorie, il faut scrapper la fiche de la catégorie elle-même pour obtenir cette information</remarks>
    FullCategories = 16384,
    
    /// <summary>
    /// Inclut les noms des détendeurs de licences pour cet animé sans la description
    /// </summary>
    Licenses = 32768,
    
    /// <summary>
    /// Inclut les détendeurs de licences pour cet animé avec la description
    /// </summary>
    FullLicenses = 65536,
    
    /// <summary>
    /// Inclut les statistiques de l'animé
    /// </summary>
    Statistic = 131072,
    
    /// <summary>
    /// Inclut uniquement les informations nécessaire de l'animé pour le planning saisonnier
    /// </summary>
    SeasonalPlanning = Categories | Season,
    
    /// <summary>
    /// Inclut toutes les informations de l'animé
    /// </summary>
    All = Default | Format | OriginAdaptation | Target | Season | AlternativeTitles | WebSites | Episodes | FullStudios | FullCharacters | FullStaff | FullCategories | FullLicenses | Statistic,
}

/// <summary>
/// Enumération des modes de tri.
/// </summary>
public enum OrderBy : byte
{
    /// <summary>
    /// Tri les éléments par ordre croissant.
    /// </summary>
    Asc,

    /// <summary>
    /// Tri les éléments par ordre décroissant.
    /// </summary>
    Desc
}

/// <summary>
/// Enumération des types de catégories.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// Représente la catégorie "Theme".
    /// </summary>
    Theme,

    /// <summary>
    /// Représente la catégorie "Genre".
    /// </summary>
    Genre,
}

#region Status de lecture/visionnage et statut de diffusion

/// <summary>
/// Représente le status de lecture ou de visionnage d'une oeuvre.
/// </summary>
public enum WatchStatusKind : byte
{
    /// <summary>
    /// La lecture ou le visionnage n'a pas été plannifié
    /// </summary>
    NotPlanned,
    
    /// <summary>
    /// La lecture ou le visionnage a été plannifié mais n'a pas encore commencé
    /// </summary>
    Planned,
    
    /// <summary>
    /// La lecture ou le visionnage est en cours
    /// </summary>
    InProgress,
    
    /// <summary>
    /// La lecture ou le visionnage a été mis en pause
    /// </summary>
    Paused,
    
    /// <summary>
    /// La lecture ou le visionnage a été arrêté
    /// </summary>
    Dropped,
    
    /// <summary>
    /// La lecture ou le visionnage a été complété
    /// </summary>
    Completed,
}

/// <summary>
/// Représente un état de diffusion
/// </summary>
public enum DiffusionStateKind : byte
{
    /// <summary>
    /// Diffusion inconnue
    /// </summary>
    Unknown,
    /// <summary>
    /// Diffusion à venir
    /// </summary>
    UpComing,
    /// <summary>
    /// Diffusion en cours
    /// </summary>
    InProgress,
    /// <summary>
    /// Diffusion terminée
    /// </summary>
    Completed,
    /// <summary>
    /// Diffusion en pause
    /// </summary>
    Paused,
    /// <summary>
    /// Diffusion arrêtée
    /// </summary>
    Stopped
}

#endregion

public enum ContactType : byte
{
    Unknown,
    Person,
    Character,
    Studio,
    Distributor,
}

public enum RoleType : byte
{
    Staff,
    Character,
}

/// <summary>
/// Enumération des saisons.
/// </summary>
public enum WeatherSeasonKind : byte
{
    Unknown = 0,
    /// <summary>
    /// Printemps
    /// </summary>
    Spring = 1,
    /// <summary>
    /// Eté
    /// </summary>
    Summer = 2,
    /// <summary>
    /// Automne
    /// </summary>
    Fall = 3,
    /// <summary>
    /// Hiver
    /// </summary>
    Winter = 4,
}

#region Database

public enum DbInsertMode
{
    Insert,
    InsertOrReplace,
    InsertOrIgnore,
    InsertOrAbort,
    InsertOrFail,
    InsertOrRollback,
    Replace,
}

public enum DbStartFilterMode
{
    None,
    Where,
    And,
    Or,
}

public enum DbScriptMode : byte
{
    Select,
    Count,
    GetId
}

#endregion

