namespace IcotakuScrapper.Common;

/// <summary>
/// Représente un titre alternatif d'un animé, d'un manga ou autre.
/// </summary>
public class AlternativeTitle
{
    /// <summary>
    /// Obtient ou définit le titre alternatif.
    /// </summary>
    public string Title { get; set; } = null!;
    ///                      
    /// <summary>
    /// Obtient ou définit la description du titre alternatif.
    /// </summary>
    /// <example>titre original, titre français, titre alternatif, etc.</example>
    public string? Description { get; set; }
}