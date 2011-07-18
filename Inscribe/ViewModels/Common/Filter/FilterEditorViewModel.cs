using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Inscribe.Filter;
using System.Windows;

namespace Inscribe.ViewModels.Common.Filter
{
    public class FilterEditorViewModel : ViewModel
    {
        public IFilter[] RootFilters { get; private set; }

        public FilterEditorViewModel(IFilter[] roots)
        {
            this.RootFilters = roots;
        }

        public IEnumerable<FilterObjectViewModel> RootFilterViewModels
        {
            get
            {
                return RootFilters.Select(f => FilterObjectViewModel.GenerateViewModel(this, null, f))
                        .Concat(new[] { new FilterAdderViewModel(this, null) });
            }
        }

        public void AddChild(IFilter filterObject)
        {
            if (filterObject == null)
                throw new ArgumentNullException("filterObject");
            if (this.RootFilters == null)
                this.RootFilters = new[] { filterObject };
            else
                this.RootFilters = 
                    this.RootFilters.Concat(new[]{ filterObject}).ToArray();
            this.RaisePropertyChanged(()=>RootFilterViewModels);
        }

        public void RemoveChild(IFilter filterObject)
        {
            this.RootFilters = this.RootFilters.Except(new[]{filterObject}).ToArray();
            this.RaisePropertyChanged(()=>RootFilterViewModels);
        }

        public void ReplaceChild(IFilter oldObject, IFilter newObject)
        {
            this.RootFilters = this.RootFilters.Replace(oldObject, newObject).ToArray();
            this.RaisePropertyChanged(() => RootFilterViewModels);
        }

        public FilterBase EditFilter(FilterBase original = null)
        {
            var vm = new FilterElementEditorViewModel(original);
            Messenger.Raise(new TransitionMessage(vm, "ShowFilterElementEditor"));
            if (vm.Success)
                return vm.ConfiguredFilterBase;
            else
                return null;
        }

        #region Commands

        #region EditWithQueryCommand
        DelegateCommand _EditWithQueryCommand;

        public DelegateCommand EditWithQueryCommand
        {
            get
            {
                if (_EditWithQueryCommand == null)
                    _EditWithQueryCommand = new DelegateCommand(EditWithQuery);
                return _EditWithQueryCommand;
            }
        }

        private void EditWithQuery()
        {
            var qvm = new FilterQueryEditorViewModel(this.RootFilters);
            Messenger.Raise(new TransitionMessage(qvm, "ShowFilterQueryEditor"));
            if (qvm.Success)
            {
                if (qvm.FilterCluster.ConcatenateAnd)
                    this.RootFilters = new[] { qvm.FilterCluster };
                else
                    this.RootFilters = qvm.FilterCluster.Filters.ToArray();
                RaisePropertyChanged(() => RootFilterViewModels);
            }
        }
        #endregion

        #region OnDropCommand
        DelegateCommand<DragEventArgs> _OnDropCommand;

        public DelegateCommand<DragEventArgs> OnDropCommand
        {
            get
            {
                if (_OnDropCommand == null)
                    _OnDropCommand = new DelegateCommand<DragEventArgs>(OnDrop);
                return _OnDropCommand;
            }
        }

        private void OnDrop(DragEventArgs parameter)
        {
            FilterObjectViewModel data = null;
            if (parameter.Data.GetDataPresent(typeof(FilterClusterViewModel)))
                data = parameter.Data.GetData(typeof(FilterClusterViewModel)) as FilterObjectViewModel;
            else if (parameter.Data.GetDataPresent(typeof(FilterItemViewModel)))
                data = parameter.Data.GetData(typeof(FilterItemViewModel)) as FilterObjectViewModel;
            if (data == null) return;
            // target decision
            var odo = parameter.OriginalSource as FrameworkElement;
            var target = odo != null ? odo.DataContext as FilterClusterViewModel : null;
            var troot = odo != null ? odo.DataContext as FilterEditorViewModel : null;
            if (target == null && troot == null) return;
            if (target != null && target.Children.Contains(data)) return;
            if (troot != null && troot.RootFilters.Contains(data.FilterObject)) return;
            var cluster = data as FilterClusterViewModel;
            if (data.Parent != null)
                data.Parent.RemoveChild(data.FilterObject);
            else
                this.RemoveChild(data.FilterObject);
            if (target != null)
                target.AddChild(data.FilterObject);
            else
                troot.AddChild(data.FilterObject);
        }

        #endregion

        #endregion
    }
}
