﻿using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Services;
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
            => await Tanime.SingleAsync((int)id, IntColumnSelect.Id);

        [HttpGet("Single/SheetId/{id}")]
        public async Task<Tanime?> SelectBySheetIdAsync([FromRoute] uint id)
            => await Tanime.SingleAsync((int)id, IntColumnSelect.SheetId);

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
            return await Tanime.ScrapFromUrlAsync(uri);
        }

        [HttpPost("Create/Restricted/Url")]
        public async Task<OperationState<int>> CreateRestrictedByUrlAsync([FromQuery] string url, [FromQuery] string username, [FromQuery] string password)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return new OperationState<int>(false, "L'url n'est pas valide");
            return await Tanime.ScrapFromUrlAsync(uri, username, password);
        }


        [HttpDelete("Delete/Id")]
        public async Task<OperationState> DeleteByIdAsync([FromQuery] uint id)
            => await TanimeBase.DeleteAsync((int)id, IntColumnSelect.Id);

        [HttpDelete("Delete/SheetId")]
        public async Task<OperationState> DeleteBySheetIdAsync([FromQuery] uint id)
           => await TanimeBase.DeleteAsync((int)id, IntColumnSelect.SheetId);

        [HttpDelete("Delete/Url")]
        public async Task<OperationState> DeleteBySheetIdAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return new OperationState(false, "L'url n'est pas valide");
            return await TanimeBase.DeleteAsync(uri);
        }

        [HttpGet("Download/Path/Url")]
        public async Task<IActionResult> GetDownloadedFolderAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return NotFound("L'url n'est pas valide.");

            return Ok(await TanimeBase.GetFolderPathAsync(uri));
        }

        [HttpPost("Download/Folder/Url")]
        public async Task<IActionResult> DownloadAsync([FromQuery] string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return NotFound("L'url n'est pas valide.");

            return Ok(await TanimeBase.DownloadFolderAsync(uri));
        }
    }
}
