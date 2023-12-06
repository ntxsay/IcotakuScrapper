using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace IcotakuScrapper
{
    public static class Main
    {
        /// <summary>
        /// Obtient ou définit une valeur indiquant si l'utilisateur a accès au contenu pour adulte (Hentai, Yaoi, Yuri)
        /// </summary>
        public static bool IsAccessingToAdultContent { get; set; } = false;

        /// <summary>
        /// Obtient ou définit une valeur indiquant si l'utilisateur a accès au contenu explicite (violence ou nudité explicite)
        /// </summary>
        public static bool IsAccessingToExplicitContent { get; set; } = true;

        #region Working Directory Variables/Properties
        /// <summary>
        ///     Chemin d'accès du dossier contenant les ressources de l'API
        /// </summary>
        internal static string BasePath { get; private set; } =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ParentFolderName);

        /// <summary>
        ///     Retourne l'objet <see cref="DirectoryInfo" /> représentant le dossier parent-root de l'api.
        /// </summary>
        /// <returns></returns>
        internal static DirectoryInfo BaseDirectoryInfo { get; private set; } =
            new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ParentFolderName));

        /// <summary>
        ///     Nom du dossier parent contenant tous les autres dossiers nécessaire au bon fontionnement de l'application
        /// </summary>
        private const string ParentFolderName = "Resources";
        #endregion

        #region Culture
        public static void SetCultureInfo(string name = "fr-GP")
        {
            try
            {
                var cultureInfo = new CultureInfo(name);
                CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

                //Thread.CurrentThread.CurrentCulture
                //    = CultureInfo.CreateSpecificCulture(name);
                //Thread.CurrentThread.CurrentUICulture
                //    = CultureInfo.CreateSpecificCulture(name);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Data Base Variables/Properties
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

        private static string DefaultConnectionString => $"Data Source={DbFile}";

        /// <summary>
        ///     Retourne la chaine de connexion à la base de données SQLite
        /// </summary>
        internal static string ConnexionString { get; private set; } = DefaultConnectionString;
        #endregion

        #region Manage Working Directory
        /// <summary>
        ///     Initialise le dossier de travail de l'application
        /// </summary>
        /// <param name="directoryPath">chemin d'accès du dossier à vérifier</param>
        /// <returns>True si le dossier de travail a été chargé sinon False</returns>
        public static bool LoadWorkingDirectoryAt(string directoryPath)
        {
            if (directoryPath.IsStringNullOrEmptyOrWhiteSpace())
            {
                LogServices.LogDebug("Le chemin d'accès du dossier est invalide");
                return false;
            }

            if (!Path.IsPathFullyQualified(directoryPath))
            {
                LogServices.LogDebug("Le chemin d'accès du dossier est invalide");
                return false;
            }

            try
            {
                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                BasePath = directoryPath;
                BaseDirectoryInfo = new DirectoryInfo(directoryPath);
                return Directory.Exists(directoryPath);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }
        #endregion

        #region Sqlite Raw
        /// <summary>
        ///  Sélectionne une base de données SQLite
        /// </summary>
        /// <remarks>
        /// <list type="bullet">Par défaut la base de données se trouve à la racine du projet mais si vous souhaitez changer l'emplacement de la base de données sans altérer le code.</list>
        /// <list type="bullet">De préférence copiez la base de données vers le nouvel emplacement</list>
        /// Si la base de données n'existe pas, elle est créée</remarks>
        /// <param name="databasefile">Chemin d'accès complet du fichier à vérifier</param>
        public static bool LoadDataBaseAt(string databasefile)
        {
            if (databasefile.IsStringNullOrEmptyOrWhiteSpace() || !File.Exists(databasefile))
                return false;

            DbFile = databasefile;
            DbFileName = Path.GetFileName(databasefile);
            return true;
        }

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
        #endregion

    }
}
