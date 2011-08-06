using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class NotificationProperty
    {
        public NotificationProperty()
        {
            this.TabNotifyEnabledAsDefault = true;
            this.TabNotifyStackTopTimeline = false;
            this.IsEnabledNotificationBar = true;
            this.NotifyMentionEvent = true;
            this.NotifyDmEvent = true;
            this.NotifyRetweetEvent = true;
            this.NotifyFavoriteEvent = true;

            this.NotifyMention = true;
            this.NotifyDm = true;
            this.NotifyRetweet = true;
            this.NotifyFavorite = true;
            this.NotifyReceives = true;
        }

        public bool TabNotifyEnabledAsDefault { get; set; }

        public bool TabNotifyStackTopTimeline { get; set; }

        public bool IsEnabledNotificationBar { get; set; }

        public bool NotifyMentionEvent { get; set; }

        public bool NotifyDmEvent { get; set; }

        public bool NotifyRetweetEvent { get; set; }

        public bool NotifyFavoriteEvent { get; set; }

        public bool NotifyMention { get; set; }

        public bool NotifyDm { get; set; }

        public bool NotifyRetweet { get; set; }

        public bool NotifyFavorite { get; set; }

        public bool NotifyReceives { get; set; }
    }
}
