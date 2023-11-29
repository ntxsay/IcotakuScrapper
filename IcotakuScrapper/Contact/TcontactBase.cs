namespace IcotakuScrapper.Contact;

public class TcontactBase
{
    /// <summary>
    /// Obtient ou définit l'id du contact.
    /// </summary>
    public int Id { get; protected set; }
    
    /// <summary>
    /// Obtient ou définit l'id de la fiche Icotaku du contact.
    /// </summary>
    public int SheetId { get; set; }
    
    /// <summary>
    /// Obtient ou définit le type de contact.
    /// </summary>
    public ContactType Type { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nom d'affichage du contact.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Obtient ou définit la description de l'anime.
    /// </summary>
    public string? Presentation { get; set; }

    /// <summary>
    /// Obtient ou définit l'url de la fiche de l'anime.
    /// </summary>
    public string Url { get; set; } = null!;

}