using System.Globalization;
using System.Web;
using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Contact;

public partial class Tcontact
{
    public static async Task<Tcontact?> ScrapFromUriAsync(Uri sheetUri)
    {
        var contactType = IcotakuWebHelpers.GetContactType(sheetUri);
        if (contactType is null or ContactType.Unknown)
            return null;
        
        var section = IcotakuWebHelpers.GetIcotakuSection(sheetUri);
        if (section is null)
            return null;
        
        HtmlWeb web = new();
        var htmlDocument = web.Load(sheetUri.OriginalString);
        
        return contactType switch
        {
            ContactType.Person => await ScrapIndividual(sheetUri, htmlDocument.DocumentNode, section.Value),
            
            _ => null
        };
    }

    internal static async Task<Tcontact?> ScrapIndividual(Uri sheetUri, HtmlNode documentNode, IcotakuSection section)
    {
        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
        if (sheetId == 0)
            return null;
        
        var displayNameNode = documentNode.SelectSingleNode("//div[@id='fiche_entete']/div[1]/h1");
        var displayName = HttpUtility.HtmlDecode(displayNameNode?.InnerText.Trim());
        if (displayName == null || displayName.IsStringNullOrEmptyOrWhiteSpace())
            return null;
        
        var nomNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Nom :')]/following-sibling::text()[1]");
        var nom = HttpUtility.HtmlDecode(nomNode?.InnerText.Trim());
        if (nom == "?") nom = null;
        
        var prenomNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Prénom :')]/following-sibling::text()[1]");
        var prenom = HttpUtility.HtmlDecode(prenomNode?.InnerText.Trim());
        if (prenom == "?") prenom = null;
        
        var originalNameNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Nom original :')]/following-sibling::text()[1]");
        var originalName = HttpUtility.HtmlDecode(originalNameNode?.InnerText.Trim());
        if (originalName == "?") originalName = null;
        
        var genreNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Genre :')]/following-sibling::text()[1]");
        var genre = HttpUtility.HtmlDecode(genreNode?.InnerText.Trim());
        if (genre == "?") genre = null;
        
        TcontactGenre? contactGenre = null;
        if (genre != null && !genre.IsStringNullOrEmptyOrWhiteSpace())
        {
            contactGenre = new TcontactGenre()
            {
                Name = genre,
            };
            
            contactGenre = await TcontactGenre.SingleOrCreateAsync(contactGenre);
        }
        
        var birthDateNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Date de naissance :')]/following-sibling::text()[1]");
        var birthDate = HttpUtility.HtmlDecode(birthDateNode?.InnerText.Trim());
        if (birthDate == "?") birthDate = null;
        
        DateOnly date =  default;
        var hasBirthDate = birthDate != null && !birthDate.IsStringEmptyOrWhiteSpace() &&
                           DateOnly.TryParseExact(birthDate, "dd MMMM yyyy", new CultureInfo("fr-FR"),
                               DateTimeStyles.None, out date);
        
        var ageNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Âge :')]/following-sibling::text()[1]");
        var age = HttpUtility.HtmlDecode(ageNode?.InnerText.Trim());
        if (age == "?") age = null;
        
        uint intAge = 0;
        var hasAge = age != null && !age.IsStringEmptyOrWhiteSpace() && uint.TryParse(age, out intAge);
        
        var presentationNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//h2[starts-with(text(), 'Présentation')]/following-sibling::p[1]");
        var presentation = HttpUtility.HtmlDecode(presentationNode?.InnerText.Trim());
        
        var websiteNode = documentNode.SelectNodes("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Site officiel :')]/parent::div/a")?.ToArray();
        var websites = websiteNode?.Where(w => w.Attributes["href"] != null && !w.Attributes["href"].Value.IsStringNullOrEmptyOrWhiteSpace()).Select(node => node.Attributes["href"].Value).ToArray() ?? Array.Empty<string>();
        List<TcontactWebSite> contactWebSites = [];
        if (websites.Length > 0)
            contactWebSites.AddRange(websites.Select(website => new TcontactWebSite { Url = website }));

        Tcontact? contact = new()
        {
            SheetId = sheetId,
            Type = ContactType.Person,
            DisplayName = displayName,
            Genre = contactGenre,
            BirthName = nom,
            FirstName = prenom,
            OriginalName = originalName,
            BirthDate = hasBirthDate ? date.ToString("yyyy-MM-dd") : null,
            Age = hasAge ? intAge : null,
            Url = sheetUri.ToString(),
            ThumbnailUrl = ScrapFullThumbnail(documentNode, ContactType.Person, section),
            Presentation = presentation,
        };
        
        if (contactWebSites.Count > 0)
            foreach (var website in contactWebSites)
                contact.WebSites.Add(website);
        
        contact = await SingleOrCreateAsync(contact);
        return contact;
    }
    
    internal static string? ScrapFullThumbnail(HtmlNode documentNode, ContactType contactType, IcotakuSection section)
    {
        var srcStartWith = IcotakuWebHelpers.GetStartImgSrcByContactType(contactType);
        if (srcStartWith == null)
            return null;
        
        //Récupère le noeud de l'image de la vignette
        var imgNode = documentNode.SelectSingleNode($"//div[contains(@class, 'contenu')]/div[2]/img[contains(@src, '{srcStartWith}')]");

        //Récupère l'url de l'image de la vignette
        var src = imgNode?.Attributes["src"]?.Value;

        //Si l'url est valide, on retourne l'url de l'image de la vignette
        if (src == null)
            return null;

        //Sinon on retourne null
        var uri = IcotakuWebHelpers.GetImageFromSrc(section, src);
        return uri?.ToString();
    }

    internal static string? ScrapFullThumbnail(Uri contactUri)
    {
        HtmlWeb web = new();
        var htmlDocument = web.Load(contactUri.ToString());

        var documentNode = htmlDocument.DocumentNode;

        var contactType = IcotakuWebHelpers.GetContactType(contactUri);
        if (contactType is null or ContactType.Unknown)
            return null;

        var section = IcotakuWebHelpers.GetIcotakuSection(contactUri);
        if (section is null)
            return null;

        return ScrapFullThumbnail(documentNode, (ContactType)contactType, (IcotakuSection)section);
    }
}