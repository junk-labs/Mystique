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
        }

        public bool TabNotifyEnabledAsDefault { get; set; }

        public bool TabNotifyStackTopTimeline { get; set; }
    }
}
