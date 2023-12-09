using IcotakuScrapper.Services;

namespace IcotakuScrapperTest;

public class IcotakuHelpersTest
{
    [Test]
    public void TestContactTypeFromUri()
    {
        var characterUri = new Uri("https://anime.icotaku.com/personnage/7766/MIZUHARA-Akane.html");
        var personUri = new Uri("https://anime.icotaku.com/individu/216/HASEGAWA-Katsumi.html");
        var studioUri = new Uri("https://anime.icotaku.com/studio/168/Silver.html");
        var distributorUri = new Uri("https://anime.icotaku.com/editeur/155/Crunchyroll.html");
        
        var characterType = IcotakuWebHelpers.GetContactType(characterUri);
        var personType = IcotakuWebHelpers.GetContactType(personUri);
        var studioType = IcotakuWebHelpers.GetContactType(studioUri);
        var distributorType = IcotakuWebHelpers.GetContactType(distributorUri);
        Assert.Pass();
    }
}