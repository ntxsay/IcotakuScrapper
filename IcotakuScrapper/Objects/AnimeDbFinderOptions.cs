using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Objects;

public record AnimeDbFinderOptions
{
    public string? Keyword { get; init; } = null;
    public bool IsFindInDescription { get; init; }
    public bool IsFindInTitles { get; init; }
    public bool IsFindInRemark { get; init; }
    public bool? IsAdultContent { get; init; }
    public bool? IsExplicitContent { get; init; }
    public bool? HasThumbnail { get; init; }
    public MonthDate MinDate { get; init; }
    public MonthDate MaxDate { get; init; }

    public ItemGroupCountStruct[] ItemGroupCountData { get; init; } = [];

    public HashSet<int> IdOrigineAdaptationToInclude { get; init; } = [];
    public HashSet<int> IdOrigineAdaptationToExclude { get; init; } = [];
    public HashSet<int> IdDistributorsToInclude { get; init; } = [];
    public HashSet<int> IdDistributorsToExclude { get; init; } = [];
    public HashSet<int> IdStudiosToInclude { get; init; } = [];
    public HashSet<int> IdStudiosToExclude { get; init; } = [];

    public HashSet<int> IdTargetToInclude { get; init; } = [];
    public HashSet<int> IdTargetToExclude { get; init; } = [];
    
    public HashSet<int> IdGenreToInclude { get; init; } = [];
    public HashSet<int> IdGenreToExclude { get; init; } = [];


    
    public bool HasMinDate => !MinDate.Equals(default(MonthDate)) && MinDate.Month is > 0 and < 13 && MinDate.Year > 0;
    public bool HasMaxDate => !MaxDate.Equals(default(MonthDate)) && MaxDate.Month is > 0 and < 13 && MaxDate.Year > 0;
    public bool HasKeyword => Keyword != null && !Keyword.IsStringNullOrEmptyOrWhiteSpace();

    public bool HasIdOrigineAdaptationToInclude => IdOrigineAdaptationToInclude.Count > 0;
    public bool HasIdOrigineAdaptationToExclude => IdOrigineAdaptationToExclude.Count > 0;

    public bool HasIdDistributorsToInclude => IdDistributorsToInclude.Count > 0;
    public bool HasIdDistributorsToExclude => IdDistributorsToExclude.Count > 0;

    public bool HasIdStudiosToInclude => IdStudiosToInclude.Count > 0;
    public bool HasIdStudiosToExclude => IdStudiosToExclude.Count > 0;
}