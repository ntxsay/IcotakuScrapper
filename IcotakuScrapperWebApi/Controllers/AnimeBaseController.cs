using IcotakuScrapper;
using IcotakuScrapper.Anime;
using Microsoft.AspNetCore.Mvc;

namespace IcotakuScrapperWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnimeBaseController : ControllerBase
{
    [HttpGet("Single/Id/{id}")]
    public async Task<TanimeBase?> SelectByIdAsync([FromRoute] uint id)
        => await TanimeBase.SingleAsync((int)id, IntColumnSelect.Id);
    
    [HttpPost("Scrap/Url")]
    public async Task<OperationState<int>> CreateByUrlAsync([FromQuery] string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new OperationState<int>(false, "L'url n'est pas valide");
        return await Tanime.ScrapFromUrlAsync(uri, AnimeScrapingOptions.Default);
    }
}