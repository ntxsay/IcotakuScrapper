using IcotakuScrapper.Common;

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
    
}