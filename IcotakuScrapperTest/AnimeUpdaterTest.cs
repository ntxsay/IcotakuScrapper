using IcotakuScrapper;
using IcotakuScrapper.Objects;

namespace IcotakuScrapperTest;

public class AnimeUpdaterTest
{
    [Test]
    public async Task FindAnimeToUpdateAsyncTest()
    {
        var updater = new AnimeUpdater();
        await updater.FindAnimeToUpdateAsync(new WeatherSeason(WeatherSeasonKind.Fall, 2010));
        Assert.That(updater.Count, Is.GreaterThan(0));
    }
    
    [Test]
    public async Task UpdateAnimeAsyncTest()
    {
        var downloader = new AnimeDownloader();
        
    }
}