using IcotakuScrapper;
using IcotakuScrapper.Anime;

namespace IcotakuScrapperTest
{
    public class AnimeTest
    {

        public void SetUp()
        {
            //Chemin d'accès complet à la base de données SQLite
            string dbPath = @"C:\MyProject\Data\IcotakuScrapper.db";

            //Indique à l'application où se trouve la base de données
            Main.LoadDatabaseAt(dbPath);
        }

        [Test]
        public async Task GetAnimeFromUrlAsync()
        {
            //Récupère les informations de l'anime via l'url de la fiche
            OperationState<int> animeCreationResult = await Tanime.ScrapFromUrlAsync(new Uri("https://anime.icotaku.com/anime/5633/Dr-STONE.html"));
            
            //Vérifie que l'opération s'est bien déroulée
            Console.WriteLine(animeCreationResult.IsSuccess);

            //Obtient des informations supplémentaires sur l'opération
            Console.WriteLine(animeCreationResult.Message);

            //Obtient l'id (SQLite) de l'anime
            Console.WriteLine(animeCreationResult.Data);

            Assert.That(animeCreationResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task GetAnimeFromIdAsync()
        {
            //Récupère les informations de l'anime précédement "scrapé" via l'url de la fiche
            Tanime? anime = await Tanime.SingleAsync(new Uri("https://anime.icotaku.com/anime/5633/Dr-STONE.html"));

            if (anime is null)
            {
                Console.WriteLine("L'anime n'a pas été trouvé");
                return;
            }

            //Obtient le nom de l'anime
            Console.WriteLine(anime.Name);

            //Obtient le nombre d'épisodes
            Console.WriteLine(anime.EpisodesCount);

            //obtient le synopsis
            Console.WriteLine(anime.Description);
        }
    }
}
