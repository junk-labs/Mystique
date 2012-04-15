using System;
using System.Collections.Generic;
using Acuerdo.Injection;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;
using System.Linq;
using Inscribe.Storage;

namespace Inscribe.Communication
{
    /// <summary>
    /// 通常のREST受信などのインジェクションを行えます。
    /// </summary>
    public static class InjectionPoint
    {
        public static IEnumerable<TwitterStatusBase> UnfoldTimeline(Func<int, IEnumerable<TwitterStatusBase>> reader, int lengthThreshold, int maxDepth)
        {
            List<IEnumerable<TwitterStatusBase>> cache = new List<IEnumerable<TwitterStatusBase>>();
            for (int i = 0; i < maxDepth; i++)
            {
                var status = ApiHelper.ExecApi(() => reader(i)).Guard().OrderByDescending(t => t.CreatedAt);
                cache.Add(status);
                if (status.Count() < lengthThreshold)
                    break;
                if (!status.Take(1).Any(s => TweetStorage.Contains(s.Id) == TweetExistState.Unreceived))
                    break;
            }
            return cache.Where(i => i != null).SelectMany(i => i);
        }

        #region Home

        internal static InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> _GetHomeTimelineInjection
            = new InjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>>(GetHomeTimeline);

        public static IInjectionPort<AccountInfo, IEnumerable<TwitterStatusBase>> GetHomeTimelineInjection
        {
            get { return _GetHomeTimelineInjection; }
        }

        private static IEnumerable<TwitterStatusBase> GetHomeTimeline(AccountInfo info)
        {
            return UnfoldTimeline(i => info.GetHomeTimeline(count: TwitterDefine.HomeReceiveMaxCount, page: i), TwitterDefine.HomeReceiveMaxCount * 2 / 3, 10);
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
            return UnfoldTimeline(i => info.GetMentions(count: TwitterDefine.MentionReceiveMaxCount, page: i), TwitterDefine.MentionReceiveMaxCount * 2 / 3, 3);
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
            if (info.NumericId != 0)
                return UnfoldTimeline(i => info.GetUserTimeline(userId: info.NumericId, count: TwitterDefine.HomeReceiveMaxCount, includeRts: true, page: i), TwitterDefine.HomeReceiveMaxCount * 2 / 3, 5);
            else
                return UnfoldTimeline(i => info.GetUserTimeline(screenName: info.ScreenName, count: TwitterDefine.HomeReceiveMaxCount, includeRts: true, page: i), TwitterDefine.HomeReceiveMaxCount * 2 / 3, 5);
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
            return UnfoldTimeline(i => info.GetFavorites(page: i), 5, 3);
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
