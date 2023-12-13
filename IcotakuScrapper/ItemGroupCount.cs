namespace IcotakuScrapper;

public enum ItemGroupCountKind
{
    AnimeLetter,
    Season,
    
}

public readonly struct ItemGroupCountStruct
{
    /// <summary>
    /// Obtient ou définit l'identifiant (Id de la base de données) de l'élément actuel
    /// </summary>
    public uint Id { get; init; }
    
    /// <summary>
    /// Obtient ou définit le type de données
    /// </summary>
    public ItemGroupCountKind IdentifierKind { get; init; }
    
    /// <summary>
    /// Obtient ou définit le nom de l'élément actuel
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// Obtient ou définit la description de l'élément actuel
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Obtient ou définit le nombre d'éléments en tout
    /// </summary>
    public uint Count { get; init; }
}

public class ItemGroupCount
{
    /// <summary>
    /// Obtient ou définit l'identifiant (Id de la base de données) de l'élément actuel
    /// </summary>
    public uint Id { get; init; }
    
    /// <summary>
    /// Obtient ou définit le nom de l'élément actuel
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Obtient ou définit la description de l'élément actuel
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nombre d'éléments en tout
    /// </summary>
    public uint Count { get; set; }
}