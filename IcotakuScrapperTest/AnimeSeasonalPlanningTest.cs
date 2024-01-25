using IcotakuScrapper;
using IcotakuScrapper.Anime;
using IcotakuScrapper.Objects;

namespace IcotakuScrapperTest;

public class AnimeSeasonalPlanningTest
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
    public async Task ScrapSeasonalPlanningTest()
    {
        var season = new WeatherSeason(WeatherSeasonKind.Spring, 2014);
        var seasonalPlanning =
            await TanimeSeasonalPlanning.ScrapAnimeBaseSheetAsync(season).ToArrayAsync();
        if (seasonalPlanning.Length == 0)
        {
            Assert.Fail($"La liste des animes de la saison {season} est vide");
            return;
        }

        foreach (var animeBase in seasonalPlanning)
        {
            Console.WriteLine(animeBase);
        }
        
        Assert.Pass($"{seasonalPlanning.Length} animés de la saison {season} a été scrapée avec succès");
    }
    
    [Test]
    public async Task SaveSeasonalPlanningTest()
    {
        var season = new WeatherSeason(WeatherSeasonKind.Spring, 2014);
        var result =
            await TanimeSeasonalPlanning.ScrapAndSaveAsync(season);
        if (!result.IsSuccess)
        {
            Assert.Fail(result.Message);
            return;
        }
        
        Assert.Pass(result.Message);
    }
}