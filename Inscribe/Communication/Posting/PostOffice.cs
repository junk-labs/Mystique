using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Configuration;
using Inscribe.Model;
using Inscribe.Storage;
using Inscribe.Threading;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;
using System.Runtime.Serialization.Json;
using System.Text;

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

            var timestamp = DateTime.Now.Subtract(TwitterDefine.UnderControlTimespan);

            // APIを利用してツイートを遡り受信
            var notify = NotifyStorage.NotifyManually("[規制管理: @" + info.ScreenName + " の直近のツイートを受信しています...]");

            try
            {
                var recvs = ApiHelper.ExecApi(() => info.GetUserTimeline(count: 150, includeRts: true));
                if (recvs != null)
                    recvs.ForEach(i => TweetStorage.Register(i));

                notify.Message = "[規制管理:規制開始時刻を割り出しています...]";

                // 127tweet/3hours
                var originate = TweetStorage.GetAll(
                    t => t.Status.User.ScreenName == info.ScreenName && DateTime.Now.Subtract(t.CreatedAt) < TwitterDefine.UnderControlTimespan)
                    .OrderByDescending((t) => t.Status.CreatedAt)
                    .Skip(TwitterDefine.UnderControlCount - 1)
                    .FirstOrDefault();

                if (originate == null)
                {
                    originate = TweetStorage.GetAll(
                        t => t.Status.User.ScreenName == info.ScreenName && DateTime.Now.Subtract(t.CreatedAt) < TwitterDefine.UnderControlTimespan)
                        .OrderByDescending((t) => t.Status.CreatedAt)
                        .LastOrDefault();
                }

                if (originate == null)
                {
                    NotifyStorage.Notify("[規制管理: @" + info.ScreenName + " はPOST規制されていますが、解除時刻を予想できません。しばらく置いて試してみてください。]");
                }
                else
                {
                    var release = (originate.Status.CreatedAt + TwitterDefine.UnderControlTimespan);
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
        /// POST規制アカウントが存在することを確認します。
        /// </summary>
        /// <returns></returns>
        public static bool IsExistsUnderControlledAccount()
        {
            return underControls.Count > 0;
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

        public static Tuple<DateTime, int> GetUnderControlChunk(AccountInfo info)
        {
            // とりあえずこのユーザーの全ツイートを持ってくる
            var times = TweetStorage.GetAll(
                t => t.Status.User.ScreenName == info.ScreenName)
                .Select(t => t.Status.CreatedAt)
                .OrderByDescending(t => t) // 新着順に並べる
                .ToArray();

            bool initPointFound = false;
            // 規制がスタートされたポイントは、[i+127-1]が3h以内のとき
            int i = 0;
            // 二倍遡って、規制開始ポイントを正確に把握する
            for (i = 0; i + TwitterDefine.UnderControlCount < times.Length && i < TwitterDefine.UnderControlCount; i++)
            {
                if (times[i] - times[i + TwitterDefine.UnderControlCount - 1] < TwitterDefine.UnderControlTimespan)
                {
                    // i - 1 が規制ポイント
                    // i < 0だと厄介なのでiで代用
                    initPointFound = true;
                    break;
                }
                else if (times[i] - times[i + 1] > TwitterDefine.UnderControlTimespan)
                {
                    // 三時間以上の間隔があいている
                    initPointFound = true;
                    break;
                }
            }
            if (initPointFound && i >= 0)
            {
                // 規制開始ポイントが割り出せた
                return new Tuple<DateTime, int>(times[i].Add(TwitterDefine.UnderControlTimespan), i);
            }
            else
            {
                // 規制開始ポイントが分からないので、とりあえずウィンドウタイムだけ遡る
                var window = times.Where(d => d > DateTime.Now.Subtract(TwitterDefine.UnderControlTimespan))
                    .OrderBy(d => d);
                var initt = window.FirstOrDefault();
                if (initt != null)
                {
                    return new Tuple<DateTime, int>(initt.Add(TwitterDefine.UnderControlTimespan), window.Count());
                }
                else
                {
                    return new Tuple<DateTime, int>(DateTime.MinValue, 0);
                }
            }
        }

        public static int UpdateTweet(AccountInfo info, string text, long? inReplyToId = null)
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

                var chunk = GetUnderControlChunk(info);
                if(chunk.Item2 > TwitterDefine.UnderControlWarningThreshold)
                {
                    throw new TweetAnnotationException(TweetAnnotationException.AnnotationKind.NearUnderControl);
                }
                return chunk.Item2;
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
                                using (var json = JsonReaderWriterFactory.CreateJsonReader(strm,
                                    System.Xml.XmlDictionaryReaderQuotas.Max))
                                {
                                    var xdoc = XDocument.Load(json);
                                    System.Diagnostics.Debug.WriteLine(xdoc);
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
            return -1;
        }

        public static void FavTweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => FavTweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void FavTweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            if (((TwitterStatus)status.Status).RetweetedOriginal != null)
                status = TweetStorage.Get(((TwitterStatus)status.Status).RetweetedOriginal.Id, true);
            if (status == null)
            {
                NotifyStorage.Notify("Fav 対象ステータスが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    var ud = UserStorage.Get(d.ScreenName);
                    // ふぁぼり状態更新
                    if (ud != null)
                        status.RegisterFavored(ud);
                    if (ApiHelper.ExecApi(() => d.CreateFavorites(status.Status.Id)) == null)
                    {
                        success = false;
                        if (ud != null)
                            status.RemoveFavored(ud);
                        NotifyStorage.Notify("Favに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Favしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        public static void UnfavTweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => UnfavTweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void UnfavTweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            if (((TwitterStatus)status.Status).RetweetedOriginal != null)
                status = TweetStorage.Get(((TwitterStatus)status.Status).RetweetedOriginal.Id, true);
            if (status == null)
            {
                NotifyStorage.Notify("Unfav 対象ステータスが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    var ud = UserStorage.Get(d.ScreenName);
                    // ふぁぼり状態更新
                    if (ud != null)
                        status.RemoveFavored(ud);
                    if (ApiHelper.ExecApi(() => d.DestroyFavorites(status.Status.Id)) == null)
                    {
                        success = false;
                        if(ud != null)
                            status.RegisterFavored(ud);
                        NotifyStorage.Notify("Unfavに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Unfavしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        public static void Retweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => RetweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void RetweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            if (((TwitterStatus)status.Status).RetweetedOriginal != null)
                status = TweetStorage.Get(((TwitterStatus)status.Status).RetweetedOriginal.Id);
            if (status == null)
            {
                NotifyStorage.Notify("Retweet オリジナルデータが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    // リツイート状態更新

                    var ud = UserStorage.Get(d.ScreenName);
                    if (ud != null)
                        status.RegisterRetweeted(ud);
                    if (ApiHelper.ExecApi(() => d.Retweet(status.Status.Id)) == null)
                    {
                        if (ud != null)
                            status.RemoveRetweeted(ud);
                        success = false;
                        NotifyStorage.Notify("Retweetに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Retweetしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        public static void Unretweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            Task.Factory.StartNew(() => UnretweetSink(infos, status), TaskCreationOptions.LongRunning);
        }

        private static void UnretweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            if (((TwitterStatus)status.Status).RetweetedOriginal != null)
                status = TweetStorage.Get(((TwitterStatus)status.Status).RetweetedOriginal.Id);
            if (status == null)
            {
                NotifyStorage.Notify("Retweet オリジナルデータが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                d =>
                {
                    // リツイート状態更新
                    var ud = UserStorage.Get(d.ScreenName);
                    if (ud != null)
                        status.RegisterRetweeted(ud);

                    // リツイートステータスの特定
                    var rts = TweetStorage.GetAll(vm =>
                        vm.Status.User.ScreenName == d.ScreenName && vm.Status is TwitterStatus &&
                        ((TwitterStatus)vm.Status).RetweetedOriginal != null && ((TwitterStatus)vm.Status).RetweetedOriginal.Id == status.Status.Id).FirstOrDefault();
                    if (rts == null || ApiHelper.ExecApi(() => d.DestroyStatus(rts.Status.Id) == null))
                    {
                        if (ud != null)
                            status.RemoveRetweeted(ud);
                        success = false;
                        NotifyStorage.Notify("Retweetキャンセルに失敗しました: @" + d.ScreenName);
                    }
                });
            if (success)
                NotifyStorage.Notify("Retweetをキャンセルしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
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
