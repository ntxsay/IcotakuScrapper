using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects.Models;

public record AnimeDbFinderOptions
{
    public string? Keyword { get; init; }
    public bool IsFindInDescription { get; init; }
    public bool IsFindInTitles { get; init; }
    public bool IsFindInRemark { get; init; }
    public bool? IsAdultContent { get; init; }
    public bool? IsExplicitContent { get; init; }
    public MonthDate MinDate { get; init; }
    public MonthDate MaxDate { get; init; }

    /// <summary>
    /// Obtient ou définit la saison de l'anime
    /// </summary>
    /// <remarks>Cette propriété est utiliser pour faire des recherche dans le champs de vision d'une saison</remarks>
    public WeatherSeason? Season { get; init; }

    public ItemGroupCountStruct[] ItemGroupCountData { get; init; } = [];

    //Origines
    public HashSet<int> IdOrigineAdaptationToInclude { get; init; } = [];
    public HashSet<int> IdOrigineAdaptationToExclude { get; init; } = [];
    
    //Distributeurs
    public HashSet<int> IdDistributorsToInclude { get; init; } = [];
    public HashSet<int> IdDistributorsToExclude { get; init; } = [];
    
    //Studios
    public HashSet<int> IdStudiosToInclude { get; init; } = [];
    public HashSet<int> IdStudiosToExclude { get; init; } = [];

    //Cible démographique
    public HashSet<int> IdTargetToInclude { get; init; } = [];
    public HashSet<int> IdTargetToExclude { get; init; } = [];
    
    //Formats
    public HashSet<int> IdFormatToInclude { get; init; } = [];
    public HashSet<int> IdFormatToExclude { get; init; } = [];
    
    //Catégories
    public HashSet<int> IdCategoriesToInclude { get; init; } = [];
    public HashSet<int> IdCategoriesToExclude { get; init; } = [];
    
    public bool HasMinDate => !MinDate.Equals(default(MonthDate)) && MinDate.Month is > 0 and < 13 && MinDate.Year > 0;
    public bool HasMaxDate => !MaxDate.Equals(default(MonthDate)) && MaxDate.Month is > 0 and < 13 && MaxDate.Year > 0;
    public bool HasKeyword => Keyword != null && !Keyword.IsStringNullOrEmptyOrWhiteSpace();

    public bool HasIdOrigineAdaptationToInclude => IdOrigineAdaptationToInclude.Count > 0;
    public bool HasIdOrigineAdaptationToExclude => IdOrigineAdaptationToExclude.Count > 0;

    public bool HasIdDistributorsToInclude => IdDistributorsToInclude.Count > 0;
    public bool HasIdDistributorsToExclude => IdDistributorsToExclude.Count > 0;

    public bool HasIdStudiosToInclude => IdStudiosToInclude.Count > 0;
    public bool HasIdStudiosToExclude => IdStudiosToExclude.Count > 0;
    
    public bool HasIdTargetToInclude => IdTargetToInclude.Count > 0;
    public bool HasIdTargetToExclude => IdTargetToExclude.Count > 0;
    
    public bool HasIdCategoryToInclude => IdCategoriesToInclude.Count > 0;
    public bool HasIdCategoryToExclude => IdCategoriesToExclude.Count > 0;
    
    public bool HasIdFormatToInclude => IdFormatToInclude.Count > 0;
    public bool HasIdFormatToExclude => IdFormatToExclude.Count > 0;

    public bool HasSeason => Season != null && !Season.Equals(default(WeatherSeason));



}