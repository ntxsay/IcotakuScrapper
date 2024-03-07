using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class StatisticTests
{
    
    [Test]
    public async Task ScrapStatisticAsyncTest()
    {
        TsheetStatistic? statistic = await TsheetStatistic.ScrapStatisticAsync(IcotakuSection.Anime ,1);
        if (statistic != null)
        {
            Console.WriteLine(statistic.CreatingDate);
            Console.WriteLine(statistic.CreatedBy);
            Console.WriteLine(statistic.LastUpdatedDate);
            Console.WriteLine(statistic.LastUpdatedBy);
            Console.WriteLine(statistic.InWatchListAverageAge);
            Console.WriteLine(statistic.VisitCount);
            Console.WriteLine(statistic.LastVisitedBy);
        }
        Assert.That(statistic != null);
    }

    [Test]
    public async Task ScrapAndSaveAsyncTest()
    {
        OperationState<int> result = await TsheetStatistic.ScrapAndSaveAsync(IcotakuSection.Anime, 1);
        
        //Etat de l'opération
        Console.WriteLine(result.IsSuccess);
        
        //Information concernant l'état de l'opération
        Console.WriteLine(result.Message);
        
        //Id de l'enregistrement nouvellement inséré
        Console.WriteLine(result.Data);
        Assert.That(result.IsSuccess);
    }
    
    [Test]
    public async Task ScrapStatisticAndGetAsyncTest()
    {
        TsheetStatistic? statistic = await TsheetStatistic.ScrapAndGetAsync(IcotakuSection.Anime ,1);
        if (statistic != null)
        {
            Console.WriteLine(statistic.CreatingDate);
            Console.WriteLine(statistic.CreatedBy);
            Console.WriteLine(statistic.LastUpdatedDate);
            Console.WriteLine(statistic.LastUpdatedBy);
            Console.WriteLine(statistic.InWatchListAverageAge);
            Console.WriteLine(statistic.VisitCount);
            Console.WriteLine(statistic.LastVisitedBy);
        }
        Assert.That(statistic != null);
    }

    [Test]
    public async Task GetSingleAsync()
    {
        TsheetStatistic? result = await TsheetStatistic.SingleAsync(IcotakuSection.Anime, 1);
        Assert.That(result != null);
    }
}