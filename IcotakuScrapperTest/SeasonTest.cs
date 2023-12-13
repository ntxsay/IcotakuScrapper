using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class SeasonTest
{
    [Test]
    public void ScrapSeason()
    {
        var d = Tseason.ScrapSeasons(IcotakuSection.Anime).ToArray();
        Assert.IsNotEmpty(d);
    }
}