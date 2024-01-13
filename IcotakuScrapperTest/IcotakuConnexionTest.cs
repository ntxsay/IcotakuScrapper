using IcotakuScrapper.Objects;

namespace IcotakuScrapperTest;

public class IcotakuConnexionTest
{
    [Test]
    public async Task GetProfilImages()
    {
        using var connexion = new IcotakuConnexion("userName", "Password");
        if (!await connexion.ConnectAsync())
        {
            Assert.Fail("Impossible de se connecter en tant qu'utilisateur sur le site d'Icotaku");
            return;
        }
        
        var iprofile = connexion.ProfilImageUrl;
        Assert.Pass();
    }
}