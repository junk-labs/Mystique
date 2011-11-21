using System;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter;
using Inscribe.Filter.Core;
using Livet;

namespace Inscribe.Filter
{
    /// <summary>
    /// フィルタベース
    /// </summary>
    public abstract class FilterBase : IFilter
    {
        public virtual bool IsOnlyTranscender
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// フィルタを適用します。
        /// </summary>
        /// <returns>フィルタを通過したか</returns>
        public bool Filter(TwitterStatusBase status)
        {
            return FilterStatus(status) == !Negate;
        }

        protected abstract bool FilterStatus(TwitterStatusBase status);

        /// <summary>
        /// 現在適用中のフィルタを破棄し、フィルタを再適用するように要求されました。
        /// </summary>
        public event Action RequireReaccept = () => { };

        public event Action<TwitterStatusBase> RequirePartialReaccept = _ => { };

        /// <summary>
        /// フィルタの再適用要求を発行します。
        /// </summary>
        protected void RaiseRequireReaccept()
        {
            RequireReaccept();
        }

        /// <summary>
        /// 指定したツイートをこのフィルタを保持しているクラスタに再度通します。<para />
        /// 存在していないツイートがフィルタを通れば追加され、存在しているツイートがフィルタを通らなくなれば除去されます。
        /// </summary>
        protected void RaisePartialRequireReaccept(TwitterStatusBase tsb)
        {
            RequirePartialReaccept(tsb);
        }

        /// <summary>
        /// このフィルタは否定条件です<para />
        /// (Filter()メソッドで考慮されます)
        /// </summary>
        public bool Negate { get; set; }

        /// <summary>
        /// このフィルタを識別する文字列です。
        /// </summary>
        public abstract string Identifier { get; }

        public string ToQuery()
        {
            var arg = String.Join(", ", from a in this.GetArgumentsForQueryify() select this.GetQueryString(a));
            if (!String.IsNullOrEmpty(arg))
                return Identifier + (this.Negate ? "!" : "") + ":" + arg;
            else
                return Identifier + (this.Negate ? "!" : "");
        }

        public abstract IEnumerable<object> GetArgumentsForQueryify();

        private string GetQueryString(object o)
        {
            if (o is string)
            {
                // escape double quote
                return ((string)o).EscapeForQuery().Quote();
            }
            else if (o is bool)
            {
                return (bool)o ? "true" : "false";
            }
            else if (o == null)
            {
                return String.Empty;
            }
            else
            {
                return o.ToString();
            }
        }

        /// <summary>
        /// このフィルタの簡単な説明(GUI編集用)
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// このフィルタの設定状態(GUI編集用)
        /// </summary>
        public abstract string FilterStateString { get; }
    }
}
