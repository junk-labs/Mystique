using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inscribe.Common;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters
{
    public class FilterVerified : FilterBase
    {
        public FilterVerified() { }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            return TwitterHelper.GetSuggestedUser(status).IsVerified;
        }

        public override string Identifier
        {
            get { return "verified"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "公式認証ユーザーのツイート"; }
        }

        public override string FilterStateString
        {
            get { return "公式認証ユーザー"; }
        }
    }
}
