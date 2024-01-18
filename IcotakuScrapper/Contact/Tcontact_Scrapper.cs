using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using System.Globalization;
using System.Web;

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
        var htmlDocument = await web.LoadFromWebAsync(sheetUri.OriginalString);

        return contactType switch
        {
            ContactType.Person => await ScrapIndividual(sheetUri, htmlDocument.DocumentNode, section.Value),
            ContactType.Distributor => await ScrapDistributor(sheetUri, htmlDocument.DocumentNode, section.Value),

            _ => null
        };
    }

    /// <summary>
    /// Scrappe la fiche d'un individu
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="documentNode"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    private static async Task<Tcontact?> ScrapIndividual(Uri sheetUri, HtmlNode documentNode, IcotakuSection section)
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

        //Genre
        TcontactGenre? contactGenre = null;
        var genreNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Genre :')]/following-sibling::text()[1]");
        if (genreNode != null)
            contactGenre = await ScapGenreAsync(genreNode);

        //Date de naissance
        string? birthDate = null;
        var birthDateNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Date de naissance :')]/following-sibling::text()[1]");
        if (birthDateNode != null)
            birthDate = ScrapBirthDate(birthDateNode);

        //Âge
        uint? age = null;
        var ageNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Âge :')]/following-sibling::text()[1]");
        if (ageNode != null)
            age = ScrapAge(ageNode);

        //Présentation
        var presentationNode = documentNode.SelectSingleNode("//div[contains(@class, 'fiche-mini')]//h2[starts-with(text(), 'Présentation')]/following-sibling::p[1]");
        var presentation = HttpUtility.HtmlDecode(presentationNode?.InnerText.Trim());

        //Sites web
        IEnumerable<TcontactWebSite> webSites = [];
        var websiteNode = documentNode.SelectNodes("//div[contains(@class, 'fiche-mini')]//b[starts-with(text(), 'Site officiel :')]/parent::div/a")?.ToArray();
        if (websiteNode != null)
            webSites = ScrapWebsites(websiteNode);

        Tcontact? contact = new()
        {
            SheetId = sheetId,
            Type = ContactType.Person,
            DisplayName = displayName,
            Genre = contactGenre,
            BirthName = nom,
            FirstName = prenom,
            OriginalName = originalName,
            BirthDate = birthDate,
            Age = age,
            Url = sheetUri.ToString(),
            ThumbnailUrl = ScrapFullThumbnail(documentNode, ContactType.Person, section),
            Presentation = presentation,
        };

        foreach (var website in webSites)
            contact.WebSites.Add(website);

        contact = await SingleOrCreateAsync(contact);
        return contact;
    }

    /// <summary>
    /// Scrappe la fiche d'un individu
    /// </summary>
    /// <param name="sheetUri"></param>
    /// <param name="documentNode"></param>
    /// <param name="section"></param>
    /// <returns></returns>
    private static async Task<Tcontact?> ScrapDistributor(Uri sheetUri, HtmlNode documentNode, IcotakuSection section)
    {
        var sheetId = IcotakuWebHelpers.GetSheetId(sheetUri);
        if (sheetId == 0)
            return null;

        //Nom
        var nameNode = documentNode.SelectSingleNode("//div[@id='fiche_entete']/div[1]/h1");
        var name = HttpUtility.HtmlDecode(nameNode?.InnerText.Trim());
        if (name == null || name.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        //Site officiel
        var siteOfficialNode = documentNode.SelectSingleNode("//span[@id='site_officiel_editeur']/a");
        var siteOfficial = WebServices.GetHrefFromElement(siteOfficialNode);

        //Présentation
        var presentationNode = documentNode.SelectSingleNode("//div[contains(@class, 'contenu')]//h2[starts-with(text(), 'Présentation')]/following-sibling::p[1]");
        var presentation = HttpUtility.HtmlDecode(presentationNode?.InnerText.Trim());

        //Sites web
        List<TcontactWebSite> webSites = [];
        if (siteOfficial != null)
            webSites.Add(new TcontactWebSite { Url = siteOfficial });

        Tcontact? contact = new()
        {
            SheetId = sheetId,
            Type = ContactType.Distributor,
            DisplayName = name,
            Url = sheetUri.ToString(),
            Presentation = presentation,
        };

        foreach (var website in webSites)
            contact.WebSites.Add(website);

        contact = await SingleOrCreateAsync(contact);
        return contact;
    }

    /// <summary>
    /// Scrappe le genre du contact
    /// </summary>
    /// <param name="genreNode"></param>
    /// <returns></returns>
    private static async Task<TcontactGenre?> ScapGenreAsync(HtmlNode genreNode)
    {
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

        return contactGenre;
    }

    /// <summary>
    /// Scrappe la date de naissance du contact
    /// </summary>
    /// <param name="birthDateNode"></param>
    /// <returns></returns>
    private static string? ScrapBirthDate(HtmlNode birthDateNode)
    {
        var birthDate = HttpUtility.HtmlDecode(birthDateNode?.InnerText.Trim());
        if (birthDate == "?" || birthDate.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (DateOnly.TryParseExact(birthDate, "dd MMMM yyyy", new CultureInfo("fr-FR"),
                               DateTimeStyles.None, out var date))
            return date.ToString("yyyy-MM-dd");

        return null;
    }

    /// <summary>
    /// Scrappe l'âge du contact
    /// </summary>
    /// <param name="ageNode"></param>
    /// <returns></returns>
    private static uint? ScrapAge(HtmlNode ageNode)
    {
        var age = HttpUtility.HtmlDecode(ageNode?.InnerText.Trim());
        if (age == "?" || age.IsStringNullOrEmptyOrWhiteSpace())
            return null;

        if (uint.TryParse(age, out var intAge))
            return intAge;

        return null;
    }

    private static IEnumerable<TcontactWebSite> ScrapWebsites(IEnumerable<HtmlNode> websiteNodes)
    {
        var websites = websiteNodes?.Where(w => w.Attributes["href"] != null && !w.Attributes["href"].Value.IsStringNullOrEmptyOrWhiteSpace()).Select(node => node.Attributes["href"].Value).ToArray() ?? [];
        if (websites.Length == 0)
            return [];
        return websites.Select(website => new TcontactWebSite { Url = website });
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