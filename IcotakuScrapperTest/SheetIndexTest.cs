using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class SheetIndexTest
{
    [Test]
    public async Task CreateIndex()
    {
        var result = await TsheetIndex.CreateIndexesAsync(IcotakuSection.Anime, SheetType.Anime);
        Assert.That(result.IsSuccess, Is.True);
    }
}