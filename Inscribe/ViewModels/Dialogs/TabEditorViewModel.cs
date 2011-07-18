using System;
using System.Linq;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.Communication.CruiseControl.Lists;
using Inscribe.Communication.Streaming;
using Inscribe.Configuration.Tabs;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Particular;
using Inscribe.Filter.Filters.Text;
using Inscribe.Storage;
using Livet;
using Livet.Commands;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using Inscribe.ViewModels.Common.Filter;
using Livet.Messaging;

namespace Inscribe.ViewModels.Dialogs
{
    public class TabEditorViewModel : ViewModel
    {
        private TabProperty property;

        public TabEditorViewModel(TabProperty property)
        {
            this.property = property;
            this._filterEditorViewModel = new FilterEditorViewModel(property.TweetSources.ToArray());
        }

        public string TabName
        {
            get { return this.property.Name; }
            set
            {
                this.property.Name = value;
                RaisePropertyChanged(() => TabName);
            }
        }

        public string NotifySoundPath
        {
            get { return this.property.NotifySoundPath; }
            set
            {
                this.property.NotifySoundPath = value;
                RaisePropertyChanged(() => NotifySoundPath);
            }
        }

        #region SelectSoundPathCommand
        DelegateCommand<OpeningFileSelectionMessage> _SelectSoundPathCommand;

        public DelegateCommand<OpeningFileSelectionMessage> SelectSoundPathCommand
        {
            get
            {
                if (_SelectSoundPathCommand == null)
                    _SelectSoundPathCommand = new DelegateCommand<OpeningFileSelectionMessage>(SelectSoundPath);
                return _SelectSoundPathCommand;
            }
        }

        private void SelectSoundPath(OpeningFileSelectionMessage parameter)
        {
            if (parameter.Response != null)
            {
                this.NotifySoundPath = parameter.Response;
            }
        }
        #endregion

        public bool IsNotifyDisabled
        {
            get { return this.property.IsNotifyDisabled; }
            set
            {
                this.property.IsNotifyDisabled = value;
                RaisePropertyChanged(() => IsNotifyDisabled);
            }
        }

        private FilterEditorViewModel _filterEditorViewModel;
        public FilterEditorViewModel FilterEditorViewModel
        {
            get { return this._filterEditorViewModel; }
        }

        #region Streaming-Query Control

        public string[] StreamingQueries
        {
            get { return this.property.StreamingQueries; }
        }

        private string _addQueryCandidate = String.Empty;

        public string AddQueryCandidate
        {
            get { return this._addQueryCandidate; }
            set
            {
                this._addQueryCandidate = value;
                RaisePropertyChanged(() => AddQueryCandidate);
                AddQueryCommand.RaiseCanExecuteChanged();
            }
        }

        #region AddQueryCommand
        DelegateCommand _AddQueryCommand;

        public DelegateCommand AddQueryCommand
        {
            get
            {
                if (_AddQueryCommand == null)
                    _AddQueryCommand = new DelegateCommand(AddQuery, CanAddQuery);
                return _AddQueryCommand;
            }
        }

        private bool CanAddQuery()
        {
            return !String.IsNullOrWhiteSpace(AddQueryCandidate);
        }

        private void AddQuery()
        {
            this.property.StreamingQueries = this.property.StreamingQueries.Concat(new[] { AddQueryCandidate }).Distinct().ToArray();
            if (!UserStreamsReceiverManager.AddQuery(AddQueryCandidate))
            {
                this.Messenger.Raise(new InformationMessage(
                    "ストリーミング クエリーの追加ができませんでした。" + Environment.NewLine +
                    "User Streams接続のアカウントが存在しないか、接続が安定化していません。",
                    "クエリ追加エラー", System.Windows.MessageBoxImage.Warning, "WarningMessage"));
            }
            this.FilterEditorViewModel.AddChild(new FilterText(AddQueryCandidate));
            this.AddQueryCandidate = String.Empty;
            RaisePropertyChanged(() => StreamingQueries);
        }
        #endregion

        #region RemoveQueryCommand
        DelegateCommand<string> _RemoveQueryCommand;

        public DelegateCommand<string> RemoveQueryCommand
        {
            get
            {
                if (_RemoveQueryCommand == null)
                    _RemoveQueryCommand = new DelegateCommand<string>(RemoveQuery);
                return _RemoveQueryCommand;
            }
        }

