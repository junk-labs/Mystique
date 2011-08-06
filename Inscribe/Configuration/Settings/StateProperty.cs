using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Configuration.Tabs;
using System.Xml.Serialization;
using Inscribe.Filter;
using Inscribe.Filter.Filters.ScreenName;
using System.Windows;
using Inscribe.Filter.Filters.Attributes;

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
                    new TabProperty(){
                        Name = "Home",
                        TweetSources = new IFilter[]{
                            new FilterFollowFrom("*"),
                            new FilterMention("*"),
                            new FilterUser("*")
                        }
                    },
                    new TabProperty(){
                        Name="Mentions",
                        TweetSources = new IFilter[]{
                            new FilterCluster(){
                                Filters = new IFilter[]{
                                    new FilterMention("*"),
                                    new FilterRetweeted(){ Negate = true }
                                }, ConcatenateAnd = true
                            }
                        }
                    },
                    new TabProperty(){
                        Name="Messages",
                        TweetSources = new IFilter[]{
                            new FilterDirectMessage()
                        }
                    }
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
