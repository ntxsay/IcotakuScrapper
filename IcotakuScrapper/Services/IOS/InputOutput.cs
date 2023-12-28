using IcotakuScrapper.Extensions;

namespace IcotakuScrapper.Services.IOS
{
    public static class InputOutput
    {
        #region Get
        /// <summary>
        /// Retourne le chemin d'accès du dossier de la defaultFolder spécifiée
        /// </summary>
        /// <param name="defaultFolder"></param>
        /// <returns></returns>
        public static string GetDirectoryPath(IcotakuDefaultFolder defaultFolder)
            => Path.Combine(Main.BasePath, defaultFolder.ToString());

        /// <summary>
        /// Retourne le chemin d'accès du dossier de l'item dans le dossier par défaut spécifié
        /// </summary>
        /// <param name="defaultFolder"></param>
        /// <param name="itemGuid"></param>
        /// <returns></returns>
        public static string? GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            if (itemGuid == Guid.Empty)
            {
                LogServices.LogDebug("L'identifiant de l'item est invalide.");
                return null;
            }
            var defaultFolderPath = GetDirectoryPath(defaultFolder);
            if (!defaultFolderPath.IsStringNullOrEmptyOrWhiteSpace())
                return Path.Combine(defaultFolderPath, itemGuid.ToString());
            
            LogServices.LogDebug("Le chemin d'accès du dossier est invalide.");
            return null;
        }

        public static string GetDirectoryPath(params string[] partialPaths)
        {
            var path = Main.BasePath;
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        }

        public static string GetDirectoryPath(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            var path = GetDirectoryPath(defaultFolder);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        }

        public static string? GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            var path = GetDirectoryPath(defaultFolder, itemGuid);
            if (path == null || path.IsStringNullOrEmptyOrWhiteSpace())
                return null;
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        } 
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultFolder"></param>
        /// <param name="itemGuid"></param>
        /// <param name="defaultSubFolder"></param>
        /// <param name="episodeNumber"></param>
        /// <returns></returns>
        public static string? GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid, IcotakuDefaultSubFolder defaultSubFolder, int episodeNumber = 0)
        {
            var relativeSubFolderPath = IcotakuWebHelpers.GetRelativeSubFolderPath(defaultSubFolder, episodeNumber);
            if (relativeSubFolderPath == null || relativeSubFolderPath.IsStringNullOrEmptyOrWhiteSpace())
                return null;
            
            var defaultFolderItemPath = GetDirectoryPath(defaultFolder, itemGuid);
            if (defaultFolderItemPath == null || defaultFolderItemPath.IsStringNullOrEmptyOrWhiteSpace())
                return null;
            defaultFolderItemPath = Path.Combine(defaultFolderItemPath, relativeSubFolderPath.TrimStart(Path.DirectorySeparatorChar));
            
            return !Path.IsPathFullyQualified(defaultFolderItemPath) ? null : defaultFolderItemPath;
        } 
        #endregion

        #region Create

        /// <summary>
        /// Crée le dossier de la <see cref="IcotakuDefaultFolder"/> spécifiée
        /// </summary>
        /// <param name="defaultFolder">Dossier par défaut</param>
        /// <returns></returns>
        public static string? CreateDefaultDirectory(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return null;
            }
        }

        /// <summary>
        /// Crée le dossier de l'item dans le dossier par défaut spécifié
        /// </summary>
        /// <param name="defaultFolder"></param>
        /// <param name="itemGuid"></param>
        /// <returns></returns>
        public static string? CreateItemDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            try
            {
                var defaultFolderItemPath = GetDirectoryPath(defaultFolder, itemGuid);
                if (defaultFolderItemPath == null || defaultFolderItemPath.IsStringNullOrEmptyOrWhiteSpace())
                    return null;
                if (!Directory.Exists(defaultFolderItemPath))
                    Directory.CreateDirectory(defaultFolderItemPath);
                return defaultFolderItemPath;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return null;
            }
        }

        public static string? CreateSpecifiedDirectory(params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(partialPaths);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return null;
            }
        }

        public static string? CreateSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, partialPaths);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return null;
            }
        }

        public static string? CreateSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid, partialPaths);
                if (path == null || path.IsStringNullOrEmptyOrWhiteSpace())
                    return null;
                
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return null;
            }
        }

        #endregion

        #region Exists

        public static bool IsDirectoryExists(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool IsDirectoryExists(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool IsDirectoryExists(params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool IsDirectoryExists(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool IsDirectoryExists(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid, partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }
        
        public static bool IsDirectoryExists(IcotakuDefaultFolder defaultFolder, Guid itemGuid, IcotakuDefaultSubFolder defaultSubFolder, int episodeNumber = 0)
        {
            var path = GetDirectoryPath(defaultFolder, itemGuid, defaultSubFolder, episodeNumber);
            return Directory.Exists(path);
        }

        #endregion

        #region Delete

        public static bool DeleteSectionDirectory(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return !Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool DeleteItemDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return !Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool DeleteSpecifiedDirectory(params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(partialPaths);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return !Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool DeleteSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, partialPaths);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return !Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }


        public static bool DeleteSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid, partialPaths);
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
                return !Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        #endregion
    }
}
