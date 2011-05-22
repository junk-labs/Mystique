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
        }

        public bool TabNotifyEnabledAsDefault { get; set; }
    }
}
