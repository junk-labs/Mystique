using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dulcet.Twitter;

namespace Inscribe.Filter.Filters.Arg0
{
    public class FilterDirectMessage : FilterBase
    {
        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return status is TwitterDirectMessage;
        }

        public override string Identifier
        {
            get { return "dmsg"; }
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
