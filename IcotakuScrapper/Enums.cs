
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

    /// <summary>
    /// Représente la colonne "IdTarget".
    /// </summary>
    IdTarget,

    /// <summary>
    /// Représente la colonne "IdStudio".
    /// </summary>
    IdStudio,

    /// <summary>
    /// Représente la colonne "IdDistributor".
    /// </summary>
    IdDistributor,

    /// <summary>
    /// Représente la colonne "IdLicenseType".
    /// </summary>
    IdLicenseType,
}

/// <summary>
/// Afin d'éviter de créer plusieurs méthodes de sélection, cette énumération permet de sélectionner la colonne à utiliser pour la sélection.
/// </summary>
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

public enum DbScriptMode : byte
{
    Select,
    Count,
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

public enum IcotakuDownloadSection : byte
{
    /// <summary>
    /// Reprentente la section des animés du dossier de téléchargement.
    /// </summary>
    Anime,
    Manga,
    LightNovel,
    Drama,
    Person,
    Character
}

public enum IcotakuDownloadType : byte
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


public enum SheetType : byte
{
    Unknown,
    Anime,
    Person,
    Character,
    Studio,
    Distributor,
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
    Stopped,
}

public enum ContactType : byte
{
    Unknown,
    Person,
    Character,
    Studio,
    Distributor,
}



public enum MoisKind
{
    Unknow,
    Janvier,
    Février,
    Mars,
    Avril,
    Mai,
    Juin,
    Juillet,
    Août,
    Septembre,
    Octobre,
    Novembre,
    Décembre
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

