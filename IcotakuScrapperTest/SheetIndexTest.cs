using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class SheetIndexTest
{
    [Test]
    public async Task CreateIndex()
    {
        var result = await TsheetIndex.CreateIndexesAsync(IcotakuContentType.Anime);
        Assert.That(result.IsSuccess, Is.True);
    }
}