namespace IcotakuScrapper.Anime;

/// <summary>
/// Représente la base d'un animé.
/// </summary>
public class AnimeBase
{
    /// <summary>
    /// Obtient ou définit l'id de l'anime.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Obtient ou définit le titire de l'anime.
    /// </summary>
    public string Title { get; set; } = null!;

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
