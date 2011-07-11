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
        public IEnumerable<IEnumerable<TabProperty>> TabInformations { get; private set; }

        [XmlIgnore()]
        public Func<IEnumerable<IEnumerable<TabProperty>>> TabPropertyProvider { get; set; }

        [XmlArray("Columns"), XmlArrayItem("Column")]
        public TabProperty[][] _TabColumnsArray
        {
            get
            {
                return this.TabPropertyProvider().Select(i => i.ToArray()).ToArray();
            }
            set
            {
                this.TabInformations = value;
            }
        }

        /// <summary>
        /// サイレントモードにあるか
        /// </summary>
        public bool IsInSilentMode { get; set; }

    }
}
