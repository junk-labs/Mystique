using System.Linq;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterBlockFrom : ScreenNameUserFilterBase
    {
        private FilterBlockFrom() { }
        public FilterBlockFrom(string needle)
        {
            this.needle = needle;
        }

        public override string Identifier
        {
            get { return "block_from"; }
        }
        public override string Description
        {
            get { return "指定アカウントのブロック"; }
        }

        public override string FilterStateString
        {
            get { return "アカウント @" + this.needle + " でブロックしているユーザー"; }
        }

        public override bool FilterUser(Dulcet.Twitter.TwitterUser user)
        {
            return AccountStorage.Accounts.Where(i => Match(i.ScreenName, needle))
                .Any(i => i.IsBlocking(user.NumericId));
        }
    }
}
