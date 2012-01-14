using Dulcet.Twitter;

namespace Inscribe.Filter.Filters
{
    /// <summary>
    /// ユーザー情報を用いたフィルタも可能であることを示します。
    /// </summary>
    public interface IUserFilter : IFilter
    {
        /// <summary>
        /// フィルタを適用します。<para />
        /// Negate値も考慮されます。
        /// </summary>
        bool FilterUser(TwitterUser user);
    }
}
