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
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IcotakuDefaultParentFolderName);
        
        /// <summary>
        ///     Nom du dossier parent contenant tous les autres dossiers nécessaire au bon fontionnement de l'application
        /// </summary>
        private const string IcotakuDefaultParentFolderName = "IcotakuScrapper";
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
        /// Obtient le chemin d'accès complet de la base de données SQLite de l'application
        /// </summary>
        private static string DefaultDbFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DbFileName);
        
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
        public static void LoadWorkingDirectoryAt(string directoryPath)
        {
            if (directoryPath.IsStringNullOrEmptyOrWhiteSpace())
                throw new DirectoryNotFoundException("Le chemin d'accès du dossier de travail est invalide.");

            if (!Path.IsPathFullyQualified(directoryPath))
                throw new DirectoryNotFoundException("Le chemin d'accès du dossier de travail est invalide.");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            BasePath = directoryPath;
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
        /// <param name="databaseFile">Chemin d'accès complet du fichier à vérifier</param>
        public static void LoadDatabaseAt(string databaseFile)
        {
            if (databaseFile.IsStringNullOrEmptyOrWhiteSpace())
                throw new FileNotFoundException("Le chemin d'accès à la base de données est invalide.");
            
            if (!Path.IsPathFullyQualified(databaseFile))
                throw new FileNotFoundException("Le chemin d'accès à la base de données est invalide.");
            
            if (!Path.HasExtension(databaseFile))
                throw new FileNotFoundException("Le fichier de la base de données ne contient pas l'extension \".db\".");
            
            if (!Path.GetExtension(databaseFile).Equals(".db", StringComparison.OrdinalIgnoreCase))
                throw new FileNotFoundException("Le fichier de la base de données ne contient pas l'extension \".db\".");
            
            if (!File.Exists(databaseFile))
            {
                if (!File.Exists(DefaultDbFile))
                    throw new FileNotFoundException($"La base de données n'existe pas à : \"{DefaultDbFile}\"");
                File.Copy(DefaultDbFile, databaseFile);
            }

            DbFile = databaseFile;
            DbFileName = Path.GetFileName(databaseFile);
        }

        /// <summary>
        /// Initialise la chaine de connexion à la base de données SQLite
        /// </summary>
        /// <param name="sqliteCipherPassword">Clé de décryptage de la base de données</param>
        public static void InitializeDbConnectionString(string? sqliteCipherPassword = null)
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
        private static SqliteCommand? _command;

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
        /// Retourne une commande SQL à partir de la connexion à la base de données SQLite
        /// </summary>
        /// <returns></returns>
        internal static async Task<SqliteCommand> GetSqliteCommandAsync()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(ConnexionString);
                await _connection.OpenAsync();
            }

            if (_command == null)
            {
                _command = _connection.CreateCommand();
            }
            else
            {
                _command.Parameters.Clear();
                _command.CommandText = null;
            }

            return _command;
        }

        /// <summary>
        /// Retourne une commande SQL à partir de la connexion à la base de données SQLite
        /// </summary>
        /// <returns></returns>
        internal static SqliteCommand GetSqliteCommand()
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection(ConnexionString);
                _connection.Open();
            }

            if (_command == null)
            {
                _command = _connection.CreateCommand();
            }
            else
            {
                _command.Parameters.Clear();
                _command.CommandText = null;
            }

            return _command;
        }
        #endregion

    }
}
