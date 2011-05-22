using System.Xml.Serialization;
using Dulcet.Twitter.Credential;

namespace Romance.Credential
{
    public class Credential : OAuth
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
