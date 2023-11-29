using IcotakuScrapper;
using IcotakuScrapper.Common;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SheetsIndexController : ControllerBase
    {
        [HttpGet("All")]
        public async Task<TsheetIndex[]> SelectAllAsync([FromQuery] IcotakuSection[] sections, [FromQuery] SheetSortBy sortBy = SheetSortBy.Type, [FromQuery] OrderBy orderBy = OrderBy.Asc, [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            var distinctSections = sections.Distinct();
            return await TsheetIndex.SelectAsync([.. distinctSections], sortBy, orderBy, limit, skip);
        }

        [HttpPost("Create")]
        public async Task<OperationState> CreateAsync([FromQuery] IcotakuSection section)
        {
            return await TsheetIndex.CreateIndexesAsync(section);
        }
    }
}
