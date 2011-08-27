
namespace Inscribe.Configuration.Settings
{
    public class ExperienceProperty
    {
        public ExperienceProperty()
        {
            this.PostFinishShowLength = 3000;
            this.UpdateKind = Define.GetVersion().FileBuildPart;
            this.TwitterActionNotifyShowLength = 5000;
            this.IsPowerUserMode = false;
            this.StatusMessageDefaultShowLengthSec = 5;
        }

        #region General experience

        /// <summary>
        /// 上級者向けの解説を使用する
        /// </summary>
        public bool IsPowerUserMode { get; set; }

        /// <summary>
        /// 更新パッケージ種別
        /// </summary>
        /// <remarks>
        /// PowerUserModeがオンなら0-3で選択可能
        /// </remarks>
        public int UpdateKind { get; set; }

        #endregion

        #region Callback show expereience

        public int PostFinishShowLength { get; set; }

        public int TwitterActionNotifyShowLength { get; set; }

        #endregion

        #region Paticular experience

        public int StatusMessageDefaultShowLengthSec { get; set; }

        #endregion

    }
}
