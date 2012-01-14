using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Inscribe.Configuration.Tabs;
using Inscribe.Filter;
using Inscribe.Filter.Filters.Attributes;
using Inscribe.Filter.Filters.ScreenName;

namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// 容易に変化する設定項目
    /// </summary>
    public class StateProperty
    {
        public StateProperty()
        {
            WindowPosition = Rect.Empty;
            WindowState = System.Windows.WindowState.Normal;
            this.TabInformations = new[]{
                new[]{
                    new TabProperty("Home",
                            new IFilter[]{
                                new FilterFollowFrom("*"),
                                new FilterTo("*"),
                                new FilterUserScreenName("*"),
                                new FilterDirectMessage()}),
                    new TabProperty("Mentions",
                        new IFilter[]{
                            new FilterCluster(
                                new IFilter[]{
                                    new FilterTo("*"),
                                    new FilterRetweeted(){ Negate = true }
                                }, concatAnd: true)}),
                    new TabProperty("Messages",
                        new IFilter[]{
                            new FilterDirectMessage()
                        })
                }
            };
        }

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

        public Rect WindowPosition { get; set; }

        public WindowState WindowState { get; set; }
    }
}
