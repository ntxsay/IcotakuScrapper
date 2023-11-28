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
}