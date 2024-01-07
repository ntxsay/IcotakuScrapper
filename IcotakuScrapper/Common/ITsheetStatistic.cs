namespace IcotakuScrapper.Common;

/// <summary>
/// Interface représentant les statistiques d'une fiche
/// </summary>
public interface ITsheetStatistic
{
    /// <summary>
    /// Obtient ou définit la section dans laquelle se trouve la fiche
    /// </summary>
    public IcotakuSection Section { get; set; }
    
    /// <summary>
    /// Obtient ou définit la date de création de l'animé
    /// </summary>
    public DateTime? CreatingDate { get; set; }
    
    /// <summary>
    /// Obtient ou définit la date de la dernière mise à jour de la fiche
    /// </summary>
    public DateTime? LastUpdatedDate { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre Icotaku qui a créé la fiche
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre du site d'Icotaku qui a mis à jour la fiche pour la dernière fois
    /// </summary>
    public string? LastUpdatedBy { get; set; }
    
    /// <summary>
    /// Obtient ou définit l'âge moyen des membres ayant cet anime dans leur watchlist
    /// </summary>
    public float? InWatchListAverageAge { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nombre de visite qu'a eu cette fiche jusqu'à présent
    /// </summary>
    public uint VisitCount { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom du membre qui a visité cette fiche pour la dernière fois
    /// </summary>
    public string? LastVisitedBy { get; set; }
}