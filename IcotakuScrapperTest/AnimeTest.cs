using IcotakuScrapper;
using IcotakuScrapper.Anime;

namespace IcotakuScrapperTest
{
    public class AnimeTest
    {
        [SetUp]
        public void SetUp()
        {
            /*Main.LoadDatabaseAt(@"C:\Datas\icotaku.db");

            

            //Initialise le dossier de travail
            Main.LoadWorkingDirectoryAt(@"C:\Datas\icotaku");*/
            
            //Initialise la connexion à la base de données SQLite
            Main.InitializeDbConnectionString();

            //Interdit l'accès au contenu adulte au sein de l'application
            Main.IsAccessingToAdultContent = false;

            //Autorise l'accès au contenu explicite au sein de l'application
            Main.IsAccessingToExplicitContent = true;
        }

        [Test]
        public async Task GetAnimeFromUrlAsync()
        {
            //Récupère les informations de l'anime via l'url de la fiche
            OperationState<int> animeCreationResult =
                await Tanime.ScrapFromUrlAsync(new Uri("https://anime.icotaku.com/anime/5633/Dr-STONE.html"));

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
            OperationState<int> animeCreationResult = await Tanime.ScrapFromSheetIdAsync(5633);

            //Vérifie que l'opération s'est bien déroulée
            Console.WriteLine(animeCreationResult.IsSuccess);

            //Obtient des informations supplémentaires sur l'opération
            Console.WriteLine(animeCreationResult.Message);

            //Obtient l'id (SQLite) de l'anime
            Console.WriteLine(animeCreationResult.Data);

            Assert.That(animeCreationResult.IsSuccess, Is.True);
        }

        [Test]
        public async Task GetSingleAnimeByUrlAsync()
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

        [Test]
        public async Task GetSingleAnimeBySheetIdAsync()
        {
            //Récupère les informations de l'anime précédement "scrapé" via l'url de la fiche
            Tanime? anime = await Tanime.SingleBySheetIdAsync(5633);

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
        
        [Test]
        public async Task GetSingleAnimeByIdAsync()
        {
            //Récupère les informations de l'anime précédement "scrapé"
            Tanime? anime = await Tanime.SingleByIdAsync(1);

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
        
        [Test]
        public async Task GetSingleAnimeByNameAsync()
        {
            //Récupère les informations de l'anime précédement "scrapé"
            Tanime? anime = await Tanime.SingleAsync("Dr.STONE");

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

        [Test]
        public async Task CountAnimesAsync()
        {
            //Compte le nombre de fiches
            int countAnimes = await TanimeBase.CountAsync();
            
            //Affiche le nombre de fiches
            Console.WriteLine(countAnimes.ToString());
        }
        
        [Test]
        public async static Task IsAnimeExistsAsync()
        {
            //Vérifie si l'anime existe
            bool isExists = await TanimeBase.ExistsByIdAsync(1);
            
            //Affiche le résultat
            Console.WriteLine(isExists.ToString());
        }
        
        [Test]
        public async static Task IsAnimeExistsBySheetIdAsync()
        {
            //Vérifie si l'anime existe
            bool isExists = await TanimeBase.ExistsBySheetIdAsync(1);
            
            //Affiche le résultat
            Console.WriteLine(isExists.ToString());
        }
        
        [Test]
        public async static Task IsAnimeExistsByNameAsync()
        {
            //Vérifie si l'anime existe
            bool isExists = await TanimeBase.ExistsAsync("Dr.STONE");
            
            //Affiche le résultat
            Console.WriteLine(isExists.ToString());
        }

        [Test]
        public async Task LoadThumbnailPath()
        {
            var anime = await TanimeBase.SingleAsync(new Uri(
                "https://anime.icotaku.com/anime/7389/Bokensha-ni-Naritai-to-Miyako-ni-Deteitta-Musume-ga-S-rank-ni-Natteta.html"));
            if (anime is null)
                return;
            var path = await anime.DownloadThumbnailAsync();
            Assert.IsNotEmpty(path);
        }
        
        [Test]
        public async Task AdvancedSearch()
        {
            var parameter = new AnimeFinderParameterStruct()
            {
                Title = null,
                OrigineAdaptation = null,
                IncludeGenresId = [9],
                ExcludeGenresId = [18],
                IncludeThemesId = [140],
                ExcludeThemesId = [221]
            };
            var animes = await TanimeBase.FindAsync(parameter);
            Assert.IsNotEmpty(animes);
        }
    }
}