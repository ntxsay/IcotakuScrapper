﻿using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Objects;
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

        [Test]
        public void GetAdvancedSearchUrl()
        {
            var parameter = new AnimeFinderParameterStruct()
            {
                Title = "silver",
                OrigineAdaptation = "manga",
                Year = 2013,
            };
            var url = IcotakuWebHelpers.GetAdvancedSearchUri(IcotakuSection.Anime, parameter);
            Assert.IsNotNull(url);
        }

    }
}
