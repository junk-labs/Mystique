using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class KernelProperty
    {
        public int ImageGCInitialDelay = 1000 * 60 * 3;

        public int ImageGCInterval = 1000 * 60;

        public int ImageLifetime = 1000 * 60 * 10;

        public int DeleteGCThreshold = 1000;

        public int CheckUnderControlledTimestampIntervalSec = 60;

        public long TweetGCInitialValue = 6000;

        public int TweetGCThreshold = 250;

        public int UserStreamsUnstableCount = 5;

        public int ReconnectionTimeout = 30000;

        public int HttpMaxConnection = 16;
    }
}
