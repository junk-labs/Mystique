using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Storage;
using Inscribe.Model;
using Livet.Command;
using System.Collections.ObjectModel;
using System.Threading;

namespace Mystique.ViewModels.Common
{
    public class UserSelectorViewModel : ViewModel
    {
        public UserSelectorViewModel()
        {
            ViewModelHelper.BindNotification(AccountStorage.AccountsChangedEvent, this, (o, e) => MakeTargets());
        }

        private void MakeTargets()
        {
            var acs = AccountStorage.Accounts.ToArray();
            var news = acs.Except(this._linkDatas.Select(ld => ld.AccountInfo)).ToArray();
            var olds = this._linkDatas.Where(u => !acs.Contains(u.AccountInfo)).ToArray();
            news.ForEach(i => this._linkDatas.Add(new UserLinkData(i, this, this.LinkElements.Contains(i))));
            olds.ForEach(i => this._linkDatas.Remove(i));
            this._linkDatas.Where(i => !news.Contains(i.AccountInfo)).ForEach(i => i.IsLink = this.LinkElements.Contains(i.AccountInfo));
        }

        private IEnumerable<AccountInfo> _linkElements = null;
        public IEnumerable<AccountInfo> LinkElements
        {
            get { return this._linkElements ?? new AccountInfo[0]; }
            set
            {
                this._linkElements = value;
                MakeTargets();
            }
        }

        private ObservableCollection<UserLinkData> _linkDatas = new ObservableCollection<UserLinkData>();
        public IEnumerable<UserLinkData> LinkDatas
        {
            get { return this._linkDatas; }
        }


        #region LinkChangedイベント

        public event EventHandler<EventArgs> LinkChanged;
        private Notificator<EventArgs> _LinkChangedEvent;
        public Notificator<EventArgs> LinkChangedEvent
        {
            get
            {
                if (_LinkChangedEvent == null) _LinkChangedEvent = new Notificator<EventArgs>();
                return _LinkChangedEvent;
            }
            set { _LinkChangedEvent = value; }
        }

        protected void OnLinkChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref LinkChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            LinkChangedEvent.Raise(e);
        }

        #endregion
      

        #region CheckAllCommand
        DelegateCommand _CheckAllCommand;

        public DelegateCommand CheckAllCommand
        {
            get
            {
                if (_CheckAllCommand == null)
                    _CheckAllCommand = new DelegateCommand(CheckAll);
                return _CheckAllCommand;
            }
        }

        private void CheckAll()
        {
            LinkElements = this._linkDatas.Select(l => l.AccountInfo).ToArray();
            MakeTargets();
        }
        #endregion

        #region UncheckAllCommand
        DelegateCommand _UncheckAllCommand;

        public DelegateCommand UncheckAllCommand
        {
            get
            {
                if (_UncheckAllCommand == null)
                    _UncheckAllCommand = new DelegateCommand(UncheckAll);
                return _UncheckAllCommand;
            }
        }

        private void UncheckAll()
        {
            this.LinkElements = null;
            MakeTargets();
        }
        #endregion

        public void SelectThis(UserLinkData ld)
        {
            LinkElements = new[] { ld.AccountInfo };
            MakeTargets();
        }

        public void ChangeLinkState(UserLinkData linkdata, bool check)
        {
            if (check)
            {
                this.LinkElements = this.LinkElements.Concat(new[] { linkdata.AccountInfo }).Distinct().ToArray();
            }
            else
            {
                this.LinkElements = this.LinkElements.Except(new[] { linkdata.AccountInfo }).ToArray();
            }
        }    
    }

    public class UserLinkData : ViewModel
    {
        UserSelectorViewModel parent;

        AccountInfo info;

        public UserLinkData(AccountInfo info, UserSelectorViewModel parent, bool isLink)
        {
            this.parent = parent;
            this.info = info;
            this.IsLink = isLink;
        }

        public AccountInfo AccountInfo
        {
            get { return this.info; }
        }

        public string ScreenName
        {
            get { return this.info.ScreenName; }
        }
        
        bool _IsLink;
        public bool IsLink
        {
            get
            { return _IsLink; }
            set
            {
                if (_IsLink == value)
                    return;
                _IsLink = value;
                RaisePropertyChanged("IsLink");
            }
        }


        #region SelectThisCommand
        DelegateCommand _SelectThisCommand;

        public DelegateCommand SelectThisCommand
        {
            get
            {
                if (_SelectThisCommand == null)
                    _SelectThisCommand = new DelegateCommand(SelectThis);
                return _SelectThisCommand;
            }
        }

        private void SelectThis()
        {
            parent.SelectThis(this);
        }
        #endregion
      

    }
}
