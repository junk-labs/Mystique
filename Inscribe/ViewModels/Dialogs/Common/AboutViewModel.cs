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
                if (UpdateReceiver.IsUpdateExists())
                {
                    CheckState = VersionCheckState.Downloading;
                    UpdateReceiver.DownloadUpdate();
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
            var apppath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            // アップデータの存在を確認
            var updater = System.IO.Path.Combine(apppath, "kup.exe");
            if (System.IO.File.Exists(updater))
            {
                // アップデータを起動して終了
                System.Diagnostics.Process.Start(updater, Define.GetNumericVersion().ToString() + " " + System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
                Application.Current.Shutdown();
            }
            else
            {
                throw new InvalidOperationException("Updater unexisted.");
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
        ReleaseCandidate,
        Beta,
        Daybreak,
        Twilight,
        Midnight,
        Special
    }
}
