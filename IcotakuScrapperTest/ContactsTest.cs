using IcotakuScrapper.Contact;

namespace IcotakuScrapperTest;

public class ContactsTest
{
    [Test]
    public async Task ScrapContact()
    {
        //var contactUri = new Uri("https://anime.icotaku.com/individu/940/IKEDA-Akihisa/staff.html");
        var contactUri = new Uri("https://anime.icotaku.com/individu/5413/5pb-/staff.html");
        var contact = await Tcontact.ScrapFromUriAsync(contactUri);
        Assert.IsNotNull(contact);
    }
}