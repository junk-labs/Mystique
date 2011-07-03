using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;
using System.Threading;

namespace Inscribe.Configuration.Tabs
{
    /// <summary>
    /// タブの設定
    /// </summary>
    public class TabProperty
    {
        #region LinkAccountInfoChangedイベント

        public event EventHandler<EventArgs> LinkAccountInfoChanged;
        private Notificator<EventArgs> _LinkAccountInfoChangedEvent;
        public Notificator<EventArgs> LinkAccountInfoChangedEvent
        {
            get
            {
                if (_LinkAccountInfoChangedEvent == null) _LinkAccountInfoChangedEvent = new Notificator<EventArgs>();
                return _LinkAccountInfoChangedEvent;
            }
            set { _LinkAccountInfoChangedEvent = value; }
        }

        protected void OnLinkAccountInfoChanged(EventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref LinkAccountInfoChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(this, e);
            LinkAccountInfoChangedEvent.Raise(e);
        }

        #endregion
      
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

        private IEnumerable<IFilter> _tweetSources;
        /// <summary>
        /// ツイートのソース
        /// </summary>
        [XmlIgnore()]
        public IEnumerable<IFilter> TweetSources
        {
            get { return this._tweetSources ?? new IFilter[0]; }
            set
            {
                if (this._tweetSources == value) return;
                if (this._tweetSources != null)
                    this._tweetSources.ForEach(f => f.Dispose());
                this._tweetSources = value;
            }
        }

        public IEnumerable<string> TweetSourceQueries
        {
            get { return this.TweetSources.Select(s => s.ToQuery()).ToArray(); }
            set
            {
                if (value == null) return;
                this.TweetSources = GenerateFilters(value).ToArray();
            }
        }

        private IEnumerable<FilterCluster> GenerateFilters(IEnumerable<string> queries)
        {
            foreach (var s in queries)
            {
                FilterCluster cluster = null;
                try
                {
                    cluster = QueryCompiler.ToFilter(s);
                }
                catch(Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError, "フィルタ クエリを読み取れません: " + s);
                }
                if (cluster != null)
                    yield return cluster;
            }
        }

        private IEnumerable<string> _linkAccountScreenNames;
        /// <summary>
        /// リンクしているアカウントのスクリーン名
        /// </summary>
        public IEnumerable<string> LinkAccountScreenNames
        {
            get { return this._linkAccountScreenNames ?? new string[0]; }
            set { this._linkAccountScreenNames = value; }
        }

        [XmlIgnore()]
        public IEnumerable<AccountInfo> LinkAccountInfos
        {
            get { return this.LinkAccountScreenNames.Select(s => AccountStorage.Get(s)).Where(i => i != null).Distinct().ToArray(); }
            set
            {
                if (value == null)
                    this.LinkAccountScreenNames = new string[0];
                else
                    this.LinkAccountScreenNames = value.Select(i => i.ScreenName).Distinct();
                OnLinkAccountInfoChanged(EventArgs.Empty);
            }
        }
    }
}
