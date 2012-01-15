using System;

namespace Inscribe
{
    /// <summary>
    /// Twitterの(半)静的値
    /// </summary>
    public static class TwitterDefine
    {
        #region ほぼ固定されている値

        public static readonly int TweetMaxLength = 140;

        #endregion

        #region Twitter API 依存

        public static readonly int UserStreamsQueryMaxCount = 200;

        public static readonly int HomeReceiveMaxCount = 200;

        public static readonly int MentionReceiveMaxCount = 100;

        public static readonly int DmReceiveMaxCount = 50;

        public static readonly int DefaultReceiveCount = 20;

        public static readonly int ListReceiveCount = 100;

        #endregion

        #region 規制管理

        public static readonly TimeSpan UnderControlTimespan = new TimeSpan(3, 0, 0);

        public static readonly int UnderControlCount = 127;

        public static readonly int UnderControlWarningThreshold = 122;

        #endregion

        #region ミリ秒単位

        public static readonly int UserInformationRefreshPeriod = 1000 * 60 * 60 * 6;

        #endregion

        #region Cluise Control

        public static readonly double MinNewbiesRate = 0.01; // 1%
        public static readonly double TimesPerTweetMaximumValue = 1000 * 60 * 60; // 1hに1ツイートはあることにする
        public static readonly int MinWindowTime = 5 * 1000;
        public static readonly int IntervalLookPrevious = 10;
        public static readonly double MinDensity = 0.1;
        public static readonly int UserStreamsTimeout = 90;
        public static readonly int UrlMaxLength = 20;

        #endregion

        #region その他

        public static readonly string TwitterApiStatusUrl = "https://dev.twitter.com/status";

        public static readonly string FavstarUrl = "http://favstar.fm/users/";

        public static readonly string TwilogUrl = "http://twilog.org/";

        public static readonly int TimelineDispatchBlockCount = 5000;

        public static readonly char[] TrimmingChars = new[] { ' ', '\t', '\r', '\n' };

        #endregion
    }
}
