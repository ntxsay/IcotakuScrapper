using IcotakuScrapper;
using IcotakuScrapper.Services.IOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IcotakuScrapperTest
{
    internal class InputOutputServiceTest
    {

        [Test]
        public void GetSectionPathTest()
        {
            var path = InputOutput.GetSectionPath(IcotakuSection.Anime);
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void GetItemPathTest()
        {
            var path = InputOutput.GetItemPath(IcotakuSection.Anime, Guid.NewGuid());
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void GetSpecifiedPathTest()
        {
            var path = InputOutput.GetSpecifiedPath(IcotakuSection.Anime, "test", "test2", "test3");
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void GetSpecifiedPathTest2()
        {
            var path = InputOutput.GetSpecifiedPath(IcotakuSection.Anime, Guid.NewGuid(), "test", "test2", "test3");
            Assert.IsNotEmpty(path);
        }

        [Test]
        public void GetSpecifiedPathTest3()
        {
            var path = InputOutput.GetSpecifiedPath("test", "test2", "test3");
            Assert.IsNotEmpty(path);
        }
    }
}
