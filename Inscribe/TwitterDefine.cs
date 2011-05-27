using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe
{
    /// <summary>
    /// Twitterの(半)静的値
    /// </summary>
    public static class TwitterDefine
    {
        #region Twitter API 依存

        public static int UserStreamsQueryMaxCount = 200;

        #endregion

        #region 秒単位

        public static readonly int RestHomeInterval = 40;
        public static readonly int RestMentionsInterval = 60;
        public static readonly int RestDirectMessagesInterval = 60 * 10;
        public static readonly int RestFavoredInterval = 60 * 5;

        public static readonly int UserStreamsRestHomeInterval = 60 * 2;
        public static readonly int UserStreamsRestMentionsInterval = 60 * 2;
        public static readonly int UserStreamsRestDirectMessagesInterval = 60 * 10;
        public static readonly int UserStreamsRestFavoredInterval = 60 * 10;

        #endregion

        #region ミリ秒単位

        public static readonly int UserInformationRefreshPeriod = 1000 * 60 * 60 * 6;

        #endregion
    }
}
