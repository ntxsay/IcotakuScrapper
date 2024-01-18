using HtmlAgilityPack;
using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Services
{
    internal static class WebServices
    {
        /// <summary>
        /// Télécharge le fichier dans le dossier spécifié puis retourne son chemin d'accès local complet
        /// </summary>
        /// <param name="fileUri"></param>
        /// <param name="localPath">CHemin d'accès local relatif</param>
        /// <returns></returns>
        public static async Task<bool> DownloadFileAsync(Uri fileUri, string destinationFile, CancellationToken cancellationToken)
        {
            try
            {
                using HttpClient client = new();

                using HttpResponseMessage response = await client.GetAsync(fileUri, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    LogServices.LogDebug("Erreur lors du téléchargement du fichier : " + response.StatusCode);
                    return false;
                }

                // Récupérer le type de contenu (Content-Type)
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType == null || contentType.IsStringNullOrEmptyOrWhiteSpace())
                {
                    LogServices.LogDebug("Le type de ce contenu n'est pas attendu : " + contentType);
                    return false;
                }

                var extension = MimeTypes.GetMimeTypeExtensions(contentType).FirstOrDefault();
                if (extension == null || extension.IsStringNullOrEmptyOrWhiteSpace())
                {
                    LogServices.LogDebug("Impossible de déterminer l'extension du fichier");
                    return false;
                }

                await using (Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                {
                    await using FileStream fileStream = File.Create(destinationFile);
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                Console.WriteLine("Téléchargement terminé !");
                return true;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }


        /// <summary>
        /// Récupère l'attribut href de l'élément spécifié
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string? GetHrefFromElement(HtmlNode element)
        {
            string? href = element.GetAttributeValue("href", null);
            if (href == null || href.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug("Impossible de récupérer l'attribut href de l'élément");
                return null;
            }

            return href;
        }
    }
}
