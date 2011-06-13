using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.Core;

namespace Inscribe.Filter
{
    public static class FilterOperations
    {
        /// <summary>
        /// フィルタクラスタにフィルタを結合します。
        /// </summary>
        public static FilterCluster Join(this FilterCluster cluster, FilterBase filter, bool concatAnd = false)
        {
            var newCluster = new FilterCluster();

            if (cluster != null)
                newCluster.Filters = new IFilter[] { cluster, filter };
            else
                newCluster.Filters = new IFilter[] { filter };

            newCluster.ConcatenateAnd = concatAnd;

            return newCluster.Optimize();
        }

        /// <summary>
        /// フィルタクラスタを新しいクラスタに結合します。(OR結合)
        /// </summary>
        public static FilterCluster Concat(this FilterCluster original, params FilterCluster[] append)
        {
            var cluster = new FilterCluster();
            cluster.ConcatenateAnd = false;
            cluster.Filters = new[] { original }.Concat(append).ToArray();
            return cluster.Optimize();
        }

        /// <summary>
        /// フィルタクラスタを新しいクラスタに結合します。(AND結合)
        /// </summary>
        public static FilterCluster Restrict(this FilterCluster original, params FilterCluster[] append)
        {
            var cluster = new FilterCluster();
            cluster.ConcatenateAnd = true;
            cluster.Filters = new[] { original }.Concat(append).ToArray();
            return cluster.Optimize();
        }

        /// <summary>
        /// フィルタを簡易化します。
        /// </summary>
        public static FilterCluster Optimize(this FilterCluster cluster)
        {
            return QueryCompiler.Optimize(cluster);
        }
        public static FilterCluster Copy(this FilterCluster cluster)
        {
            return QueryCompiler.ToFilter(cluster.ToQuery());
        }

        public static FilterCluster GetParent(this IFilter filterObject, FilterCluster root)
        {
            if (root.Filters.Contains(filterObject))
            {
                return root;
            }
            else
            {
                return root.Filters
                    .OfType<FilterCluster>()
                    .Select(f => GetParent(f, root))
                    .Where(f => f != null)
                    .FirstOrDefault();
            }
        }
    }
}
