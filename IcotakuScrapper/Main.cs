using IcotakuScrapper.Extensions;
using Microsoft.Data.Sqlite;

namespace IcotakuScrapper
{
    public static class Main
    {
        /// <summary>
        /// Nom par défaut de la base de données SQLite de l'application
        /// </summary>
        internal const string defaultDbFileName = "icotaku.db";

        /// <summary>
        /// Nom actuel de la base de données SQLite de l'application
        /// </summary>
        public static string DbFileName { get; private set; } = defaultDbFileName;

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
