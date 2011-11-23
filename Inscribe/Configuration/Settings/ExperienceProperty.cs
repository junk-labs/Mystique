
using System;
namespace Inscribe.Configuration.Settings
{
    public class ExperienceProperty
    {
        public ExperienceProperty()
        {
            this.PostFinishShowLength = 3000;
            this.UpdateKind = Define.GetVersion().FileBuildPart;
            this.TwitterActionNotifyShowLength = 5000;
            this.IsTranscender = false;
            this.StatusMessageDefaultShowLengthSec = 5;
            this.FontSize = 11.5;
            this.FontFamily = "Meiryo";
            this.IgnoreTimeoutError = true;
            this.WebBrowserPath = string.Empty;
            this.WebBrowserParam = string.Empty;
        }

        #region General experience

        /// <summary>
        /// 超越者である
        /// </summary>
        public bool IsTranscender { get; set; }

        /// <summary>
        /// 上級者向けの解説を使用する
        /// </summary>
        [Obsolete("use IsTranscender property.")]
        public bool IsAloofUserMode
        {
            get { return IsTranscender; }
            set { IsTranscender = value; }
        }

        /// <summary>
        /// 更新パッケージ種別
        /// </summary>
        /// <remarks>
        /// IsTranscenderがオンなら0-3で選択可能
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

        public string FontFamily { get; set; }

        public double FontSize { get; set; }

        public bool IgnoreTimeoutError { get; set; }

        /// <summary>
        /// URLを開くWebブラウザのパス
        /// </summary>
        /// <remarks>未設定の場合関連付けで開く</remarks>
        public string WebBrowserPath { get; set; }

        /// <summary>
        /// URLを開くWebブラウザに渡すパラメータ
        /// </summary>
        /// <remarks>{URL}はURLに置き換わる。未設定の場合はURLがそのまま指定される</remarks>
        public string WebBrowserParam { get; set; }

    }
}
