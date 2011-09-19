using System;
using System.Collections.Generic;
using Acuerdo.Injection;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;

namespace Inscribe.Communication
{
    /// <summary>
    /// 通常のREST受信などのインジェクションを行えます。
    /// </summary>
    public static class InjectionPoint
    {
        #region Home

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetHomeTimelineInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetHomeTimeline);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetHomeTimelineInjection
        {
            get { return _GetHomeTimelineInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetHomeTimeline(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetHomeTimeline(count: TwitterDefine.HomeReceiveMaxCount));
        }

        #endregion

        #region Mentions

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetMentionsInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetMentions);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetMentionsInjection
        {
            get { return _GetMentionsInjection;}
        }

        private static IEnumerable<TwitterStatusBase> GetMentions(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetMentions(count: TwitterDefine.MentionReceiveMaxCount));
        }

        #endregion

        #region DirectMessages

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetDirectMessagesInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>( GetDirectMessages);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetDirectMessagesInjection
        {
            get { return _GetDirectMessagesInjection;}
        }

        private static IEnumerable<TwitterStatusBase> GetDirectMessages(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetDirectMessages(count: TwitterDefine.DmReceiveMaxCount));
        }

        #endregion

        #region SentDirectMessages

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetSentDirectMessagesInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetSentDirectMessages);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetSentDirectMessagesInjection
        {
            get { return _GetSentDirectMessagesInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetSentDirectMessages(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetSentDirectMessages(count: TwitterDefine.DmReceiveMaxCount));
        }

        #endregion

        #region MyTweets

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetMyTweetsInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetMyTweets);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetMyTweetsInjection
        {
            get { return _GetMyTweetsInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetMyTweets(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetUserTimeline(screenName: info.ScreenName, count: TwitterDefine.HomeReceiveMaxCount, includeRts: true));
        }

        #endregion

        #region Favorites

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetFavoritesInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetFavorites);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetFavoritesInjection
        {
            get { return _GetFavoritesInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetFavorites(AccountInfo info)
        {
            return ApiHelper.ExecApi(() => info.GetFavorites());
        }

        #endregion

        #region List

        internal static InjectionPort<Tuple<AccountInfo, string, string>, IEnumerable<TwitterStatusBase>> _GetListTimelineInjection
            = new InjectionPort<Tuple<AccountInfo, string, string>, IEnumerable<TwitterStatusBase>>(GetListTimeline);

        public static IInjectionPort<Tuple<AccountInfo, string, string>, IEnumerable<TwitterStatusBase>> GetListTimelineInjection
        {
            get { return _GetListTimelineInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetListTimeline(Tuple<AccountInfo, string, string> data)
        {
            return ApiHelper.ExecApi(() =>
                data.Item1.GetListStatuses(
                data.Item2,
                data.Item3,
                perPage: TwitterDefine.ListReceiveCount, includeRts: true, includeEntities: true));
        }

        #endregion
    }
}
