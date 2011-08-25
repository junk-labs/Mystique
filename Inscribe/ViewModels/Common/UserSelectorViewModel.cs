using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Livet;
using Inscribe.Storage;
using Inscribe.Model;
using Livet.Commands;
using System.Collections.ObjectModel;
using System.Threading;

namespace Inscribe.ViewModels.Common
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

        public event Action LinkChanged;

        private void OnLinkChanged()
        {
            var lc = this.LinkChanged;
            if(lc != null)
                lc();
        }
      
        #region CheckAllCommand
        ViewModelCommand _CheckAllCommand;

        public ViewModelCommand CheckAllCommand
        {
            get
            {
                if (_CheckAllCommand == null)
                    _CheckAllCommand = new ViewModelCommand(CheckAll);
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
        ViewModelCommand _UncheckAllCommand;

        public ViewModelCommand UncheckAllCommand
        {
            get
            {
                if (_UncheckAllCommand == null)
                    _UncheckAllCommand = new ViewModelCommand(UncheckAll);
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
            this.OnLinkChanged();
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
            this.OnLinkChanged();
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
            this._profileImageProvider = new ProfileImageProvider(info);
            this._IsLink = isLink;
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
                parent.ChangeLinkState(this, this._IsLink);
            }
        }

        #region SelectThisCommand
        ViewModelCommand _SelectThisCommand;

        public ViewModelCommand SelectThisCommand
        {
            get
            {
                if (_SelectThisCommand == null)
                    _SelectThisCommand = new ViewModelCommand(SelectThis);
                return _SelectThisCommand;
            }
        }

        private void SelectThis()
        {
            parent.SelectThis(this);
        }
        #endregion

        private ProfileImageProvider _profileImageProvider;
        public ProfileImageProvider ProfileImageProvider
        {
            get { return this._profileImageProvider; }
        }

    }
}
