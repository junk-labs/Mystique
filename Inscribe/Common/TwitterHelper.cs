using System.Linq;
using System.Text.RegularExpressions;
using Dulcet.Twitter;
using Inscribe.Configuration.Tabs;
using Inscribe.Storage;
using Inscribe.ViewModels;

namespace Inscribe.Common
{
    public class TwitterHelper
    {
        public static bool IsMyTweet(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            return AccountStorage.Accounts
                .Any(d => status.Status.User.ScreenName == d.ScreenName);
        }

        public static bool IsMyCurrentTweet(TweetViewModel status, TabProperty property)
        {
            if (status == null || !status.IsStatusInfoContains || property == null) return false;
            return property.LinkAccountScreenNames.Any(a => a == status.Status.User.ScreenName);
        }

        public static bool IsRetweetedThis(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            var rtd = status.RetweetedUsers.Select(d => d.TwitterUser.ScreenName).ToArray();
            return AccountStorage.Accounts.Any(d => rtd.Contains(d.ScreenName));
        }

        public static bool IsRetweetedThisWithCurrent(TweetViewModel status, TabProperty property)
        {
            if (status == null || !status.IsStatusInfoContains || property == null) return false;
            var rtd = status.RetweetedUsers.Select(d => d.TwitterUser.ScreenName).ToArray();
            return property.LinkAccountScreenNames.Any(a => rtd.Contains(a));
        }

        public static bool IsFavoredThis(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            var fvd = status.FavoredUsers.Select(d => d.TwitterUser.ScreenName).ToArray();
            return AccountStorage.Accounts.Any(d => fvd.Contains(d.ScreenName));
        }

        public static bool IsFavoredThisWithCurrent(TweetViewModel status, TabProperty property)
        {
            if (status == null || !status.IsStatusInfoContains || property == null) return false;
            var fvd = status.FavoredUsers.Select(d => d.TwitterUser.ScreenName).ToArray();
            return property.LinkAccountScreenNames.Any(a => fvd.Contains(a));
        }

        public static bool IsInReplyToMe(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            return AccountStorage.Accounts.Any(d =>
                Regex.IsMatch(status.Status.Text, "@" + d.ScreenName + "(?![a-zA-Z0-9_])", RegexOptions.Singleline | RegexOptions.IgnoreCase));
        }

        public static bool IsInReplyToMeCurrent(TweetViewModel status, TabProperty property)
        {
            if (status == null || !status.IsStatusInfoContains || property == null) return false;
            return property.LinkAccountScreenNames.Any(a =>
                Regex.IsMatch(status.Status.Text, "@" + a + "(?![a-zA-Z0-9_])", RegexOptions.Singleline | RegexOptions.IgnoreCase));
        }

        public static bool IsInReplyToMeStrict(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            var s = status.Status as TwitterStatus;
            if (s != null)
            {
                return AccountStorage.Accounts.Any(d => d.ScreenName == s.InReplyToUserScreenName);
            }
            else
            {
                var dm = status.Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return AccountStorage.Accounts.Any(d => d.ScreenName == dm.Recipient.ScreenName);
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsInReplyToMeCurrentStrict(TweetViewModel status, TabProperty property)
        {
            if (status == null || !status.IsStatusInfoContains || property == null) return false;
            var s = status.Status as TwitterStatus;
            if (s != null)
            {
                return property.LinkAccountScreenNames.Any(a => a == s.InReplyToUserScreenName);
            }
            else
            {
                var dm = status.Status as TwitterDirectMessage;
                if (dm != null)
                {
                    return property.LinkAccountScreenNames.Any(a => a == dm.Recipient.ScreenName);
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsFollowingCurrent(UserViewModel user, TabProperty property)
        {
            if (user == null) return false;
            return property.LinkAccountInfos.All(i => i.IsFollowing(user.TwitterUser.ScreenName));
        }

        public static bool IsFollowingAny(UserViewModel user)
        {
            if (user == null) return false;
            return AccountStorage.Accounts.Any(d => d.Followings.Contains(user));
        }

        public static bool IsFollowingAll(UserViewModel user)
        {
            if (user == null) return false;
            return AccountStorage.Accounts.All(d => d.Followings.Contains(user));
        }

        public static bool IsFollowerCurrent(UserViewModel user, TabProperty property)
        {
            if (user == null || property == null) return false;
            return property.LinkAccountInfos.All(i => i.IsFollowedBy(user.TwitterUser.ScreenName));
        }

        public static bool IsFollowerAny(UserViewModel user)
        {
            return AccountStorage.Accounts.Any(d => d.Followers.Contains(user));
        }

        public static bool IsPublishedByRetweet(TweetViewModel status)
        {
            if (status == null || !status.IsStatusInfoContains) return false;
            var ss = status.Status as TwitterStatus;
            return ss != null && ss.RetweetedOriginal != null;
        }

        public static TwitterUser GetSuggestedUser(TweetViewModel status)
        {
            if (IsPublishedByRetweet(status))
                return ((TwitterStatus)status.Status).RetweetedOriginal.User;
            else
                return status.Status.User;
        }
    }
}
