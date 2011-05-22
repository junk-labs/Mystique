using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Filter.QuerySystem;
using Dulcet.Twitter;

namespace Inscribe.Filter
{
    /// <summary>
    /// フィルタベース
    /// </summary>
    public abstract class FilterBase : IFilter, QuerySystem.IQueryElement, GuiSystem.IGuiElement
    {
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

        /// <summary>
        /// フィルタの再適用要求を発行します。
        /// </summary>
        protected void RaiseRequireReaccept()
        {
            RequireReaccept();
        }

        /// <summary>
        /// このフィルタは否定条件です<para />
        /// (Filter()メソッドで考慮されます)
        /// </summary>
        public bool Negate { get; set; }

        public abstract string Namespace { get; }

        public abstract string Class { get; }

        /// <summary>
        /// このフィルタにただ一つ含まれるクエリシステムに対して公開されているメソッドを示します。<para />
        /// オーバーライドされると、そのフィルタが公開するいくつかのメソッドのうち、現在フィルタが使用しているメソッド名を示します。
        /// </summary>
        /// <returns>フィルタメソッド名</returns>
        public virtual string GetCurrentMethod()
        {
            List<string> methCandidate = new List<string>();
            foreach(var meth in this.GetType().GetMethods())
            {
                if (meth.IsSpecialName || !meth.IsPublic || meth.IsAbstract) continue;
                foreach (var attr in Attribute.GetCustomAttributes(meth, typeof(MethodVisibleAttribute)))
                {
                    var openmeth = attr as MethodVisibleAttribute;
                    if (openmeth != null)
                    {
                        methCandidate.Add(meth.Name);
                        break;
                    }
                }
            }
            if (methCandidate.Count == 0)
                throw new InvalidOperationException("型 " + this.GetType().Name + " において、クエリシステムから参照可能なメソッドがありません。");
            else if (methCandidate.Count > 1)
                throw new InvalidOperationException("型 " + this.GetType().Name + " において、クエリシステムから参照可能なメソッドが2つ以上あります。GetCurrentMethodをオーバーライドする必要があります。");
            else
                return methCandidate[0];
        }

        public abstract IEnumerable<object> GetArgumentsForQueryify();

        public string ToQuery()
        {
            var arg = String.Join(", ", from a in GetArgumentsForQueryify() select GetQueryString(a));
            var meth = Namespace + "." + Class + "." + GetCurrentMethod();
            if (!String.IsNullOrEmpty(arg))
                meth += "(" + arg + ")";
            if(Negate)
                meth = "!" + meth;
            return meth;
        }

        private string GetQueryString(object o)
        {
            if (o is string)
            {
                // escape double quote
                return "\"" + ((string)o).EscapeForQuery() + "\"";
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
