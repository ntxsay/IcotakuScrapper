using IcotakuScrapper;
using IcotakuScrapper.Services;

namespace IcotakuScrapperTest
{
    internal class WebTest
    {
        [Test]
        public void DownloadFileTest()
        {
            var url = IcotakuWebHelpers.GetDownloadFolderUrl(IcotakuSheetType.Anime, 10, IcotakuDownloadType.Episod, 1);
            Assert.IsNotEmpty(url);
        }

        

    }
}
