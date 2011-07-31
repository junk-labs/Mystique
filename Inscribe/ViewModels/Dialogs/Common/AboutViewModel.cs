using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Inscribe;
using Inscribe.Common;
using Livet;
using Livet.Commands;

namespace Inscribe.ViewModels.Dialogs.Common
{
    public class AboutViewModel : ViewModel
    {
        public AboutViewModel()
        {
            Task.Factory.StartNew(() => CheckUpdate());
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
        DelegateCommand<string> _ShowBrowserCommand;

        public DelegateCommand<string> ShowBrowserCommand
        {
            get
            {
                if (_ShowBrowserCommand == null)
                    _ShowBrowserCommand = new DelegateCommand<string>(ShowBrowser);
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
        DelegateCommand _AppUpdateCommand;

        public DelegateCommand AppUpdateCommand
        {
            get
            {
                if (_AppUpdateCommand == null)
                    _AppUpdateCommand = new DelegateCommand(AppUpdate);
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
        DelegateCommand _ShowLicenseCommand;

        public DelegateCommand ShowLicenseCommand
        {
            get
            {
                if (_ShowLicenseCommand == null)
                    _ShowLicenseCommand = new DelegateCommand(ShowLicense);
                return _ShowLicenseCommand;
            }
        }

        private void ShowLicense()
        {
            IsVisibleLicense = true;
        }
        #endregion


        #region HideLicenseCommand
        DelegateCommand _HideLicenseCommand;

        public DelegateCommand HideLicenseCommand
        {
            get
            {
                if (_HideLicenseCommand == null)
                    _HideLicenseCommand = new DelegateCommand(HideLicense);
                return _HideLicenseCommand;
            }
        }

        private void HideLicense()
        {
            IsVisibleLicense = false;
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
        ReleaseCandidate,
        Beta,
        Daybreak,
        Twilight,
        Midnight,
        Special
    }
}
