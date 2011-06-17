using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Inscribe.Model;
using Inscribe.Data;
using Livet;
using Inscribe.Threading;
using Dulcet.Twitter;
using System.Threading.Tasks;
using Dulcet.Twitter.Rest;
using Inscribe.ViewModels;
using Inscribe.Storage;
using Inscribe.Configuration;
using System.Net;
using System.Xml.Linq;
using System.Collections.Concurrent;

namespace Inscribe.Communication.Posting
{
    /// <summary>
    /// 投稿処理の管理クラス
    /// </summary>
    /// <remarks>a.k.a 郵便局</remarks>
    public static class PostOffice
    {
        static Timer updateUnderControllingTimer = new Timer(UpdateUnderControls, null, 1000, 1000);

        static ConcurrentDictionary<AccountInfo, DateTime> underControls = new ConcurrentDictionary<AccountInfo, DateTime>();
        
        #region OnUnderControlChangedイベント

        public static event EventHandler<UnderControlEventArgs> OnUnderControlChanged;
        private static Notificator<UnderControlEventArgs> _OnUnderControlChangedEvent;
        public static Notificator<UnderControlEventArgs> OnUnderControlChangedEvent
        {
            get
            {
                if (_OnUnderControlChangedEvent == null) _OnUnderControlChangedEvent = new Notificator<UnderControlEventArgs>();
                return _OnUnderControlChangedEvent;
            }
            set { _OnUnderControlChangedEvent = value; }
        }

