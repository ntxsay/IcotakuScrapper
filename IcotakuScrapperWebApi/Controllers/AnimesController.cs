using IcotakuScrapper;
using IcotakuScrapper.Anime;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimesController : ControllerBase
    {
        [HttpGet("All")]
        public async Task<Tanime[]> SelectAllAsync([FromQuery] bool? isAdultContent, [FromQuery] bool? isExplicitContent, [FromQuery] AnimeSortBy sortBy = AnimeSortBy.Name,
                   [FromQuery] OrderBy orderBy = OrderBy.Asc,
                          [FromQuery] uint limit = 0, [FromQuery] uint skip = 0)
        {
            return await Tanime.SelectAsync(isAdultContent, isExplicitContent, sortBy, orderBy, limit, skip);
        }

        [HttpGet("Single/Id/{id}")]
        public async Task<Tanime?> SelectByIdAsync([FromRoute] uint id)
            => await Tanime.SingleAsync((int)id, SheetIntColumnSelect.Id);

        [HttpGet("Single/SheetId/{id}")]
        public async Task<Tanime?> SelectBySheetIdAsync([FromRoute] uint id)
            => await Tanime.SingleAsync((int)id, SheetIntColumnSelect.SheetId);

        [HttpGet("Single/Name")]
        public async Task<Tanime?> SelectByNameAsync([FromQuery] string name)
            => await Tanime.SingleAsync(name);

        [HttpGet("Single/Url")]
        public async Task<Tanime?> SelectByUrlAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return null;
            return await Tanime.SingleAsync(uri);
        }

        [HttpPost("Create/Url")]
        public async Task<OperationState<int>> CreateByUrlAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return new OperationState<int>(false, "L'url n'est pas valide");
            return await Tanime.ScrapAnimeFromUrl(uri.ToString());
        }


        [HttpDelete("Delete/Id")]
        public async Task<OperationState> DeleteByIdAsync([FromQuery] uint id)
            => await Tanime.DeleteAsync((int)id, SheetIntColumnSelect.Id);

        [HttpDelete("Delete/SheetId")]
        public async Task<OperationState> DeleteBySheetIdAsync([FromQuery] uint id)
           => await Tanime.DeleteAsync((int)id, SheetIntColumnSelect.SheetId);

        [HttpDelete("Delete/Url")]
        public async Task<OperationState> DeleteBySheetIdAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return new OperationState(false, "L'url n'est pas valide");
            return await Tanime.DeleteAsync(uri);
        }
    }
}
