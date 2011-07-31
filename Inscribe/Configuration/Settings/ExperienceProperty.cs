
namespace Inscribe.Configuration.Settings
{
    public class ExperienceProperty
    {
        public ExperienceProperty()
        {
            this.PostFinishShowLength = 3000;
            this.UpdateKind = Define.GetVersion().FileBuildPart;
            this.PostAnnotatedFinishShowLength = 8000;
            this.TwitterActionNotifyShowLength = 5000;
            this.PowerUserMode = false;
            this.StatusMessageDefaultShowLengthSec = 5;
        }

        #region General experience

        /// <summary>
        /// 上級者向けの解説を使用する
        /// </summary>
        public bool PowerUserMode { get; set; }


        public int UpdateKind { get; set; }

        #endregion

        #region Callback show expereience

        public int PostFinishShowLength { get; set; }

        public int PostAnnotatedFinishShowLength { get; set; }

        public int TwitterActionNotifyShowLength { get; set; }

        #endregion

        #region Paticular experience

        public int StatusMessageDefaultShowLengthSec { get; set; }

        #endregion
    }
}
