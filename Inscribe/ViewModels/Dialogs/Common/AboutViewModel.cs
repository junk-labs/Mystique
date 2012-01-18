using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inscribe.Common;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class AboutViewModel : ViewModel
    {
        private readonly ContributorViewModel[] contributors = new ContributorViewModel[]{
            new ContributorViewModel("そらん", "dasoran"),
            new ContributorViewModel("佐々木＠くっくっ。", "ssk_uo"),
            new ContributorViewModel("るみぃ", "lummy_ts"),
            new ContributorViewModel("たけしけー", "takeshik"),
            new ContributorViewModel("凡骨A", "bonkotsua"),
            new ContributorViewModel("ひかりさま", "miz_hi"),
        };

        public AboutViewModel()
        {
            Task.Factory.StartNew(() => CheckUpdate());
        }

        public IEnumerable<ContributorViewModel> Contributors
        {
            get
            {
                // 順番はランダム
                return contributors.Shuffle()
                    .Concat(new[] { new ContributorViewModel("(順不同で掲載しています)") });
            }
        }

        public ReleaseKind ReleaseKind
        {
            get
            {
                int value = Define.GetVersion().FilePrivatePart;
                if (value >= 0 && value < 6)
                    return (ReleaseKind)value;
                else
                    return Common.ReleaseKind.Special;
            }
        }

        public string AppVersion
        {
            get { return Define.GetFormattedVersion(); }
        }

        #region ShowBrowserCommand
        ListenerCommand<string> _ShowBrowserCommand;

        public ListenerCommand<string> ShowBrowserCommand
        {
            get
            {
                if (_ShowBrowserCommand == null)
                    _ShowBrowserCommand = new ListenerCommand<string>(ShowBrowser);
                return _ShowBrowserCommand;
            }
        }

        private void ShowBrowser(string parameter)
        {
            Browser.Start(parameter);
        }

        #endregion

        private VersionCheckState _checkState = VersionCheckState.Checking;
        public VersionCheckState CheckState
        {
            get { return this._checkState; }
            set
            {
                this._checkState = value;
                RaisePropertyChanged(() => CheckState);
            }
        }

        private void CheckUpdate()
        {
            Thread.Sleep(100);
            try
            {
                CheckState = VersionCheckState.Checking;
                if (UpdateReceiver.CheckUpdate())
                {
                    CheckState = VersionCheckState.Ready;
                }
                else
                {
                    CheckState = VersionCheckState.Finished;
                }
            }
            catch (Exception)
            {
                CheckState = VersionCheckState.Failed;
            }
        }

        #region AppUpdateCommand
        ViewModelCommand _AppUpdateCommand;

        public ViewModelCommand AppUpdateCommand
        {
            get
            {
                if (_AppUpdateCommand == null)
                    _AppUpdateCommand = new ViewModelCommand(AppUpdate);
                return _AppUpdateCommand;
            }
        }

        private void AppUpdate()
        {
            UpdateReceiver.StartUpdateArchive();
        }

        #endregion

        private bool _isVisibleLicense = false;
        public bool IsVisibleLicense
        {
            get { return this._isVisibleLicense; }
            set
            {
                this._isVisibleLicense = value;
                RaisePropertyChanged(() => IsVisibleLicense);
            }
        }

        #region ShowLicenseCommand
        ViewModelCommand _ShowLicenseCommand;

        public ViewModelCommand ShowLicenseCommand
        {
            get
            {
                if (_ShowLicenseCommand == null)
                    _ShowLicenseCommand = new ViewModelCommand(ShowLicense);
                return _ShowLicenseCommand;
            }
        }

        private void ShowLicense()
        {
            IsVisibleLicense = true;
        }
        #endregion

        #region HideLicenseCommand
        ViewModelCommand _HideLicenseCommand;

        public ViewModelCommand HideLicenseCommand
        {
            get
            {
                if (_HideLicenseCommand == null)
                    _HideLicenseCommand = new ViewModelCommand(HideLicense);
                return _HideLicenseCommand;
            }
        }

        private void HideLicense()
        {
            IsVisibleLicense = false;
        }
        #endregion

        private bool _isVisibleContributors = false;
        public bool IsVisibleContributors
        {
            get { return this._isVisibleContributors; }
            set
            {
                this._isVisibleContributors = value;
                RaisePropertyChanged(() => IsVisibleContributors);
            }
        }

        #region ShowContributorsCommand
        private ViewModelCommand _ShowContributorsCommand;

        public ViewModelCommand ShowContributorsCommand
        {
            get
            {
                if (_ShowContributorsCommand == null)
                {
                    _ShowContributorsCommand = new ViewModelCommand(ShowContributors);
                }
                return _ShowContributorsCommand;
            }
        }

        public void ShowContributors()
        {
            IsVisibleContributors = true;
            // バインド更新
            RaisePropertyChanged(() => Contributors);
        }
        #endregion

        #region HideContributorsCommand
        private ViewModelCommand _HideContributorsCommand;

        public ViewModelCommand HideContributorsCommand
        {
            get
            {
                if (_HideContributorsCommand == null)
                {
                    _HideContributorsCommand = new ViewModelCommand(HideContributors);
                }
                return _HideContributorsCommand;
            }
        }

        public void HideContributors()
        {
            IsVisibleContributors = false;
        }
        #endregion
    }

    public class ContributorViewModel
    {
        public string Name { get; private set; }
        private string screen;

        public ContributorViewModel(string name, string screen = null)
        {
            if (!String.IsNullOrEmpty(screen))
                this.Name = name + "(" + screen + ")";
            else
                this.Name = name;
            this.screen = screen;
        }

        public bool IsLinkEnabled
        {
            get { return !String.IsNullOrEmpty(screen); }
        }

        #region OpenLinkCommand
        private ViewModelCommand _OpenLinkCommand;

        public ViewModelCommand OpenLinkCommand
        {
            get
            {
                if (_OpenLinkCommand == null)
                {
                    _OpenLinkCommand = new ViewModelCommand(OpenLink);
                }
                return _OpenLinkCommand;
            }
        }

        public void OpenLink()
        {
            if (IsLinkEnabled)
            {
                Browser.Start("http://twitter.com/" + screen);
            }

        }
        #endregion
    }

    public enum VersionCheckState
    {
        /// <summary>
        /// バージョンをチェックしています
        /// </summary>
        Checking,
        /// <summary>
        /// 更新はありません
        /// </summary>
        Finished,
        /// <summary>
        /// 更新をダウンロード中です
        /// </summary>
        Downloading,
        /// <summary>
        /// 更新準備ができました
        /// </summary>
        Ready,
        /// <summary>
        /// 更新情報を取得できません
        /// </summary>
        Failed
    }

    public enum ReleaseKind
    {
        Stable,
        Daybreak,
        Midnight,
        PitchDark,
        Special
    }
}
