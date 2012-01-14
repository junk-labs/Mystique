using System.Collections.Generic;

namespace Inscribe.Filter.Filters.Attributes
{
    public class FilterProtected : UserFilterBase
    {
        public FilterProtected() { }

        public override string Identifier
        {
            get { return "protected"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield break;
        }

        public override string Description
        {
            get { return "プロテクトユーザーのツイート"; }
        }

        public override string FilterStateString
        {
            get { return "プロテクトユーザー"; }
        }

        public override bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return user.IsProtected;
        }
    }
}
