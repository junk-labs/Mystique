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
            this.UseUserStreams = true;
            this.AutoCruiseDefaultMu = 0.5;
            this.AutoCruiseApiConsumeRate = 0.8;
        }

        public bool UserStreamsRepliesAll { get; set; }

        public bool UseUserStreams { get; set; }

        public double AutoCruiseDefaultMu { get; set; }

        public double AutoCruiseApiConsumeRate { get; set; }
    }
}
