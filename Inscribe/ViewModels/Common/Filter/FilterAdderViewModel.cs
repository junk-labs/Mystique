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
        ViewModelCommand _AddClusterCommand;

        public ViewModelCommand AddClusterCommand
        {
            get
            {
                if (_AddClusterCommand == null)
                    _AddClusterCommand = new ViewModelCommand(AddCluster);
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
        ViewModelCommand _AddFilterCommand;

        public ViewModelCommand AddFilterCommand
        {
            get
            {
                if (_AddFilterCommand == null)
                    _AddFilterCommand = new ViewModelCommand(AddFilter);
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
