using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Web;

namespace IcotakuScrapper.Contact
{
    public partial class TcontactBase
    {
        internal static async Task<TcontactBase?> ScrapContactBase(HtmlNode contactlinkNode, bool scrapFull = false,
            CancellationToken? cancellationToken = null, SqliteCommand? cmd = null)
        {
            if (contactlinkNode is null)
                return null;

            var contactHref = contactlinkNode.Attributes["href"]?.Value;
            if (contactHref == null || contactHref.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            var contactUri = IcotakuWebHelpers.GetFullHrefFromHtmlNode(contactlinkNode, IcotakuSection.Anime);
            if (contactUri == null)
                return null;

            var displayName = HttpUtility.HtmlDecode(contactlinkNode.InnerText?.Trim());
            if (displayName == null || displayName.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            //Récupère l'id de la fiche du thème ou du genre s'il existe en base de données
            Tcontact? contact = await Tcontact.SingleAsync(contactUri, cancellationToken, cmd);
            if (contact != null)
                return contact;

            //Si on ne scrappe pas la fiche du thème ou du genre depuis sa fiche via son url, on insère la catégorie dans la base de données depuis la fiche anime
            if (!scrapFull)
            {
                var sheetId = IcotakuWebHelpers.GetSheetId(contactUri);
                if (sheetId < 0)
                    return null;

                var contactType = IcotakuWebHelpers.GetContactType(contactUri);
                if (contactType == null)
                    return null;

                contact = new Tcontact()
                {
                    SheetId = sheetId,
                    Type = (ContactType)contactType,
                    DisplayName = displayName,
                    Url = contactUri.ToString(),
                    ThumbnailUrl = Tcontact.ScrapFullThumbnail(contactUri),
                    Presentation = null,
                };

                //Et insère la fiche dans la base de données
                var insertionState2 = await contact.InsertAync(cancellationToken, cmd);
                if (insertionState2.IsSuccess)
                {
                    return contact;
                }
            }

            contact = await Tcontact.ScrapFromUriAsync(contactUri);
            if (contact == null)
                return null;

            contact = await Tcontact.SingleOrCreateAsync(contact, false, cancellationToken, cmd);
            if (contact == null)
                return null;

            return contact;
        }

    }
}
