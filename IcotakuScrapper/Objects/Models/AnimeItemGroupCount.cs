using System.Collections.ObjectModel;

namespace IcotakuScrapper.Objects.Models;


public enum AnimeItemGroupCountKind
{
    None,
    AnimeLetter,
    Season,
    OrigineAdaptation,
    Category,
    ReleaseMonth,
    Target,
    Format,
}

public enum AnimeItemGroupCountScopeKind
{
    None,
    Season,
    Day,
}

public struct AnimeItemGroupCountStruct
{
    public AnimeItemGroupCountStruct()
    {
    }

    /// <summary>
    /// Obtient ou définit l'identifiant (Id de la base de données) de l'élément actuel
    /// </summary>
    public object? Data { get; set; }
    
    /// <summary>
    /// Obtient ou définit le type de données
    /// </summary>
    public AnimeItemGroupCountKind IdentifierKind { get; set; }

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

public class AnimeItemGroupCount
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

public class AnimeItemGroupCountCastVm
{
    public string GroupName { get; set; } = "Groupe inconnu";
    public string? Description { get; set; }
    public ObservableCollection<AnimeItemGroupCountStruct> Items { get; set; } = [];
}