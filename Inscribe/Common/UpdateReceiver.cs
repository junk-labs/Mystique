using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Inscribe.Storage;
using System.Net;

namespace Inscribe.Common
{
    public static class UpdateReceiver
    {
        static Timer updateCheckTimer = null;

        public static void Start()
        {
            updateCheckTimer = new Timer(CheckUpdate, null, 1000 * 10, 1000 * 60 * 60 * 3);
        }

        private static void CheckUpdate(object o)
        {
            try
            {
                if (IsUpdateExists())
                {
                    DownloadUpdate();
                }
            }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, "Krileの更新確認に失敗しました。", () => CheckUpdate(null));
            }
        }

        public static bool IsUpdateExists()
        {
            var update = Dulcet.Network.Http.WebConnectDownloadString(new Uri(Define.RemoteVersionUrl));
            if (!update.Succeeded || String.IsNullOrWhiteSpace(update.Data))
                throw new Exception("バージョンを確認できません。");
            var remoteVersion = Double.Parse(update.Data);
            var localVersion = Define.GetNumericVersion();
            return remoteVersion > localVersion;
        }

        public static void DownloadUpdate()
        {
            // 更新がありました
            using (var msg = NotifyStorage.NotifyManually("Krileの更新が見つかりました。ダウンロードしています..."))
            {
                var apppath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                var downloader = new WebClient();
                downloader.DownloadFile(Define.RemoteUpdaterUrl, System.IO.Path.Combine(apppath, "kup.exe"));
            }
            NotifyStorage.Notify("Krileは次回起動時に更新されます。");
        }
    }
}