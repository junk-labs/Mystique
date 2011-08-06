using System;
using System.Xml.Serialization;
using Inscribe.Filter;
using Inscribe.Filter.Core;
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
            this.MuteFilterCluster = new FilterCluster() { Negate = true, ConcatenateAnd = false };
            this.ShareBlocking = false;
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
                    this.MuteFilterCluster = QueryCompiler.ToFilter(value);
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.UserError, "クエリのコンパイルに失敗しました。(クエリ:" + value + ")");
                }
            }
        }

        public void AddNewNGRule(FilterBase filter)
        {
            this.MuteFilterCluster.Join(filter);
        }

        /// <summary>
        /// ブロックを共有する
        /// </summary>
        public bool ShareBlocking { get; set; }
    }
}