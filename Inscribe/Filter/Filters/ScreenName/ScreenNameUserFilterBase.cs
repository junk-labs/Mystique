using System;
using System.Text.RegularExpressions;
using Inscribe.Filter.Core;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public abstract class ScreenNameUserFilterBase : UserFilterBase
    {
        [GuiVisible("検索@ID")]
        public string Needle
        {
            get { return this.needle; }
            set
            {
                if (this.needle == value) return;
                this.needle = value;
                RaiseRequireReaccept();
            }
        }
        protected string needle = String.Empty;

        public override string FilterStateString
        {
            get { return this.Description + "から@" + this.needle + " を検索"; }
        }

        protected virtual bool Match(string haystack, string needle)
        {
            return MatchingUtil.MatchAccount(haystack, needle);
        }

        public override System.Collections.Generic.IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return needle;
        }
    }
}
