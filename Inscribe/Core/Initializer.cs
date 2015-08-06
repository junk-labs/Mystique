using System;
using System.Threading.Tasks;
using System.Windows;
using Inscribe.Common;
using Inscribe.Communication.UserStreams;
using Inscribe.Configuration;
using Inscribe.Plugin;
using Inscribe.Storage;
using Inscribe.Subsystems;

namespace Inscribe.Core
{
    public static class Initializer
    {
        private static bool initialized = false;

        /// <summary>
        /// ウィンドウが表示されるより前に行われる初期化処理
        /// </summary>
        public static void Init()
        {
            if (initialized)
                throw new InvalidOperationException("アプリケーションは既に初期化されています。");
            initialized = true;

            // ネットワーク初期化
            Dulcet.Network.Http.Expect100Continue = false;
            Dulcet.Network.Http.MaxConnectionLimit = Int32.MaxValue;

            // プラグインのロード
            PluginLoader.Load();

            // 設定のロード
            Setting.Initialize();

            // APIエンドポイントのオーバーライト
            if (!String.IsNullOrEmpty(Setting.Instance.KernelProperty.TwitterApiEndpoint))
            {
                try
                {
                    Dulcet.Twitter.Rest.Api.TwitterUri = Setting.Instance.KernelProperty.TwitterApiEndpoint;
                }
                catch (Exception ex)
                {
                    ExceptionStorage.Register(ex, ExceptionCategory.ConfigurationError,
                        "Twitter API エンドポイントの設定を行えませんでした。");
                }
            }

            // サブシステムの初期化
            NotificationCore.Initialize();
            HashtagStorage.Initialize();

            Application.Current.Exit += new ExitEventHandler(AppExit);
        }

        private static bool standby = false;

        /// <summary>
        /// ウィンドウが表示された後に行われる初期化処理
        /// </summary>
        public static void StandbyApp()
        {
            KeyAssignCore.ReloadAssign();
            if (standby)
                throw new InvalidOperationException("既にアプリケーションはスタンバイ状態を経ました。");
            standby = true;
            Task.Factory.StartNew(() => Inscribe.Communication.CruiseControl.AutoCruiseSchedulerManager.Begin());
            Task.Factory.StartNew(() => ConnectionManager.RefreshReceivers());
            var call = OnStandbyApp;
            if (call != null)
                call();
            KernelService.CallbackWindowPrepared();
        }

        public static event Action OnStandbyApp;

        static void AppExit(object sender, ExitEventArgs e)
        {
            Setting.Instance.Save();
        }
    }
}
