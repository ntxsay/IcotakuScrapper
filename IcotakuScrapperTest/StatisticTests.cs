using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class StatisticTests
{
    
    [Test]
    public async Task ScrapStatisticAsyncTest()
    {
        var statistic = await TsheetStatistic.ScrapStatisticAsync(IcotakuSection.Anime ,1);
        Assert.IsNotNull(statistic);
    }

    [Test]
    public async Task ScrapAndSaveAsyncTest()
    {
        OperationState<int> result = await TsheetStatistic.ScrapAndSaveAsync(IcotakuSection.Anime, 1);
        Assert.IsTrue(result.IsSuccess);
    }

    [Test]
    public async Task GetSingleAsync()
    {
        TsheetStatistic? result = await TsheetStatistic.SingleAsync(IcotakuSection.Anime, 1);
        Assert.IsNotNull(result);
    }
}