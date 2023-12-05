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
    }
}
