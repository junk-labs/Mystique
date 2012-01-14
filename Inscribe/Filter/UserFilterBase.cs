using Dulcet.Twitter;
using Inscribe.Common;
using Inscribe.Filter.Core;

namespace Inscribe.Filter
{
    /// <summary>
    /// ユーザーについてのフィルタの基底クラスです。
    /// </summary>
    public abstract class UserFilterBase : FilterBase
    {
        protected override bool FilterStatus(TwitterStatusBase status)
        {
            return FilterUser(status.User);
        }

        public abstract bool FilterUser(TwitterUser user);
    }
}
