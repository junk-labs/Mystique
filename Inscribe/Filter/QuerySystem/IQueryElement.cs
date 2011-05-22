using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Filter.QuerySystem
{
    /// <summary>
    /// クエリ内部表現オブジェクト
    /// </summary>
    public interface IQueryElement : IQueryConvertable
    {
        /// <summary>
        /// このオブジェクトの名前空間を定義します。
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// このオブジェクトが公開するクラスを公開します。
        /// </summary>
        string Class { get; }

        /// <summary>
        /// クエリ化する際に利用するメソッドを取得します。
        /// </summary>
        string GetCurrentMethod();

        /// <summary>
        /// クエリ化する時に添付される引数の一覧を取得します。
        /// </summary>
        /// <returns>オブジェクトの列挙、またはNULL</returns>
        IEnumerable<object> GetArgumentsForQueryify();
    }
}