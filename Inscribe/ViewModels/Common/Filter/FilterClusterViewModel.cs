using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter;
using Livet.Commands;

namespace Inscribe.ViewModels.Common.Filter
{
    /// <summary>
    /// フィルタクラスタのViewModel
    /// </summary>
    public class FilterClusterViewModel : FilterObjectViewModel
    {
        private readonly FilterCluster cluster;

        public enum ClusterStrategy
        {
            AND,
            OR,
            NAND,
            NOR
        }

        public ClusterStrategy Method
        {
            get
            {
                if (this.cluster.ConcatenateAnd)
                    return this.cluster.Negate ?
                        ClusterStrategy.NAND : ClusterStrategy.AND;
                else
                    return this.cluster.Negate ?
                        ClusterStrategy.NOR : ClusterStrategy.OR;
            }
            set
            {
                this.cluster.ConcatenateAnd = value == ClusterStrategy.AND || value == ClusterStrategy.NAND;
                this.cluster.Negate = value == ClusterStrategy.NAND || value == ClusterStrategy.NOR;
                this.RaisePropertyChanged(() => Method);
                RaisePropertyChanged(() => MethodInteger);
            }
        }

        public int MethodInteger
        {
            get { return (int)this.Method; }
            set
            {
                this.Method = (ClusterStrategy)value;
            }
        }

        public FilterClusterViewModel(FilterEditorViewModel root, FilterClusterViewModel parent, FilterCluster cluster)
            : base(root, parent)
        {
            if (cluster == null)
                throw new ArgumentNullException("cluster");
            this.cluster = cluster;
        }

        public IEnumerable<FilterObjectViewModel> Children
        {
            get
            {
                if (this.cluster.Filters == null)
                    return new[] { new FilterAdderViewModel(this.Root, this) };
                else
                    return this.cluster.Filters
                        .Select(f => FilterObjectViewModel.GenerateViewModel(this.Root, this, f))
                        .Concat(new[] { new FilterAdderViewModel(this.Root, this) });
            }
        }

        public void AddChild(IFilter filterObject)
        {
            if (filterObject == null)
                throw new ArgumentNullException("filterObject");
            if (this.cluster.Filters == null)
                this.cluster.Filters = new[] { filterObject };
            else
                this.cluster.Filters =
                    this.cluster.Filters.Concat(new[] { filterObject }).ToArray();
            this.RaisePropertyChanged(() => Children);
        }

        public void RemoveChild(IFilter filterObject)
        {
            this.cluster.Filters =
                this.cluster.Filters.Except(new[] { filterObject }).ToArray();
            this.RaisePropertyChanged(() => Children);
        }

        public void ReplaceChild(IFilter oldObject, IFilter newObject)
        {
            this.cluster.Filters = this.cluster.Filters
                .Replace(oldObject, newObject).ToArray();
            this.RaisePropertyChanged(() => Children);
        }

        #region Commands

        #region DeleteCommand
        DelegateCommand _DeleteCommand;
        
        public DelegateCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                    _DeleteCommand = new DelegateCommand(Delete);
                return _DeleteCommand;
            }
        }
       
        private void Delete()
        {
            if (this.Parent != null)
                this.Parent.RemoveChild(this.cluster);
            else
                this.Root.RemoveChild(this.cluster);
        }
        #endregion

        #endregion

        public override IFilter FilterObject
        {
            get { return this.cluster; }
        }
    }
}
