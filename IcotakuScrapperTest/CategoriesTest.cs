using IcotakuScrapper;
using IcotakuScrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IcotakuScrapperTest
{
    public class CategoriesTest
    {

        [Test]
        public async Task CreateIndexAsync()
        {
            var sections = new HashSet<IcotakuSection>
            {
                IcotakuSection.Anime,
                IcotakuSection.Manga,
            };

            var result = await Tcategory.CreateIndexAsync(sections);
            Assert.That(result.IsSuccess, Is.True);
        }
    }
}
