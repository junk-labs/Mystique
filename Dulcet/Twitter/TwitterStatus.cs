using System.Xml.Linq;
using Dulcet.Util;
using System;

namespace Dulcet.Twitter
{
    public sealed class TwitterStatus : TwitterStatusBase
    {
        #region ファクトリメソッド

        public static TwitterStatus FromNode(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return new TwitterStatus()
            {
                Id = node.Element("id").ParseLong(),
                Text = node.Element("text").ParseString(),
                Source = node.Element("source").ParseString(),
                CreatedAt = node.Element("created_at").ParseDateTime("ddd MMM d HH':'mm':'ss zzz yyyy"),
                InReplyToStatusId = node.Element("in_reply_to_status_id").ParseLong(),
                InReplyToUserId = node.Element("in_reply_to_user_id").ParseLong(),
                InReplyToUserScreenName = node.Element("in_reply_to_screen_name").ParseString(),
                RetweetedOriginal = node.Element("retweeted_status") == null ? null : TwitterStatus.FromNode(node.Element("retweeted_status")),
                User = TwitterUser.FromNode(node.Element("user")),
                Entities = TwitterEntity.Parse(node.Element("entities")),
            };
        }

        #endregion

        private TwitterStatus() { }

        public string Source { get; set; }

        public long InReplyToStatusId { get; set; }

        public long InReplyToUserId { get; set; }

        public string InReplyToUserScreenName { get; set; }

        public TwitterStatus RetweetedOriginal { get; set; }

        public override string ToString()
        {
            return "@" + this.User.ScreenName + ": " + this.Text;
        }
    }
}
