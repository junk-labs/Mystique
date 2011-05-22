using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Elements
{
    /// <summary>
    /// アカウント情報に付随する設定
    /// </summary>
    public class AccountProperty
    {
        public AccountProperty()
        {
            this.UserStreamsRepliesAll = false;
            this.ConnectionKind = Elements.ConnectionKind.UserStreams;
            this.RestHomeInterval = TwitterStatic.RestHomeInterval;
            this.RestMentionsInterval = TwitterStatic.RestMentionsInterval;
            this.RestDirectMessagesInterval = TwitterStatic.RestDirectMessagesInterval;
            this.RestFavoredInterval = TwitterStatic.RestFavoredInterval;
        }

        public bool UserStreamsRepliesAll { get; set; }

        public ConnectionKind ConnectionKind { get; set; }

        public int RestHomeInterval { get; set; }
        public int RestMentionsInterval { get; set; }
        public int RestDirectMessagesInterval { get; set; }
        public int RestFavoredInterval { get; set; }

        public int? UserStreamsOverrideRestHomeInterval { get; set; }
        public int? UserStreamsOverrideRestMentionsInterval { get; set; }
        public int? UserStreamsOverrideRestDirectMessagesInterval { get; set; }
        public int? UserStreamsOverrideRestFavoredInterval { get; set; }
    }

    public enum ConnectionKind
    {
        None,
        Rest,
        UserStreams,
    }
}
