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
