using IcotakuScrapper;
using IcotakuScrapper.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormatsController : ControllerBase
    {

        [HttpGet("All")]
        public async Task<Tformat[]> GetAllFormatsAsync([FromQuery] FormatSortBy sortBy, [FromQuery] OrderBy orderBy,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            return await Tformat.SelectAsync(sortBy, orderBy, limit, skip);
        }

        [HttpGet("All/Section")]
        public async Task<Tformat[]> GetAllFormatsAsync([FromQuery] IcotakuSection section, [FromQuery] FormatSortBy sortBy, [FromQuery] OrderBy orderBy,
            [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            return await Tformat.SelectAsync(section, sortBy, orderBy, limit, skip);
        }

        [HttpGet("Scrap")]
        public async Task<OperationState> ScrapAsync([FromQuery] IcotakuSection[] sections)
        {
            var distinctSections = sections.Distinct();
            return await Tformat.ScrapAsync([.. distinctSections]);
        }
    }
}
