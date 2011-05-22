using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// 容易に変化する設定項目
    /// </summary>
    public class StateProperty
    {
        public StateProperty()
        {
            SideViewWidth = 350;
            IsSideViewInLeft = false;
        }

        public double SideViewWidth { get; set; }

        public bool IsSideViewInLeft { get; set; }

        // public Data.TabData[] TabDatas { get; set; }

        /// <summary>
        /// サイレントモードにあるか
        /// </summary>
        public bool IsInSilentMode { get; set; }

    }
}
