using IcotakuScrapper;
using IcotakuScrapper.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.ComponentModel;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        [HttpPost("Create/Index")]
        public async Task<OperationState> CreateIndexAsync(IcotakuSection[] sections)
        {
            var distinctSections = sections.Distinct();
            return await Tcategory.CreateIndexAsync([.. distinctSections]);
        }

        [HttpGet(Name = "all")]

        public async Task<Tcategory[]> SelectAllAsync([FromQuery] IcotakuSection[] sections, [FromQuery] CategoryType[] categoryType, [FromQuery] FormatSortBy sortBy = FormatSortBy.Name,
        [FromQuery] OrderBy orderBy = OrderBy.Asc,
        [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            var distinctSections = sections.Distinct();
            var distinctCategoryType = categoryType.Distinct();
            return await Tcategory.SelectAsync([.. distinctSections], [.. distinctCategoryType], sortBy, orderBy, limit, skip);
        }

        [HttpGet("Single/Id/{id}")]
        public async Task<Tcategory?> SelectByIdAsync([FromRoute] uint id) 
            => await Tcategory.SingleAsync((int)id);

        [HttpGet("Single/Name")]
        public async Task<Tcategory?> SelectByNameAsync([FromQuery] IcotakuSection section, [FromQuery] CategoryType categoryType, [FromQuery] string name)
            => await Tcategory.SingleAsync(name, section, categoryType);

        [HttpGet("Single/Url")]
        public async Task<Tcategory?> SelectByUrlAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;
            return await Tcategory.SingleAsync(uri);
        }
    }
}
