using IcotakuScrapper.Anime;
using IcotakuScrapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanningAnimeController : ControllerBase
    {
        [HttpGet("Seasonal/All")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery] AnimeSeasonalPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/Many")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery]uint seasonNumber, [FromQuery] AnimeSeasonalPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(seasonNumber,  sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/Many/MultiQueries")]
        public async Task<TanimeSeasonalPlanning[]> SelectAllSeasonalPlanningAsync([FromQuery] ushort year, [FromQuery] FourSeasonsKind season, [FromQuery] AnimeSeasonalPlanningSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
            => await TanimeSeasonalPlanning.SelectAsync(year, season, sortBy, orderBy, limit, skip);

        [HttpGet("Seasonal/LoadFromWeb")]
        public async Task<TanimeSeasonalPlanning[]> SelectseasonalPlanningsync([FromQuery] ushort year, [FromQuery] FourSeasonsKind season)
        {
            if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year || season == FourSeasonsKind.Unknown)
                return [];

            return await TanimeSeasonalPlanning.GetSeasonalPlanningAsync(season, year);
        }

        [HttpPost("Seasonal/LoadFromWebAndSave")]
        public async Task<OperationState> SaveDailyPlanningsync([FromQuery] ushort year, [FromQuery] FourSeasonsKind season)
        {
            if (year < DateOnly.MinValue.Year || year > DateOnly.MaxValue.Year || season == FourSeasonsKind.Unknown)
                return new OperationState(false, "La saison ou l'année n'est pas valide");

            return await TanimeSeasonalPlanning.GetAndInsertSeasonalPlanningAsync(season, year);
        }

        [HttpGet("Daily/Range")]
        public async Task<TanimeDailyPlanning[]> SelectDailyPlanningAsync([FromQuery] string minDate, [FromQuery] string maxDate)
        {
            if (!DateOnly.TryParse(minDate, out var min) || !DateOnly.TryParse(maxDate, out var max))
                return [];

            return await TanimeDailyPlanning.GetAnimePlanningAsync(minDate, maxDate);
        }
    }

    
}
