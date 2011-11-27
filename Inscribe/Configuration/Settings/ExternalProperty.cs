using System;

namespace Inscribe.Configuration.Settings
{
    public class ExternalProperty
    {
        public ExternalProperty()
        {
            UploaderService = String.Empty;
            this.WebBrowserPath = String.Empty;
            this.WebBrowserParam = String.Empty;
        }

        public string UploaderService { get; set; }

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
