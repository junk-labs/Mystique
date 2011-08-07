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
            this.TabNotifyEnabledAsDefault = false;
            this.TabNotifyStackTopTimeline = false;
            this.IsEnabledNotificationBar = true;

            this.NotifyMentionEvent = false;
            this.NotifyDmEvent = true;
            this.NotifyRetweetEvent = true;
            this.NotifyFavoriteEvent = true;

            this.NotifyMention = true;
            this.NotifyDm = true;
            this.NotifyRetweet = true;
            this.NotifyFavorite = true;
            this.NotifyReceives = true;

            this.IsShowMultiple = true;
            this.NotifyInMainWindowDisplay = false;
            this.NotifyLocation = Settings.NotifyLocation.RightBottom;
            this.NotifyWindowShowLength = 3000;

            this.WindowNotificationStrategy = NotificationStrategy.OnlyInactive;
            this.SoundNotificationStrategy = NotificationStrategy.Always;
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

        public bool IsShowMultiple { get; set; }

        public bool NotifyInMainWindowDisplay { get; set; }

        public NotifyLocation NotifyLocation { get; set; }

        public int NotifyWindowShowLength { get; set; }

        public NotificationStrategy WindowNotificationStrategy { get; set; }

        public NotificationStrategy SoundNotificationStrategy { get; set; }
    }

    public enum NotifyLocation
    {
        LeftTop,
        LeftBottom,
        RightTop,
        RightBottom,
    }

    public enum NotificationStrategy
    {
        Always,
        OnlyInactive,
        Disabled
    }
}
