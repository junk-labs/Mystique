using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.Core
{
    public interface IQueryConvertable
    {
        /// <summary>
        /// クエリ化した文字列を返却します。
        /// </summary>
        string ToQuery();
    }
}
