using IcotakuScrapper;
using IcotakuScrapper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IcotakuScrapperTest
{
    internal class WebTest
    {
        [Test]
        public void DownloadFileTest()
        {
            var url = IcotakuWebHelpers.GetDownloadFolderUrl(IcotakuDownloadSection.Anime, 10, IcotakuDownloadType.Episod, 1);
            Assert.IsNotEmpty(url);
        }

        [Test]
        public async Task GetRestrictedHtmlAsync()
        {
            var html = await IcotakuWebHelpers.GetRestrictedHtmlAsync(IcotakuSection.Anime, new Uri("https://anime.icotaku.com/anime/3529/JK-to-Ero-Giin-Sensei.html"), "ntxsay", "Sayonala2");
            Assert.IsNotEmpty(html);
        }
    }
}
