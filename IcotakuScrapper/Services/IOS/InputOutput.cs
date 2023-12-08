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

        public static string GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            if (itemGuid == Guid.Empty)
                throw new ArgumentException("L'identifiant de l'item ne peut pas être vide.", nameof(itemGuid));
            return Path.Combine(GetDirectoryPath(defaultFolder), itemGuid.ToString());
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

        public static string GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            var path = GetDirectoryPath(defaultFolder, itemGuid);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        } 
        
        public static string? GetDirectoryPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid, IcotakuDefaultSubFolder defaultSubFolder, int episodeNumber = 0)
        {
            var subFolder = IcotakuWebHelpers.GetSubFolderName(defaultSubFolder, episodeNumber);
            if (subFolder == null || subFolder.IsStringNullOrEmptyOrWhiteSpace())
                return null;
            
            var path = GetDirectoryPath(defaultFolder, itemGuid);
            path = Path.Combine(path, subFolder);
            
            return !Path.IsPathFullyQualified(path) ? null : path;
        } 
        #endregion

        #region Create

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

        public static string? CreateItemDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            try
            {
                var path = GetDirectoryPath(defaultFolder, itemGuid);
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
