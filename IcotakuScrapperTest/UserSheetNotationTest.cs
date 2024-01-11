using IcotakuScrapper;
using IcotakuScrapper.Common;
using IcotakuScrapper.Objects;

namespace IcotakuScrapperTest;

public class UserSheetNotationTest
{
    [Test]
    public async Task ScrapAsyncTest()
    {
        using var connexion = new IcotakuConnexion("userName", "passWord");
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }

        var item = await TuserSheetNotation.ScrapAsync(connexion, IcotakuSection.Anime, 556);
        Assert.Pass();
    }
    
    [Test]
    public async Task PostAsyncTest()
    {
        using var connexion = new IcotakuConnexion("userName", "passWord");
        
        //Lance la tentative de connexion
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }
        
        var notation = new TuserSheetNotation
        {
            Section = IcotakuSection.Anime,
            SheetId = 32,
            WatchStatus = WatchStatusKind.Completed,
            Note = 9.5f,
            PublicComment = "Cet animé est génial ! Ou du moins à mes débuts dans les animés, à l'heure actuelle je ne sais pas si je le trouverais toujours aussi génial.",
            PrivateComment = "Test"
        };

        var isSubmitted = await notation.PostNotationAsync(connexion);
        if (!isSubmitted)
            Assert.Fail("Impossible de poster la notation");
        else
            Assert.Pass();
    }
    
    [Test]
    public async Task GoToPageTest()
    {
        using var connexion = new IcotakuConnexion("userName", "passWord");
        
        //Lance la tentative de connexion
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }
        
        //Retourne le contenu HTML de la page d'accueil
        string? htmlString = await connexion.GetHtmlStringAsync(new Uri("https://anime.icotaku.com/"));
    }
    
    [Test]
    public async Task PostTest()
    {
        using var connexion = new IcotakuConnexion("userName", "passWord");
        
        //Lance la tentative de connexion
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }
        
        //Création du dictionnaire de données obligatoire du formulaire
        var formData = new Dictionary<string, string>
        {
            { "input_Name", "input_Value" },
            { "select_Name", "selected_Value" },
            { "textarea_Name", "text_value"},
            //...
        };

        var postResult = await connexion.PostAsync("/post_url", new FormUrlEncodedContent(formData));
        Console.WriteLine(postResult.Message);
        Console.WriteLine(postResult.StatusCode);
        Console.WriteLine(postResult.IsSucces);
    }
    
    [Test]
    public async Task DeconnexionTest()
    {
        using var connexion = new IcotakuConnexion("userName", "passWord");
        
        //Lance la tentative de connexion
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }
        
        //Déconnecte l'utilisateur
        await connexion.DisconnectAsync();
    }
}