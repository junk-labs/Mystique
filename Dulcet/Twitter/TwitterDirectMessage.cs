using System.Xml.Linq;
using Dulcet.Util;
using System;

namespace Dulcet.Twitter
{
    public sealed class TwitterDirectMessage : TwitterStatusBase
    {
        #region ファクトリメソッド

        public static TwitterDirectMessage FromNode(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return new TwitterDirectMessage()
            {
                Id = node.Element("id").ParseLong(),
                Text = node.Element("text").ParseString(),
                CreatedAt = node.Element("created_at").ParseDateTime("ddd MMM d HH':'mm':'ss zzz yyyy"),
                Sender = TwitterUser.FromNode(node.Element("sender")),
                Recipient = TwitterUser.FromNode(node.Element("recipient")),
                Entities = TwitterEntity.Parse(node.Element("entities")),
            };
        }

        #endregion

        private TwitterDirectMessage() { }

        /// <summary>
        /// Userと同一です。
        /// </summary>
        public TwitterUser Sender
        {
            get { return this.User; }
            set { this.User = value; }
        }

        public TwitterUser Recipient { get; set; }

        public override string ToString()
        {
            return "@" + this.Sender.ScreenName + ": " + this.Text;
        }
    }
}
