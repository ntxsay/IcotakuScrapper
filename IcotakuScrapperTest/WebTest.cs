using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Extensions;
using IcotakuScrapper.Objects;
using IcotakuScrapper.Objects.Models;
using IcotakuScrapper.Services;

namespace IcotakuScrapperTest
{
    internal class WebTest
    {
        [Test]
        public void DownloadFileTest()
        {
            var url = IcotakuWebHelpers.GetDownloadFolderUrl(IcotakuSheetType.Anime, 10, IcotakuDefaultSubFolder.Episod, 1);
            Assert.That(!url.IsStringNullOrEmptyOrWhiteSpace());
        }

        
    }
}
