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
        Assert.True(updater.Count > 0);
    }
}