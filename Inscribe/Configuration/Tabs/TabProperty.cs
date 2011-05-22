using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter;

namespace Inscribe.Configuration.Tabs
{
    /// <summary>
    /// タブの設定
    /// </summary>
    public class TabProperty
    {
        /// <summary>
        /// タブ名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// タブがロック状態にあるか
        /// </summary>
        public bool IsLock { get; set; }

        /// <summary>
        /// タブの通知が有効か
        /// </summary>
        public bool IsNotifyEnabled { get; set; }

        /// <summary>
        /// タブ通知の際に利用する音
        /// </summary>
        public string NotifySoundPath { get; set; }

        /// <summary>
        /// ツイートのソース
        /// </summary>
        IEnumerable<IFilter> TweetSources { get; set; }
    }
}
