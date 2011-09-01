using System.ComponentModel;
using System.Linq;
using Inscribe.Authentication;
using Inscribe.Storage;

namespace Inscribe.Configuration.Settings
{
    public class ConnectionProperty
    {
        public ConnectionProperty()
        {
            UserStreamsConnectionFailedInitialWaitSec = 30;
            UserStreamsConnectionFailedMaxWaitSec = 300;
            AutoRetryCount = 3;
            AutoRetryIntervalMSec = 5000;
        }

        /// <summary>
        /// 登録済みアカウント一覧<para />
        /// ここから変更しないでください。AccountStorageで集中的に管理しています。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public AccountInfo[] Accounts
        {
            get { return AccountStorage.Accounts.ToArray(); }
            set
            {
                if (value != null)
                {
                    value.ForEach(a => AccountStorage.RegisterAccount(a));
                }
            }
        }

        /// <summary>
        /// UserStreams接続に失敗したときの初期待機時間
        /// </summary>
        public int UserStreamsConnectionFailedInitialWaitSec { get; set; }

        /// <summary>
        /// UserStreams接続に失敗したときの最大待機時間
        /// </summary>
        public int UserStreamsConnectionFailedMaxWaitSec { get; set; }

        /// <summary>
        /// APIアクセス自動リトライの最大試行回数
        /// </summary>
        public int AutoRetryCount { get; set; }

        /// <summary>
        /// APIアクセス自動リトライの際の待機時間
        /// </summary>
        public int AutoRetryIntervalMSec { get; set; }
    }
}
