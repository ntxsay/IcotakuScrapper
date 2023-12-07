using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static string GetSectionPath(IcotakuDefaultFolder defaultFolder)
            => Path.Combine(Main.BasePath, defaultFolder.ToString());

        public static string GetItemPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            if (itemGuid == Guid.Empty)
                throw new ArgumentException("L'identifiant de l'item ne peut pas être vide.", nameof(itemGuid));
            return Path.Combine(GetSectionPath(defaultFolder), itemGuid.ToString());
        }

        public static string GetSpecifiedPath(params string[] partialPaths)
        {
            var path = Main.BasePath;
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        }

        public static string GetSpecifiedPath(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            var path = GetSectionPath(defaultFolder);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        }

        public static string GetSpecifiedPath(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            var path = GetItemPath(defaultFolder, itemGuid);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        } 
        #endregion

        #region Create

        public static string? CreateDefaultDirectory(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetSectionPath(defaultFolder);
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
                var path = GetItemPath(defaultFolder, itemGuid);
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
                var path = GetSpecifiedPath(partialPaths);
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
                var path = GetSpecifiedPath(defaultFolder, partialPaths);
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
                var path = GetSpecifiedPath(defaultFolder, itemGuid, partialPaths);
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

        public static bool ExistsSectionDirectory(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetSectionPath(defaultFolder);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool ExistsItemDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid)
        {
            try
            {
                var path = GetItemPath(defaultFolder, itemGuid);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool ExistsSpecifiedDirectory(params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool ExistsSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(defaultFolder, partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        public static bool ExistsSpecifiedDirectory(IcotakuDefaultFolder defaultFolder, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(defaultFolder, itemGuid, partialPaths);
                return Directory.Exists(path);
            }
            catch (Exception e)
            {
                LogServices.LogDebug(e);
                return false;
            }
        }

        #endregion

        #region Delete

        public static bool DeleteSectionDirectory(IcotakuDefaultFolder defaultFolder)
        {
            try
            {
                var path = GetSectionPath(defaultFolder);
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
                var path = GetItemPath(defaultFolder, itemGuid);
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
                var path = GetSpecifiedPath(partialPaths);
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
                var path = GetSpecifiedPath(defaultFolder, partialPaths);
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
                var path = GetSpecifiedPath(defaultFolder, itemGuid, partialPaths);
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
