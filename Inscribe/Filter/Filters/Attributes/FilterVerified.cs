using System.Collections.Generic;
using Dulcet.Twitter;

namespace Inscribe.Filter.Filters.Attributes
{
    public class FilterVerified : UserFilterBase
    {
        public FilterVerified() { }

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

        public override bool FilterUser(TwitterUser user)
        {
            return user.IsVerified;
        }
    }
}
