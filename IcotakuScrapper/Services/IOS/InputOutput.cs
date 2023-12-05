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
        /// Retourne le chemin d'accès du dossier de la section spécifiée
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static string GetSectionPath(IcotakuSection section)
            => Path.Combine(Main.BasePath, section.ToString());

        public static string GetItemPath(IcotakuSection section, Guid itemGuid)
        {
            if (itemGuid == Guid.Empty)
                throw new ArgumentException("L'identifiant de l'item ne peut pas être vide.", nameof(itemGuid));
            return Path.Combine(GetSectionPath(section), itemGuid.ToString());
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

        public static string GetSpecifiedPath(IcotakuSection section, params string[] partialPaths)
        {
            var path = GetSectionPath(section);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        }

        public static string GetSpecifiedPath(IcotakuSection section, Guid itemGuid, params string[] partialPaths)
        {
            var path = GetItemPath(section, itemGuid);
            foreach (var partialPath in partialPaths)
            {
                path = Path.Combine(path, partialPath);
            }

            return path;
        } 
        #endregion

        #region Create

        public static string? CreateSectionDirectory(IcotakuSection section)
        {
            try
            {
                var path = GetSectionPath(section);
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

        public static string? CreateItemDirectory(IcotakuSection section, Guid itemGuid)
        {
            try
            {
                var path = GetItemPath(section, itemGuid);
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

        public static string? CreateSpecifiedDirectory(IcotakuSection section, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(section, partialPaths);
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

        public static string? CreateSpecifiedDirectory(IcotakuSection section, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(section, itemGuid, partialPaths);
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

        #region Delete

        public static bool DeleteSectionDirectory(IcotakuSection section)
        {
            try
            {
                var path = GetSectionPath(section);
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

        public static bool DeleteItemDirectory(IcotakuSection section, Guid itemGuid)
        {
            try
            {
                var path = GetItemPath(section, itemGuid);
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

        public static bool DeleteSpecifiedDirectory(IcotakuSection section, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(section, partialPaths);
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


        public static bool DeleteSpecifiedDirectory(IcotakuSection section, Guid itemGuid, params string[] partialPaths)
        {
            try
            {
                var path = GetSpecifiedPath(section, itemGuid, partialPaths);
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
