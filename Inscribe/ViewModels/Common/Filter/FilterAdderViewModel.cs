using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet.Commands;
using Inscribe.Filter;

namespace Inscribe.ViewModels.Common.Filter
{
    /// <summary>
    /// フィルタの「追加ボタン」を表現するViewModel
    /// </summary>
    public class FilterAdderViewModel : FilterObjectViewModel
    {
        public FilterAdderViewModel(FilterEditorViewModel root, FilterClusterViewModel parent)
            : base(root, parent) { }

        #region Commands


        #region AddClusterCommand
        DelegateCommand _AddClusterCommand;

        public DelegateCommand AddClusterCommand
        {
            get
            {
                if (_AddClusterCommand == null)
                    _AddClusterCommand = new DelegateCommand(AddCluster);
                return _AddClusterCommand;
            }
        }

        private void AddCluster()
        {
            if (this.Parent != null)
                this.Parent.AddChild(new FilterCluster());
            else
                this.Root.AddChild(new FilterCluster());
        }
        #endregion


        #region AddFilterCommand
        DelegateCommand _AddFilterCommand;

        public DelegateCommand AddFilterCommand
        {
            get
            {
                if (_AddFilterCommand == null)
                    _AddFilterCommand = new DelegateCommand(AddFilter);
                return _AddFilterCommand;
            }
        }

        private void AddFilter()
        {
            var filter = this.Root.EditFilter();
            if (filter != null)
            {
            if (this.Parent != null)
                this.Parent.AddChild(filter);
            else
                this.Root.AddChild(filter);
            }
        }
        #endregion
      
        #endregion

        public override IFilter FilterObject
        {
            get { return null; }
        }
    }
}
