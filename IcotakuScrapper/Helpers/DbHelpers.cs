using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IcotakuScrapper.Helpers
{
    internal static class DbHelpers
    {
        internal static string? ConvertGuidToStringSqlite(Guid _guid)
        {
            return _guid.ToString("N")?.ToUpper();
        }
    }
}
