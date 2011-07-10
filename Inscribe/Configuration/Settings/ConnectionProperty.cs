using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            ApiTweetReceiveCount = 200;
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

        /// <summary>
        /// APIを利用してツイートを受信する場合に受信する数
        /// </summary>
        public int ApiTweetReceiveCount { get; set; }
    }
}