        private void RemoveQuery(string parameter)
        {
            this.property.StreamingQueries =
                this.property.StreamingQueries.Except(new[] { parameter }).ToArray();
            UserStreamsReceiverManager.RemoveQuery(parameter);
            this.FilterEditorViewModel.RootFilters.OfType<FilterText>()
                .Where(f => f.Needle == parameter).ForEach(f => this.FilterEditorViewModel.RemoveChild(f));
            RaisePropertyChanged(() => StreamingQueries);
        }
        #endregion

        #endregion

        #region List Control

        public string[] Accounts
        {
            get { return AccountStorage.Accounts.Select(s => s.ScreenName).ToArray(); }
        }

        private string _selectedScreenName;
        public string SelectedScreenName
        {
            get { return this._selectedScreenName; }
            set
            {
                this._selectedScreenName = value;
                RaisePropertyChanged(() => SelectedScreenName);
                RaisePropertyChanged(() => ListItems);
            }
        }

        #region UpdateListCommand
        DelegateCommand _UpdateListCommand;

        public DelegateCommand UpdateListCommand
        {
            get
            {
                if (_UpdateListCommand == null)
                    _UpdateListCommand = new DelegateCommand(UpdateList);
                return _UpdateListCommand;
            }
        }

        private bool _isListLoading = false;
        public bool IsListLoading
        {
            get { return this._isListLoading; }
            set
            {
                this._isListLoading = value;
                RaisePropertyChanged(() => IsListLoading);
            }
        }

        private void UpdateList()
        {
            if (!AccountStorage.Contains(_selectedScreenName))
                return;
            var acc = AccountStorage.Get(_selectedScreenName);
            IsListLoading = true;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var lists = acc.GetFollowingListsAll(_selectedScreenName);
                    if (lists != null)
                        lists.ForEach(l => acc.RegisterFollowingList(l));
                    RaisePropertyChanged(() => ListItems);
                }
                finally
                {
                    IsListLoading = false;
                }
            });
        }
        #endregion

        public string[] ListItems
        {
            get
            {
                if (!AccountStorage.Contains(_selectedScreenName))
                    return null;
                return AccountStorage.Get(_selectedScreenName).FollowingLists.Select(l => l.User.ScreenName + "/" + l.Name).ToArray();
            }
        }

        #region AddListCommand
        DelegateCommand<string> _AddListCommand;

        public DelegateCommand<string> AddListCommand
        {
            get
            {
                if (_AddListCommand == null)
                    _AddListCommand = new DelegateCommand<string>(AddList, CanAddList);
                return _AddListCommand;
            }
        }

        private bool CanAddList(string parameter)
        {
            return !String.IsNullOrEmpty(parameter);
        }

        private void AddList(string parameter)
        {
            this.property.FollowingLists =
                this.property.FollowingLists.Concat(new[] { parameter }).Distinct().ToArray();
            var sp = parameter.Split('/');
            ListReceiverManager.RegisterReceive(sp[0], sp[1]);
            this.FilterEditorViewModel.AddChild(new FilterList(sp[0], sp[1]));
            RaisePropertyChanged(() => FollowingLists);
        }
        #endregion

        #region RemoveListCommand
        DelegateCommand<string> _RemoveListCommand;

        public DelegateCommand<string> RemoveListCommand
        {
            get
            {
                if (_RemoveListCommand == null)
                    _RemoveListCommand = new DelegateCommand<string>(RemoveList, CanRemoveList);
                return _RemoveListCommand;
            }
        }

        private bool CanRemoveList(string parameter)
        {
            return !String.IsNullOrEmpty(parameter);
        }

        private void RemoveList(string parameter)
        {
            this.property.FollowingLists =
                this.property.FollowingLists.Except(new[] { parameter }).Distinct().ToArray();
            var sp = parameter.Split('/');
            ListReceiverManager.RemoveReceive(sp[0], sp[1]);
            this.FilterEditorViewModel.RootFilters
                .OfType<FilterList>()
                .Where(f => f.ListUser == sp[0] && f.ListName == sp[1])
                .ForEach(f => this.FilterEditorViewModel.RemoveChild(f));
            RaisePropertyChanged(() => FollowingLists);
        }
        #endregion

        public string[] FollowingLists
        {
            get { return this.property.FollowingLists; }
        }

        #endregion 

        #region CloseCommand
        DelegateCommand _CloseCommand;

        public DelegateCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                    _CloseCommand = new DelegateCommand(Close);
                return _CloseCommand;
            }
        }

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage("Close", WindowAction.Close));
        }
        #endregion
    }
}
