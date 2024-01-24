using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Objects;
using IcotakuScrapper.Objects.Models;
using IcotakuScrapper.Services;

namespace IcotakuScrapperTest;

public class AnimeFindTest
{
    [Test]
    public async Task AdvancedSearch()
    {
        var parameter = new AnimeFinderParameter
        {
            Title = null,
            OrigineAdaptation = null,
            IncludeGenresId = [9],
            ExcludeGenresId = [18],
            IncludeThemesId = [140],
            ExcludeThemesId = [221]
        };
        
        var animes = await TanimeBase.FindAndSaveAsync(parameter).ToArrayAsync();
        Assert.IsNotEmpty(animes);
    }

    [Test]
    public async Task AdvancedSearchBackgroundWorker()
    {
        var parameter = new AnimeFinderParameter()
        {
            Title = null,
            OrigineAdaptation = null,
            IncludeGenresId = [9],
            ExcludeGenresId = [18],
            IncludeThemesId = [140],
            ExcludeThemesId = [221]
        };

        List<OperationState<TanimeBase?>> animes = new();

        AnimeFinder finder = new();
        finder.ProgressChangedRequested += (percent, state) => { animes.Add(state); };

        finder.OperationCompletedRequested += args => { Console.WriteLine(args.Result); };

        finder.Find(parameter);

        while (finder.IsRunning)
        {
            await Task.Delay(100);
        }

        Assert.IsTrue(animes.Any(x => x.IsSuccess));
    }
    
    [Test]
    public void GetAdvancedSearchUrl()
    {
        var parameter = new AnimeFinderParameter()
        {
            Title = "silver",
            OrigineAdaptation = "manga",
            Year = 2013,
        };
        var url = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, parameter);
        Assert.IsNotNull(url);
    }

}