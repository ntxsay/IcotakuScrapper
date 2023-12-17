using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Common;
using IcotakuScrapper.Services;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanningAnimeController : ControllerBase
    {
        [HttpGet("Seasonal/All")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] SeasonalAnimePlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/Many")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery] uint seasonNumber, [FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] SeasonalAnimePlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(seasonNumber, isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/Many/MultiQueries")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery] WeatherSeason season, [FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] SeasonalAnimePlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(season, isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/ItemsGroupCount")]
        public async Task<ItemGroupCountStruct[]> SelectAllSeasonalPlanningAsync([FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] SeasonalAnimePlanningGroupBy groupBy, [FromQuery] SeasonalAnimePlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc)
            => await TanimeSeasonalPlanning.CountAndGroupBySelectionMode(groupBy, orderBy, isAdultContent, isExplicitContent).ToArrayAsync();
        
        [HttpPost("Seasonal/Scrap")]
        public async Task<OperationState> SaveDailyPlanningsync([FromQuery] uint year, [FromQuery] WeatherSeasonKind season)
        {
            if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year)
                return new OperationState(false, "L'année n'est pas valide");
            
            if (season == WeatherSeasonKind.Unknown)
                return new OperationState(false, "La saison n'est pas valide");
            
            return await TanimeSeasonalPlanning.ScrapAsync(new WeatherSeason(season, year));
        }

        //[HttpGet("Daily/String/Range")]
        //public TanimeDailyPlanning[] SelectDailyPlanningAsync([FromQuery] string minDate, [FromQuery] string maxDate)
        //{
        //    if (!DateOnly.TryParse(minDate, out var min) || !DateOnly.TryParse(maxDate, out var max))
        //        return [];

        //    return TanimeDailyPlanning.GetAnimePlanning(minDate, maxDate);
        //}

        [HttpGet("Daily/All")]
        public async Task<TanimeDailyPlanning[]> SelectDailyPlanningAsync([FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] AnimeDailyPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
                       [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeDailyPlanning.SelectAsync(isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);

        [HttpGet("Daily/OneDate")]
        public async Task<TanimeDailyPlanning[]> SelectDailyPlanningAsync([FromQuery] string date, [FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] AnimeDailyPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            if (!DateOnly.TryParse(date, out var d))
                return [];
            return await TanimeDailyPlanning.SelectAsync(d, isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);
        }

        [HttpGet("Daily/RangeDate")]
        public async Task<TanimeDailyPlanning[]> SelectDailyPlanningAsync([FromQuery] string minDate, [FromQuery] string maxDate, [FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] AnimeDailyPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
                                             [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            if (!DateOnly.TryParse(minDate, out var min) || !DateOnly.TryParse(maxDate, out var max))
                return [];
            return await TanimeDailyPlanning.SelectAsync(min, max, isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);
        }

        [HttpPost("Daily/Scrap/RangeDate")]
        public async Task<OperationState> SelectDailyPlanningAsync([FromQuery] string minDate, [FromQuery] string maxDate)
        {
            if (!DateOnly.TryParse(minDate, out var min) || !DateOnly.TryParse(maxDate, out var max))
                return new OperationState(false, "La date n'est pas valide");
            return await TanimeDailyPlanning.ScrapAsync(min, max);
        }

        [HttpPost("Daily/Scrap/OneDate")]
        public async Task<OperationState> SelectDailyPlanningAsync([FromQuery] string date)
        {
            if (!DateOnly.TryParse(date, out var d))
                return new OperationState(false, "La date n'est pas valide");
            return await TanimeDailyPlanning.ScrapAsync(d);
        }
    }


}
