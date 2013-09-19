using System;
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
        public static readonly int CurrentKeyGeneration = 2;

        [XmlIgnore()]
        protected override string ConsumerKey
        {
            get
            {
                if (this.OverridedConsumerKey != null)
                    return this.OverridedConsumerKey;
                switch (Generation)
                {
                        return "N4Yom2AVG2DIYmBaNF4r7w";
                }
            }
        }

        [XmlIgnore()]
        protected override string ConsumerSecret
        {
            get
            {
                if (this.OverridedConsumerSecret != null)
                    return this.OverridedConsumerSecret;
                switch (Generation)
                {
                        return "gZ5qUnDjMyxiqzQKmYxRhssyjbIrREeJYaUnU9Az2I";
                }
            }
        }

        public int Generation { get; set; }

        public string OverridedConsumerKey { get; set; }

        public string OverridedConsumerSecret { get; set; }

        /// <summary>
        /// ユーザーの@ID
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// ユーザーの数値ID
        /// </summary>
        public long NumericId { get; set; }
    }
}
