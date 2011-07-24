using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class TweetExperienceProperty
    {
        public TweetExperienceProperty()
        {
            ShowUnofficialRetweetButton = true;
            ShowQuoteButton = false;
            P3StyleIcon = true;
            NameAreaWidth = 120;
            UserNameMode = NameViewMode.ID;
            UrlResolving = UrlResolveStrategy.OnPointed;
            UrlTooltipShowLength = 60 * 1000;
        }

        public bool ShowUnofficialRetweetButton { get; set; }

        public bool P3StyleIcon { get; set; }

        public int NameAreaWidth { get; set; }

        public enum NameViewMode
        {
            ID,
            Name,
            Both
        }

        public NameViewMode UserNameMode { get; set; }

        public enum UrlResolveStrategy
        {
            /// <summary>
            /// URL短縮を解決しません
            /// </summary>
            Never,
            /// <summary>
            /// ツールチップ上で解決します
            /// </summary>
            OnPointed,
            /// <summary>
            /// テキスト表示で解決します
            /// </summary>
            OnReceived
        }

        public UrlResolveStrategy UrlResolving { get; set; }

        public int UrlTooltipShowLength { get; set; }

        public bool ShowQuoteButton { get; set; }
    }
}
