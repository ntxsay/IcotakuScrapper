using IcotakuScrapper.Objects;
using IcotakuScrapper.Objects.Models;

namespace IcotakuScrapper.Anime;

public partial class TanimeBase
{
    public static async IAsyncEnumerable<OperationState<int>> FindAndSaveAsync(AnimeFinderParameter finderParameter, AnimeScrapingOptions options = AnimeScrapingOptions.All, CancellationToken? cancellationToken = null)
    {
        var animes = await FindAsync(finderParameter, options, cancellationToken);
        if (animes.Length == 0)
            yield break;
        
        foreach (var anime in animes)
        {
            var fullAnime = anime.ToFullAnime();
            yield return await fullAnime.AddOrUpdateAsync(cancellationToken);
        }
    }

    public static async IAsyncEnumerable<Tanime> FindAndGetAsync(
        AnimeFinderParameter finderParameter, AnimeScrapingOptions options = AnimeScrapingOptions.All,
        CancellationToken? cancellationToken = null)
    {
        var results = await FindAndSaveAsync(finderParameter, options, cancellationToken).ToArrayAsync();
        results = results.Where(x => x is { IsSuccess: true, Data: > 0 }).ToArray();
        
        foreach (var result in results)
        {
            var anime = await Tanime.SingleByIdAsync(result.Data);
            if (anime is null)
                continue;

            yield return anime;
        }
    }


    public static async Task<TanimeBase[]> FindAsync(AnimeFinderParameter finderParameter, AnimeScrapingOptions options = AnimeScrapingOptions.All, CancellationToken? cancellationToken = null)
        => await ScrapAnimeSearchAsync(finderParameter, options, cancellationToken).ToArrayAsync();
}