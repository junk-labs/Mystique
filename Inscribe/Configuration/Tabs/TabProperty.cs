using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Inscribe.Communication.CruiseControl.Lists;
using Inscribe.Communication.UserStreams;
using Inscribe.Filter;
using Inscribe.Filter.Core;
using Inscribe.Model;
using Inscribe.Storage;
using Livet;

namespace Inscribe.Configuration.Tabs
{
    /// <summary>
    /// タブの設定
    /// </summary>
    public class TabProperty
    {
        public TabProperty()
        {
            if (Setting.IsInitialized)
                this.LinkAccountInfos = AccountStorage.Accounts.Where(i => i.AccoutProperty.IsSelectedDefault).ToArray();
            this.Name = "Untitled";
            this.IsNotifyEnabled = Setting.IsInitialized ? Setting.Instance.NotificationProperty.IsTabNotifyEnabledAsDefault : false;
        }

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
                this._tweetSources = value;
            }
        }

        [XmlArray("TweetSource"), XmlArrayItem("Query"), EditorBrowsable(EditorBrowsableState.Never)]
        public string[] _TweetSourceQueries
        {
            get { return this.TweetSources.Select(s => s.ToQuery()).ToArray(); }
            set
            {
                if (value == null) return;
                Setting.AddAfterInitInvoke(() => this.TweetSources = GenerateFilters(value).ToArray());
            }
        }

        private string[] _streamingQueries = null;
        [XmlArray("StreamingQueries"), EditorBrowsable(EditorBrowsableState.Never)]
        public string[] _StreamingQueries
        {
            get { return this.StreamingQueries; }
            set
            {
                this.StreamingQueries = value;
                value.ForEach(q => ConnectionManager.AddQuery(q));
            }
        }
        [XmlIgnore()]
        public string[] StreamingQueries
        {
            get { return this._streamingQueries ?? new string[0]; }
            set { this._streamingQueries = value; }
        }

        private string[] _followingLists = null;
        [XmlArray("Lists"), XmlArrayItem("List"), EditorBrowsable(EditorBrowsableState.Never)]
        public string[] _FollowingLists
        {
            get { return this.FollowingLists; }
            set
            {
                this.FollowingLists = value;
                value.Select(s => s.Split(new[] { "/" }, StringSplitOptions.None))
                    .ForEach(s => ListReceiverManager.RegisterReceive(s[0], s[1]));
            }
        }
        [XmlIgnore()]
        public string[] FollowingLists
        {
            get { return this._followingLists ?? new string[0]; }
            set { this._followingLists = value; }
        }

        private IEnumerable<IFilter> GenerateFilters(IEnumerable<string> queries)
        {
            foreach (var s in queries)
            {
                FilterCluster cluster = null;
                try
                {
                    cluster = QueryCompiler.ToFilter(s);
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.ConfigurationError, "フィルタ クエリを読み取れません: " + s);
                }
                if (cluster != null)
                {
                    if (cluster.Filters.Count() == 1)
                    {
                        var filter = cluster.Filters.First();
                        if (cluster.Negate)
                            filter.Negate = !filter.Negate;
                        yield return filter;
                    }
                    else
                    {
                        yield return cluster;
                    }
                }
            }
        }

        private string[] _linkAccountScreenNames;
        /// <summary>
        /// リンクしているアカウントのスクリーン名
        /// </summary>
        public string[] LinkAccountScreenNames
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
                    this.LinkAccountScreenNames = value.Select(i => i.ScreenName).Distinct().ToArray();
                OnLinkAccountInfoChanged(EventArgs.Empty);
            }
        }
    }
}
