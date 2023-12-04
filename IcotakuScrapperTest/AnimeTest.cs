using IcotakuScrapper;
using IcotakuScrapper.Anime;

namespace IcotakuScrapperTest
{
    public class AnimeTest
    {

        [Test]
        public async Task GetAnimeFromUrlAsync()
        {
            OperationState<int> animeCreationResult = await Tanime.ScrapAnimeFromUrl("https://anime.icotaku.com/anime/5633/Dr-STONE.html");
            Assert.That(animeCreationResult.IsSuccess, Is.True);
        }
    }
}
