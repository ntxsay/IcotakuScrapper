using IcotakuScrapper.Common;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    public static async Task<Paginate<TanimeBase>> PaginateAsync(IReadOnlyCollection<TanimeBase> values,
        uint currentPage = 1, uint maxContentByPage = 20,
        AnimeSortBy sortBy = AnimeSortBy.Name,
        OrderBy orderBy = OrderBy.Asc)
    {
        int totalItems = values.Count;
        if (totalItems <= 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        var totalPages = ExtensionMethods.CountPage((uint)totalItems, maxContentByPage);
        var paginatedValues = ExtensionMethods.GetPage(values, currentPage, maxContentByPage);
        if (paginatedValues.Length == 0)
            return new Paginate<TanimeBase>(
                currentPage: 1,
                totalPages: 1,
                maxItemsPerPage: maxContentByPage,
                totalItems: 0,
                items: []);

        return new Paginate<TanimeBase>(
            currentPage: currentPage,
            totalPages: totalPages,
            maxItemsPerPage: maxContentByPage,
            totalItems: (uint)totalItems,
            items: paginatedValues);
    }
}

public record AnimeFinderOptions
{
    public string? Keyword { get; init; } = null;
    public string? GroupName { get; init; } = null;
    public bool? IsAdultContent { get; init; }
    public bool? IsExplicitContent { get; init; }
    public bool? HasThumbnail { get; init; }
    public WeatherSeason MinSeason { get; init; }
    public WeatherSeason MaxSeason { get; init; }
    public uint MinReleaseMonth { get; init; }
    public uint MaxReleaseMonth { get; init; }

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


    
    public bool HasMinSeason => !MinSeason.Equals(default(WeatherSeason)) && MinSeason.Season != WeatherSeasonKind.Unknown && MinSeason.Year > 0;
    public bool HasMaxSeason => !MaxSeason.Equals(default(WeatherSeason)) && MaxSeason.Season != WeatherSeasonKind.Unknown && MaxSeason.Year > 0;
    public bool HasMinReleaseMonth => MinReleaseMonth > 0;
    public bool HasMaxReleaseMonth => MaxReleaseMonth > 0;
    public bool HasKeyword => Keyword != null && !Keyword.IsStringNullOrEmptyOrWhiteSpace();

    public bool HasIdOrigineAdaptationToInclude => IdOrigineAdaptationToInclude.Count > 0;
    public bool HasIdOrigineAdaptationToExclude => IdOrigineAdaptationToExclude.Count > 0;

    public bool HasIdDistributorsToInclude => IdDistributorsToInclude.Count > 0;
    public bool HasIdDistributorsToExclude => IdDistributorsToExclude.Count > 0;

    public bool HasIdStudiosToInclude => IdStudiosToInclude.Count > 0;
    public bool HasIdStudiosToExclude => IdStudiosToExclude.Count > 0;
    public bool HasGroupName => GroupName != null && !GroupName.IsStringNullOrEmptyOrWhiteSpace();

}