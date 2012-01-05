using System.Collections.Generic;
using Dulcet.Twitter;

namespace Inscribe.Filter.Filters.Attributes
{
    public class FilterDirectMessage : FilterBase
    {
        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return status is TwitterDirectMessage;
        }

        public override string Identifier
        {
            get { return "dm"; }
        }

        public override IEnumerable<string> Aliases
        {
            get { yield return "dmsg"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "ダイレクトメッセージ"; }
        }

        public override string FilterStateString
        {
            get { return "ダイレクトメッセージ"; }
        }
    }
}
