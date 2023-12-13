using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class SeasonTest
{
    [Test]
    public async Task ScrapSeasonAsync()
    {
        var result = await Tseason.ScrapAsync(IcotakuSection.Anime);
        Assert.IsTrue(result.IsSuccess);
    }
}