using System.Linq;
using Dulcet.Twitter;
using Inscribe.Configuration;
using Inscribe.Storage;

namespace Inscribe.Common
{
    public static class FilterHelper
    {
        public static bool IsMuted(TwitterStatusBase status)
        {
            return (Setting.Instance.TimelineFilteringProperty.MuteFilterCluster != null &&
                Setting.Instance.TimelineFilteringProperty.MuteFilterCluster.Filter(status)) ||
                IsMuted(status.User);
        }

        public static bool IsMuted(TwitterUser user)
        {
            return Setting.Instance.TimelineFilteringProperty.MuteBlockedUsers &&
                AccountStorage.Accounts.Any(a => a.IsBlocking(user.NumericId));
        }
    }
}