        private static void OnOnUnderControlChanged(UnderControlEventArgs e)
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref OnUnderControlChanged, null, null);
            if (threadSafeHandler != null) threadSafeHandler(null, e);
            OnUnderControlChangedEvent.Raise(e);
        }

        #endregion

        static PostOffice()
        {
            ThreadHelper.Halt += updateUnderControllingTimer.Dispose;
        }

        private static void AddUnderControlled(AccountInfo info)
        {
            if (underControls.ContainsKey(info))
            {
                NotifyStorage.Notify("[規制管理: @" + info.ScreenName + " は規制されています。解除予想時刻: " + underControls[info].ToString("HH:mm:ss"));
                return;
            }

            var timestamp = DateTime.Now.Subtract(Setting.Instance.TwitterProperty.UnderControlTimespan);

            // APIを利用してツイートを遡り受信
            var notify = NotifyStorage.NotifyManually("[規制管理: @" + info.ScreenName + " の直近のツイートを受信しています...]");

            try
            {
                var recvs = ApiHelper.ExecApi(() => info.GetUserTimeline(count: 200, include_rts: true));
                if (recvs != null)
                    recvs.ForEach(TweetStorage.Register);

                notify.Message = "[規制管理:規制開始時刻を割り出しています...]";

                // 127tweet/3hours
                var originate = TweetStorage.GetAll(
                    t => t.Status.User.ScreenName == info.ScreenName)
                    .OrderByDescending((t) => t.Status.CreatedAt)
                    .Skip(Setting.Instance.TwitterProperty.UnderControlCount - 1)
                    .FirstOrDefault();

                if (originate == null)
                {
                    NotifyStorage.Notify("[規制管理: @" + info.ScreenName + " はPOST規制されていますが、解除時刻を予想できません。しばらく置いて試してみてください。]");
                }
                else
                {
                    var release = (originate.Status.CreatedAt + Setting.Instance.TwitterProperty.UnderControlTimespan);
                    NotifyStorage.Notify("[規制管理: @" + info.ScreenName +
                        " はPOST規制されています。解除予想時刻は " + release.ToString("HH:mm:ss") + " です。]");
                    underControls.AddOrUpdate(info, release);
                    OnOnUnderControlChanged(new UnderControlEventArgs(info, true));
                }
            }
            finally
            {
                notify.Dispose();
            }
        }

        /// <summary>
        /// 指定したアカウントのPOST規制解放時間を取得します。
        /// </summary>
        public static DateTime GetAccountInfoControlReleaseTime(AccountInfo info)
        {
            DateTime release;
            if (underControls.TryGetValue(info, out release))
                return release;
            else
                return DateTime.MinValue;
        }

        /// <summary>
        /// 指定したアカウントがPOST規制対象であるかを確認します。
        /// </summary>
        public static bool IsAccountUnderControlled(AccountInfo info)
        {
            return underControls.ContainsKey(info);
        }

        /// <summary>
        /// 規制されたアカウントが再度投稿できるようになったかを確認
        /// </summary>
        private static void UpdateUnderControls(object o)
        {
            var dicts = underControls.ToArray();
            if (dicts.Length == 0) return;

            foreach (var i in dicts)
            {
                if (i.Value.Subtract(DateTime.Now).TotalSeconds < 0)
                {
                    // released
                    underControls.Remove(i.Key);
                    OnOnUnderControlChanged(new UnderControlEventArgs(i.Key, false));
                }
            }
        }

        public static void UpdateTweet(AccountInfo info, string text, long? inReplyToId = null)
        {
            if (Setting.Instance.InputExperienceProperty.TrimBeginSpace)
            {
                text = text.TrimStart(' ', '\t', '　');
            }
            try
            {
                var status = info.UpdateStatus(text, inReplyToId);
                if (status == null)
                    throw new InvalidOperationException("ツイートの成功を確認できませんでした。");
                TweetStorage.Register(status);
                NotifyStorage.Notify("ツイートしました:@" + info.ScreenName + ": " + text);

                var timestamp = DateTime.Now.Subtract(Setting.Instance.TwitterProperty.UnderControlTimespan);
                // 過去3時間以内のツイート数を取得
                var recent = TweetStorage.GetAll(
                    t => t.Status.User.ScreenName == info.ScreenName &&
                           timestamp < t.Status.CreatedAt).Count();
                if (recent>= Setting.Instance.TwitterProperty.UnderControlWarningThreshold)
                {
                    // 規制から明けたばかりかもしれない
                    var times = TweetStorage.GetAll(
                        t => t.Status.User.ScreenName == info.ScreenName)
                        .Select(t => t.Status.CreatedAt)
                        .Take(Setting.Instance.TwitterProperty.UnderControlCount * 2) // 直近2枠分を持ってこれば良い
                        .ToArray();

                    // 規制がスタートされたポイントは、[i+127-1]が3h以内のとき
                    int i = 0;
                    bool controlPointFound = false;
                    for (i = 0; i + Setting.Instance.TwitterProperty.UnderControlCount < times.Length && i < Setting.Instance.TwitterProperty.UnderControlCount; i++)
                    {
                        if (times[i] - times[i + Setting.Instance.TwitterProperty.UnderControlCount - 1] < Setting.Instance.TwitterProperty.UnderControlTimespan)
                        {
                            // 時間内に規制を行われる以上の投稿を行っている
                            controlPointFound = true;
                            break;
                        }
                    }

                    if (controlPointFound)
                    {
                        // 規制スタートポイントが判明した
                        if (i >= Setting.Instance.TwitterProperty.UnderControlWarningThreshold)
                            throw new TweetAnnotationException(TweetAnnotationException.AnnotationKind.NearUnderControl);
                    }
                    else
                    {
                        // そろそろ規制されるんじゃない？
                        throw new TweetAnnotationException(TweetAnnotationException.AnnotationKind.NearUnderControl);
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    var hrw = wex.Response as HttpWebResponse;
                    if (hrw != null)
                    {

                        if (hrw.StatusCode == HttpStatusCode.Forbidden)
                        {
                            // 規制？
                            using (var strm = hrw.GetResponseStream())
                            {
                                var xdoc = XDocument.Load(strm);
                                var eel = xdoc.Root.Element("error");
                                if (eel != null)
                                {
                                    if (eel.Value.IndexOf("update limit", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    {
                                        // User is over daily status update limit.
                                        // POST規制
                                        AddUnderControlled(info);
                                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.Controlled);
                                    }
                                    else if (eel.Value.IndexOf("duplicate", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    {
                                        // 同じツイートをしようとした
                                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.Duplicated);
                                    }
                                    else
                                    {
                                        // 何かよくわからない
                                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, eel.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (wex.Status == WebExceptionStatus.Timeout)
                    {
                        // タイムアウト
                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.Timeout, "ツイートに失敗しました(タイムアウトしました。Twitterが不調かも)", wex);
                    }
                    else
                    {
                        // 何かがおかしい
                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, "ツイートに失敗しました(" + (int)wex.Status + ")", wex);
                    }
                }
            }
            catch (TweetAnnotationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, "ツイートに失敗しました(" + ex.Message + ")", ex);
            }
        }
        public static void FavTweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => FavTweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void FavTweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    if (ApiHelper.ExecApi(() => d.CreateFavorites(status.Status.Id)) != null)
                    {
                        var ud = UserStorage.Get(d.ScreenName);
                        if (ud != null)
                            status.RegisterFavored(ud);
                    }
                    else
                    {
                        success = false;
                        NotifyStorage.Notify("Favに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Favしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        public static void Retweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => RetweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void RetweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    if (ApiHelper.ExecApi(() => d.Retweet(status.Status.Id)) != null)
                    {
                        var ud = UserStorage.Get(d.ScreenName);
                        if (ud != null)
                            status.RegisterRetweeted(ud);
                    }
                    else
                    {
                        success = false;
                        NotifyStorage.Notify("Retweetに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Retweetしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        public static void RemoveTweet(AccountInfo info, long tweetId)
        {
            Task.Factory.StartNew(() => RemoveTweetSink(info, tweetId), TaskCreationOptions.LongRunning);
        }

        private static void RemoveTweetSink(AccountInfo info, long tweetId)
        {
            var tweet = ApiHelper.ExecApi(() => info.DestroyStatus(tweetId));
            if (tweet != null)
            {
                if (tweet.Id != tweetId)
                {
                    NotifyStorage.Notify("削除には成功しましたが、ツイートIDが一致しません。(" + tweetId.ToString() + " -> " + tweet.Id.ToString() + ")");
                }
                else
                {
                    TweetStorage.Remove(tweetId);
                    NotifyStorage.Notify("削除しました:" + tweet.ToString());
                }
            }
            else
            {
                NotifyStorage.Notify("ツイートを削除できませんでした(@" + info.ScreenName + ")");
            }
        }

    }

    public class UnderControlEventArgs : EventArgs
    {
        public AccountInfo AccountInfo { get; set; }

        /// <summary>
        /// UnderControlに追加されたか(falseなら削除された)
        /// </summary>
        public bool Added { get; set; }

        public UnderControlEventArgs(AccountInfo info, bool added)
        {
            this.AccountInfo = info;
            this.Added = added;
        }
    }
}
