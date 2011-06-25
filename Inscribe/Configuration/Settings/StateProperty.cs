using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Configuration.Tabs;
using System.Xml.Serialization;

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

        [XmlIgnore()]
        public IEnumerable<IEnumerable<TabProperty>> TabColumns { get; set; }

        [XmlArray("TabColumns")]
        public TabProperty[][] _TabColumnsArray
        {
            get
            {
                return this.TabColumns.Select(i => i.ToArray()).ToArray();
            }
            set
            {
                this.TabColumns = value;
            }
        }

        /// <summary>
        /// サイレントモードにあるか
        /// </summary>
        public bool IsInSilentMode { get; set; }

    }
}
