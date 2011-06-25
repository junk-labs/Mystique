
namespace Inscribe
{
    /// <summary>
    /// Twitterの(半)静的値
    /// </summary>
    public static class TwitterDefine
    {
        #region Twitter API 依存

        public static readonly int UserStreamsQueryMaxCount = 200;

        public static readonly int HomeReceiveMaxCount = 200;

        public static readonly int MentionReceiveMaxCount = 200;

        public static readonly int DmReceiveMaxCount = 200;

        public static readonly int DefaultReceiveCount = 20;

        public static readonly int ListReceiveCount = 200;

        #endregion


        #region ミリ秒単位

        public static readonly int UserInformationRefreshPeriod = 1000 * 60 * 60 * 6;

        #endregion

        #region Cluise Control

        public static double MinNewbiesRate = 0.01; // 1%

        #endregion
    }
}
