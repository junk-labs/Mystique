using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Filter;

namespace Inscribe.ViewModels.Common.Filter
{
    /// <summary>
    /// フィルタオブジェクト基底ViewModel
    /// </summary>
    public abstract class FilterObjectViewModel : ViewModel
    {
        public static FilterObjectViewModel GenerateViewModel(FilterEditorViewModel root, FilterClusterViewModel parent, IFilter item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            var cluster = item as FilterCluster;
            if (cluster != null)
                return new FilterClusterViewModel(root, parent, cluster);
            else
                return new FilterItemViewModel(root, parent, item as FilterBase);
        }

        public readonly FilterClusterViewModel Parent;
        public readonly FilterEditorViewModel Root;
        public FilterObjectViewModel(FilterEditorViewModel root, FilterClusterViewModel parent)
        {
            this.Root = root;
            this.Parent = parent;
        }

        public abstract IFilter FilterObject { get; }
    }

}
