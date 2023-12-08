using IcotakuScrapper;
using IcotakuScrapper.Services;

namespace IcotakuScrapperTest
{
    internal class WebTest
    {
        [Test]
        public void DownloadFileTest()
        {
            var url = IcotakuWebHelpers.GetDownloadFolderUrl(IcotakuSheetType.Anime, 10, IcotakuDefaultSubFolder.Episod, 1);
            Assert.IsNotEmpty(url);
        }

        

    }
}
