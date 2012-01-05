using System.Collections.Generic;
using System.Linq;
using Inscribe.Storage;

namespace Inscribe.Filter.Filters.ScreenName
{
    public class FilterFavoriteFrom : ScreenNameFilterBase
    {
        private FilterFavoriteFrom() { }
        public FilterFavoriteFrom(string needle)
        {
            this.needle = needle;
        }

        protected override bool FilterStatus(Dulcet.Twitter.TwitterStatusBase status)
        {
            var vm = TweetStorage.Get(status.Id);
            if (vm == null) return false;
            return vm.FavoredUsers.Any(u => Match(u.TwitterUser.ScreenName, needle));
        }

        public override string Identifier
        {
            get { return "fav_by"; }
        }

        public override IEnumerable<string> Aliases
        {
            get { yield return "fav_from"; }
        }

        public override IEnumerable<object> GetArgumentsForQueryify()
        {
            yield return needle;
        }

        public override string Description
        {
            get { return "指定ユーザーのFavorite"; }
        }

        public override string FilterStateString
        {
            get { return "ユーザー @" + this.needle + " のFavorite"; }
        }
    }
}
