using System;
using System.Xml.Serialization;
using Inscribe.Filter;
using Inscribe.Filter.QuerySystem;
using Inscribe.Storage;

namespace Inscribe.Configuration.Settings
{
    /// <summary>
    /// タイムラインのフィルタを行う
    /// </summary>
    public class TimelineFilterlingProperty
    {
        public TimelineFilterlingProperty()
        {
            this.MuteFilterCluster = new FilterCluster() { Negate = true, ConcatenateOR = false };

            this.RedStarUsers = new string[0];
            this.BlueStarUsers = new string[0];
            this.GreenStarUsers = new string[0];
        }

        /// <summary>
        /// ミュートフィルタ
        /// </summary>
        [XmlIgnore()]
        public FilterCluster MuteFilterCluster { get; set; }

        public string MuteFilterClusterQueryString
        {
            get { return this.MuteFilterCluster.ToQuery(); }
            set
            {
                try
                {
                    this.MuteFilterCluster = QueryConverter.ToFilter(value);
                }
                catch (Exception e)
                {
                    NotifyStorage.Notify(e.Message);
                }
            }
        }

        public void AddNewNGRule(FilterBase filter)
        {
            this.MuteFilterCluster.Join(filter);
        }

        public string[] RedStarUsers { get; set; }

        public string[] BlueStarUsers { get; set; }

        public string[] GreenStarUsers { get; set; }

        public event Action<StarKind> StarChanged = _ => { };

        public void RaiseStarChanged(StarKind kind)
        {
            StarChanged(kind);
        }
    }

    public enum StarKind
    {
        Red,
        Blue,
        Green
    }
}
