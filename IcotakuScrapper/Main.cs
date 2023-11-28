using HtmlAgilityPack;
using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper
{
    public static class Main
    {
        #region Icotaku
        public const string IcotakuBaseUrl = "https://icotaku.com";
        public static string GetBaseUrl(IcotakuSection section) => section switch
        {
            IcotakuSection.Anime => "https://anime.icotaku.com",
            IcotakuSection.Manga => "https://manga.icotaku.com",
            IcotakuSection.LightNovel => "https://novel.icotaku.com",
            IcotakuSection.Drama => "https://drama.icotaku.com",
            IcotakuSection.Community => "https://communaute.icotaku.com",
            _ => "https://icotaku.com"
        };

        public static int? GetSheetId(Uri sheetUri)
        {
            var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
            if (splitUrl.Length == 0)
                return null;

            var sheetId = splitUrl.FirstOrDefault(f =>
                       !f.Any(a =>
                                      char.IsLetter(a) || a == '-' || a == '_'));

            if (sheetId.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            if (!int.TryParse(sheetId, out var sheetIdInt))
                return null;

            return sheetIdInt;
        }

        public static IcotakuSection? GetIcotakuSection(Uri sheetUri)
        {
            var splitUrl = sheetUri.Segments.Select(s => s.Trim('/')).Where(w => !w.IsStringNullOrEmptyOrWhiteSpace()).ToArray();
            if (splitUrl.Length == 0)
                return null;

            var section = splitUrl.FirstOrDefault(f =>
                                  f.Any(a => char.IsLetter(a) || a == '-' || a == '_'));

            if (section.IsStringNullOrEmptyOrWhiteSpace())
                return null;

            return section switch
            {
                "anime" => IcotakuSection.Anime,
                "manga" => IcotakuSection.Manga,
                "novel" => IcotakuSection.LightNovel,
                "drama" => IcotakuSection.Drama,
                "communaute" => IcotakuSection.Community,
                _ => null
            };
        }

        public static Uri? GetFullHrefFromHtmlNode(HtmlNode node, IcotakuSection section)
        {
            var href = node.GetAttributeValue("href", string.Empty);
            if (href.IsStringNullOrEmptyOrWhiteSpace())
                return new Uri(IcotakuBaseUrl);

            if (href.StartsWith('/'))
                href = href.TrimStart('/');

            href = $"{GetBaseUrl(section)}/{href}";

            if (Uri.TryCreate(href, UriKind.Absolute, out var uri))
                return uri;

            return null;
        }
        
        public static Uri? GetImageFromSrc(IcotakuSection section, string? src)
        {
            if (src == null || src.IsStringNullOrEmptyOrWhiteSpace())
            {
                return null;
            }

            var value = src.Replace("/images/..", GetBaseUrl(section));

            //https://anime.icotaku.com/uploads/animes/anime_229/fiche/affiche_umzrcyl4lhodbB8.jpg
            bool isUri = Uri.TryCreate(value, UriKind.Absolute, out var uri);
            return isUri && uri != null ? uri : null;
        }
        #endregion


        /// <summary>
        /// Nom par défaut de la base de données SQLite de l'application
        /// </summary>
        internal const string DefaultDbFileName = "icotaku.db";

        /// <summary>
        /// Nom actuel de la base de données SQLite de l'application
        /// </summary>
        public static string DbFileName { get; private set; } = DefaultDbFileName;

        /// <summary>
        ///     Chemin d'accès à la base de données SQLite
        /// </summary>
        internal static string DbFile { get; private set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);

        #region Sqlite Raw

        private static string DefaultConnectionString => $"Data Source={DbFile}";

        /// <summary>
        ///     Retourne la chaine de connexion à la base de données SQLite
        /// </summary>
        internal static string ConnexionString { get; private set; } = DefaultConnectionString;

        /// <summary>
        /// Initialise la chaine de connexion à la base de données SQLite
        /// </summary>
        /// <param name="sqliteCipherPassword">Clé de décryptage de la base de données</param>
        public static void GetDbConnectionString(string? sqliteCipherPassword = null)
        {
            if (sqliteCipherPassword == null || sqliteCipherPassword.IsStringNullOrEmptyOrWhiteSpace())
            {
                ConnexionString = DefaultConnectionString;
                return;
            }

            ConnexionString = new SqliteConnectionStringBuilder(DefaultConnectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = sqliteCipherPassword
            }.ToString();
        }

        private static SqliteConnection? _connection;

        /// <summary>
        /// Initialise et retourne une connexion à la base de données SQLite
        /// </summary>
        /// <remarks>Initialise une seule fois la connexion puis reste en mémoire jusqu'à ce que les ressources soient libérées manuellement</remarks>
        /// <returns></returns>
        internal static async Task<SqliteConnection> GetSqliteConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(ConnexionString);
                await _connection.OpenAsync();
            }

            return _connection;
        }

        /// <summary>
        /// Initialise et retourne une connexion à la base de données SQLite
        /// </summary>
        /// <remarks>Initialise une seule fois la connexion puis reste en mémoire jusqu'à ce que les ressources soient libérées manuellement.</remarks>
        /// <returns></returns>
        internal static SqliteConnection GetSqliteConnection()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(ConnexionString);
                _connection.Open();
            }

            return _connection;
        }

        /// <summary>
        ///  Sélectionne une base de données SQLite
        /// </summary>
        /// <remarks>
        /// <list type="bullet">Par défaut la base de données se trouve à la racine du projet mais si vous souhaitez changer l'emplacement de la base de données sans altérer le code.</list>
        /// <list type="bullet">De préférence copiez la base de données vers le nouvel emplacement</list>
        /// Si la base de données n'existe pas, elle est créée</remarks>
        /// <param name="databasefile">Chemin d'accès complet du fichier à vérifier</param>
        public static bool FindSqliteDatabase(string databasefile)
        {
            if (databasefile.IsStringNullOrEmptyOrWhiteSpace() || !File.Exists(databasefile)) 
                return false;

            DbFile = databasefile;
            DbFileName = Path.GetFileName(databasefile);
            return true;
        }
        #endregion

    }
}
