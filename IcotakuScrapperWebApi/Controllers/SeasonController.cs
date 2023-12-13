using IcotakuScrapper;
using IcotakuScrapper.Common;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeasonController : ControllerBase
{
    // GET
    [HttpPost("Scrap")]
    public async Task<OperationState> ScrapSeasons([FromQuery] IcotakuSection section = IcotakuSection.Anime)
    {
        return await Tseason.ScrapAsync(section);
    }
    
    [HttpGet("All")]
    public async Task<Tseason[]> SelectAllSeasonsAsync([FromQuery] SeasonSortBy sortBy, [FromQuery] OrderBy orderBy = OrderBy.Asc,
        [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        => await Tseason.SelectAsync(sortBy, orderBy, limit, skip);
}