namespace IcotakuScrapper;

public enum ItemGroupCountKind
{
    None,
    AnimeLetter,
    Season,
    OrigineAdaptation,
    ReleaseMonth,
    GroupName,
}

public struct ItemGroupCountStruct
{
    public ItemGroupCountStruct()
    {
    }

    /// <summary>
    /// Obtient ou définit l'identifiant (Id de la base de données) de l'élément actuel
    /// </summary>
    public object? Data { get; set; }
    
    /// <summary>
    /// Obtient ou définit le type de données
    /// </summary>
    public ItemGroupCountKind IdentifierKind { get; set; }

    public string GroupName { get; set; } = "Groupe inconnu";


    /// <summary>
    /// Obtient ou définit le nom de l'élément actuel
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Obtient ou définit la description de l'élément actuel
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nombre d'éléments en tout
    /// </summary>
    public int Count { get; set; }
}

public class ItemGroupCount
{
    /// <summary>
    /// Obtient ou définit l'identifiant (Id de la base de données) de l'élément actuel
    /// </summary>
    public object? Data { get; set; }
    
    public string GroupName { get; set; } = "Groupe inconnu";

    /// <summary>
    /// Obtient ou définit le nom de l'élément actuel
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Obtient ou définit la description de l'élément actuel
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Obtient ou définit le nombre d'éléments en tout
    /// </summary>
    public int Count { get; set; }
}