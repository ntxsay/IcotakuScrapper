using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class StatisticTests
{
    
    [Test]
    public async Task ScrapStatisticAsyncTest()
    {
        var statistic = await TsheetStatistic.ScrapStatisticAsync(1);
        Assert.IsNotNull(statistic);
    }
}