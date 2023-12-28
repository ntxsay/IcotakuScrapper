using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IcotakuScrapper.Anime
{
    internal interface ITanimeBase : ITableSheetBase<TanimeBase>
    {
        /// <summary>
        /// Obtient ou définit le guid de l'anime.
        /// </summary>
        public Guid Guid { get; }
    }
}
