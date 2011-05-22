using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    public class TwitterProperty
    {
        public TwitterProperty()
        {
            this.OverrideDefaultValue = false;
        }

        public bool OverrideDefaultValue { get; set; }

        private int _tweetTextMaxLength = 140;
        private readonly int _tweetTextDefaultMaxLength = 140;
        public int TweetTextMaxLength
        {
            get { return this.OverrideDefaultValue ? this._tweetTextMaxLength : this._tweetTextDefaultMaxLength; }
            set { this._tweetTextMaxLength = value; }
        }

        private TimeSpan _underControlTimeSpan = new TimeSpan(3, 0, 0);
        public readonly TimeSpan _underControlDefaultTimeSpan = new TimeSpan(3, 0, 0);
        public TimeSpan UnderControlTimespan
        {
            get { return this.OverrideDefaultValue ? this._underControlTimeSpan : this._underControlDefaultTimeSpan; }
            set { this._underControlTimeSpan = value; }
        }

        private int _underControlCount = 127;
        private readonly int _underControlDefaultCount = 127;
        public int UnderControlCount
        {
            get { return this.OverrideDefaultValue ? this._underControlCount : this._underControlDefaultCount; }
            set { this._underControlCount = value; }
        }

        private int _underControlWarningThreshold = 117;
        private readonly int _underControlWarningDefaultThreshold = 117;
        public int UnderControlWarningThreshold
        {
            get { return this.OverrideDefaultValue ? this._underControlWarningThreshold : this._underControlWarningDefaultThreshold; }
            set { this._underControlWarningThreshold = value; }
        }
    }
}
