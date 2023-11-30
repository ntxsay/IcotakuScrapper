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
        public async Task<TsheetIndex[]> SelectAllAsync([FromQuery] IcotakuSection[] sections, [FromQuery] SheetType[] sheetTypes, [FromQuery] SheetSortBy sortBy = SheetSortBy.Type, [FromQuery] OrderBy orderBy = OrderBy.Asc, [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            var distinctSections = sections.Distinct();
            var distinctSheetTypes = sheetTypes.Distinct();
            return await TsheetIndex.SelectAsync([.. distinctSections], [.. distinctSheetTypes], sortBy, orderBy, limit, skip);
        }

        [HttpPost("Create")]
        public async Task<OperationState> CreateAsync([FromQuery] IcotakuSection section, [FromQuery]SheetType sheetType)
        {
            return await TsheetIndex.CreateIndexesAsync(section, sheetType);
        }
    }
}
