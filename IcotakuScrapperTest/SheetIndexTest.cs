using IcotakuScrapper;
using IcotakuScrapper.Common;

namespace IcotakuScrapperTest;

public class SheetIndexTest
{
    [Test]
    public async Task CreateIndex()
    {
        var result = await TsheetIndex.CreateIndexesAsync(IcotakuSection.Anime, IcotakuSheetType.Anime);
        Assert.That(result.IsSuccess, Is.True);
    }
}