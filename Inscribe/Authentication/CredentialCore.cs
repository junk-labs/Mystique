using System.Xml.Serialization;
using Dulcet.Twitter.Credential;

namespace Inscribe.Authentication
{
    /// <summary>
    /// アカウント情報の基本的な部分を保持します。<para />
    /// アカウント情報の一時的な保持に利用されます。
    /// </summary>
    public class CredentialCore : OAuth
    {
        [XmlIgnore()]
        protected override string ConsumerKey
        {
            get
            {
                if (this.OverridedConsumerKey != null)
                    return this.OverridedConsumerKey;
                else
                    return "K3FZBXVOzsm271KC1jPPHA";
            }
        }

        [XmlIgnore()]
        protected override string ConsumerSecret
        {
            get
            {
                if (this.OverridedConsumerSecret != null)
                    return this.OverridedConsumerSecret;
                else
                    return "tOJVCdRrlzc08WilwcU5BtwuGzgbo2MlTWJIFRYaeow";
            }
        }

        public string OverridedConsumerKey { get; set; }

        public string OverridedConsumerSecret { get; set; }

        /// <summary>
        /// ユーザーの@ID
        /// </summary>
        public string ScreenName { get; set; }
    }
}
