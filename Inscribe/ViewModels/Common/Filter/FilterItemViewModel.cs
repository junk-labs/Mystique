using System;
using Inscribe.Filter;
using Livet.Commands;

namespace Inscribe.ViewModels.Common.Filter
{

    /// <summary>
    /// フィルタ要素のViewModel
    /// </summary>
    public class FilterItemViewModel : FilterObjectViewModel
    {
        public FilterItemViewModel(FilterEditorViewModel root, FilterClusterViewModel parent, FilterBase item)
            : base(root, parent)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            this.Filter = item;
        }

        public string FilterDescription
        {
            get
            {
                return _filter.FilterStateString;
            }
        }

        public bool IsNegate
        {
            get { return _filter.Negate; }
        }

        private FilterBase _filter;
        public FilterBase Filter
        {
            get { return this._filter; }
            set
            {
                this._filter = value;
                RaisePropertyChanged(() => FilterObject);
                RaisePropertyChanged(() => FilterDescription);
                RaisePropertyChanged(() => IsNegate);
            }
        }
        public override IFilter FilterObject
        {
            get { return this._filter; }
        }

        #region EditCommand
        DelegateCommand _EditCommand;

        public DelegateCommand EditCommand
        {
            get
            {
                if (_EditCommand == null)
                    _EditCommand = new DelegateCommand(Edit);
                return _EditCommand;
            }
        }

        private void Edit()
        {
            var f = this.Root.EditFilter(_filter);
            if (f == null) return;
            // 再生成される
            this.Parent.ReplaceChild(this.Filter, f);
        }
        #endregion

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
                this.Parent.RemoveChild(this._filter);
            else
                this.Root.RemoveChild(this._filter);
        }
        #endregion
      
    }
}
