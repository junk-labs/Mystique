using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Acuerdo.Injection;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Authentication;
using Inscribe.Common;
using Inscribe.Configuration;
using Inscribe.Storage;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;
using Livet;

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

        static QueueTaskDispatcher operationDispatcher;

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
            operationDispatcher = new QueueTaskDispatcher(20);
            ThreadHelper.Halt += updateUnderControllingTimer.Dispose;
            ThreadHelper.Halt += () => operationDispatcher.Dispose();
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
                ApiHelper.ExecApi(() => info.GetUserTimeline(count: 150, includeRts: true))
                    .Guard()
                    .ForEach(i => TweetStorage.Register(i));

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
            // based on http://ltzz.info/alpha/twitter_kisei.html
            // セクション(Under control chunk)を導出する

            // とりあえずこのユーザーの全ツイートを持ってくる
            // 投稿時間配列にする
            var times = TweetStorage.GetAll(t => t.Status.User.ScreenName == info.ScreenName)
                .Select(t => t.Status.CreatedAt)
                .OrderByDescending(t => t) // 新着順に並べる
                .ToArray();

            // 全ツイートのうち、3時間以上の投稿の空きがある部分を調べる
            int initPoint = -1;
            for (int i = 0; i < times.Length; i++)
            {
                // 前のツイートまで、3時間以上の空きがあるとそこがチャンクの切れ目
                if (i + 1 < times.Length &&
                    times[i] - times[i + 1] > TwitterDefine.UnderControlTimespan)
                {
                    // ここがチャンクの切れ目
                    initPoint = i;
                    break;
                }
                else if (i + TwitterDefine.UnderControlCount < times.Length &&
                    times[i + 1] - times[i + TwitterDefine.UnderControlCount] <
                    TwitterDefine.UnderControlTimespan)
                {
                    // UnderControlTimespanの期間中、UnderControlCountを超える投稿がある
                    // →チャンクの切れ目
                    initPoint = i;
                    break;
                }
            }

            while (initPoint >= 0 &&
                DateTime.Now.Subtract(times[initPoint]) > TwitterDefine.UnderControlTimespan)
            {
                // 導出したチャンクから現在消費中のチャンクを推測する
                // チャンクのスタートポイントがチャンク時間内でない場合、チャンク時間後のツイートを順番に辿る
                var chunkEnd = times[initPoint] + TwitterDefine.UnderControlTimespan;
                bool found = false;
                for (int i = initPoint; i >= 0; i--)
                {
                    if (times[initPoint] >= chunkEnd)
                    {
                        initPoint = i;
                        found = true;
                        break;
                    }
                }
                // チャンクの導出ができないとは何事だ
                if (!found)
                    initPoint = -1;
            }

            if (initPoint >= 0)
            {
                // 結局チャンクの導出がしっかりできた
                return new Tuple<DateTime, int>(times[initPoint].Add(TwitterDefine.UnderControlTimespan), initPoint);
            }
            else
            {
                // なんかだめだった
                // 規制開始ポイントが分からないので、とりあえずウィンドウタイムだけ遡る
                var window = times.Where(d => d > DateTime.Now.Subtract(TwitterDefine.UnderControlTimespan))
                    .OrderBy(d => d);
                var initt = window.FirstOrDefault();
                if (initt != null)
                {
                    return new Tuple<DateTime, int>(initt.Add(TwitterDefine.UnderControlTimespan),
                        window.Count());
                }
                else
                {
                    return new Tuple<DateTime, int>(DateTime.MinValue, 0);
                }
            }
        }

        public static int UpdateTweet(AccountInfo info, string text, long? inReplyToId = null)
        {
            int retryCount = 0;
            do
            {
                try
                {
                    updateInjection.Execute(new Tuple<AccountInfo, string, long?>(info, text, inReplyToId));
                    break; // break roop on succeeded
                }
                catch (WebException wex)
                {
                    if (wex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var hrw = wex.Response as HttpWebResponse;
                        if (hrw != null && hrw.StatusCode == HttpStatusCode.Forbidden)
                        {
                            // 規制？
                            using (var strm = hrw.GetResponseStream())
                            using (var json = JsonReaderWriterFactory.CreateJsonReader(strm,
                                System.Xml.XmlDictionaryReaderQuotas.Max))
                            {
                                var xdoc = XDocument.Load(json);
                                System.Diagnostics.Debug.WriteLine(xdoc);
                                var eel = xdoc.Root.Element("errors");
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
                                        if (retryCount == 0) // 複数回投稿している場合はサスペンドする
                                            throw new TweetFailedException(TweetFailedException.TweetErrorKind.Duplicated);
                                        else
                                            break; // 成功
                                    }
                                    else
                                    {
                                        // 何かよくわからない
                                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, eel.Value);
                                    }
                                }
                            }
                        }
                        // 何かよくわからない
                        throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, wex.Message);
                    }
                    else
                    {
                        if (!Setting.Instance.InputExperienceProperty.AutoRetryOnError)
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
                }
                catch (TweetAnnotationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new TweetFailedException(TweetFailedException.TweetErrorKind.CommonFailed, "ツイートに失敗しました(" + ex.Message + ")", ex);
                }
            } while (Setting.Instance.InputExperienceProperty.AutoRetryOnError && retryCount++ < Setting.Instance.InputExperienceProperty.AutoRetryMaxCount);

            var chunk = GetUnderControlChunk(info);
            if (chunk.Item2 > TwitterDefine.UnderControlWarningThreshold)
                throw new TweetAnnotationException(TweetAnnotationException.AnnotationKind.NearUnderControl);

            return chunk.Item2;
        }

        private static InjectionPort<Tuple<AccountInfo, string, long?>> updateInjection =
            new InjectionPort<Tuple<AccountInfo, string, long?>>(a => UpdateTweetSink(a.Item1, a.Item2, a.Item3));

        public static IInjectionPort<Tuple<AccountInfo, string, long?>> UpdateInjection
        {
            get { return updateInjection.GetInterface(); }
        }

        private static void UpdateTweetSink(AccountInfo info, string text, long? inReplyToId = null)
        {
            var status = info.UpdateStatus(text, inReplyToId);
            if (status == null || status.Id == 0)
                throw new WebException("Timeout or failure sending tweet.", WebExceptionStatus.Timeout);

            TweetStorage.Register(status);
            NotifyStorage.Notify("ツイートしました:@" + info.ScreenName + ": " + text);
        }

        internal static void UpdateDirectMessage(AccountInfo accountInfo, string target, string body)
        {
            dmInjection.Execute(new Tuple<AccountInfo, string, string>(accountInfo, target, body));
        }

        private static InjectionPort<Tuple<AccountInfo, string, string>> dmInjection =
            new InjectionPort<Tuple<AccountInfo, string, string>>(a => UpdateDirectMessageSink(a.Item1, a.Item2, a.Item3));

        public static IInjectionPort<Tuple<AccountInfo, string, string>> DirectMessageInjection
        {
            get { return dmInjection.GetInterface(); }
        }

        private static void UpdateDirectMessageSink(AccountInfo info, string target, string body)
        {
            var status = info.SendDirectMessage(target, body);
            if (status == null || status.Id == 0)
                throw new WebException("Timeout or failure sending tweet.", WebExceptionStatus.Timeout);

            TweetStorage.Register(status);
            NotifyStorage.Notify("DMを送信しました: @" + info.ScreenName + " -> @" + target + ": " + body);
        }

        #region Favorite

        public static void FavTweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            operationDispatcher.Enqueue(() => FavTweetSink(infos, status));
        }

        private static InjectionPort<Tuple<AccountInfo, TweetViewModel>> favoriteInjection =
            new InjectionPort<Tuple<AccountInfo, TweetViewModel>>(a => FavTweetCore(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, TweetViewModel>> FavoriteInjection
        {
            get { return favoriteInjection.GetInterface(); }
        }

        public static void FavTweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            var ts = status.Status as TwitterStatus;
            if (ts == null)
            {
                NotifyStorage.Notify("DirectMessageはFavできません。");
                return;
            }
            if (ts.RetweetedOriginal != null)
                status = TweetStorage.Get(ts.RetweetedOriginal.Id, true);
            if (status == null)
            {
                NotifyStorage.Notify("Fav 対象ステータスが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    var ud = d.UserViewModel;
                    // ふぁぼり状態更新
                    if (ud != null)
                        status.RegisterFavored(ud);
                    try
                    {
                        favoriteInjection.Execute(new Tuple<AccountInfo, TweetViewModel>(d, status));
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        if (ud != null)
                            status.RemoveFavored(ud);
                        if (ex is FavoriteSuspendedException && Setting.Instance.InputExperienceProperty.EnableFavoriteFallback)
                        {
                            // ふぁぼ規制 -> フォールバック
                            AccountInfo fallback = null;
                            if (!String.IsNullOrEmpty(d.AccountProperty.FallbackAccount) &&
                                (fallback = AccountStorage.Get(d.AccountProperty.FallbackAccount)) != null &&
                                !status.FavoredUsers.Contains(fallback.UserViewModel))
                            {
                                NotifyStorage.Notify("Fav fallbackします: @" + d.ScreenName + " >> @");
                                FavTweetSink(new[] { fallback }, status);
                            }
                        }
                        else
                        {
                            NotifyStorage.Notify("Favに失敗しました: @" + d.ScreenName);
                            if (!(ex is ApplicationException))
                            {
                                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                                    "Fav操作時にエラーが発生しました");
                            }
                        }
                    }
                });
            if (success)
                NotifyStorage.Notify("Favしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        private static void FavTweetCore(AccountInfo d, TweetViewModel status)
        {
            /*
            if (ApiHelper.ExecApi(() => d.CreateFavorites(status.Status.Id)) == null)
                throw new ApplicationException();
            */
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (d.CreateFavorites(status.Status.Id) != null)
                        break;
                }
                catch (WebException wex)
                {
                    var hwr = wex.Response as HttpWebResponse;
                    if (wex.Status == WebExceptionStatus.ProtocolError &&
                        hwr != null && hwr.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // あとIt's great that ... ならふぁぼ規制
                        using (var read = new StreamReader(hwr.GetResponseStream()))
                        {
                            var desc = read.ReadToEnd();
                            if (desc.Contains("It's great that you like so many updates, but we only allow so many updates to be marked as a favorite per day."))
                            {
                                throw new FavoriteSuspendedException();
                            }
                        }
                    }
                    else if (wex.Status == WebExceptionStatus.Timeout)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region Unfavorite

        public static void UnfavTweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            operationDispatcher.Enqueue(() => UnfavTweetSink(infos, status));
        }

        private static InjectionPort<Tuple<AccountInfo, TweetViewModel>> unfavoriteInjection =
            new InjectionPort<Tuple<AccountInfo, TweetViewModel>>(a => UnfavTweetCore(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, TweetViewModel>> UnfavoriteInjection
        {
            get { return unfavoriteInjection.GetInterface(); }
        }

        private static void UnfavTweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            var ts = status.Status as TwitterStatus;
            if (ts == null)
            {
                NotifyStorage.Notify("DirectMessageはFavできません。");
                return;
            }
            if (ts.RetweetedOriginal != null)
                status = TweetStorage.Get(ts.RetweetedOriginal.Id, true);
            if (status == null)
            {
                NotifyStorage.Notify("Unfav 対象ステータスが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos,
                (d) =>
                {
                    var ud = d.UserViewModel;
                    // ふぁぼり状態更新
                    if (ud != null)
                        status.RemoveFavored(ud);
                    try
                    {
                        unfavoriteInjection.Execute(new Tuple<AccountInfo, TweetViewModel>(d, status));
                    }
                    catch (Exception ex)
                    {
                        success = false;
                        if (ud != null)
                            status.RegisterFavored(ud);
                        NotifyStorage.Notify("Unfavに失敗しました: @" + d.ScreenName);
                        if (!(ex is ApplicationException))
                        {
                            ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                                "Unfav操作時にエラーが発生しました");
                        }
                    }
                });
            if (success)
                NotifyStorage.Notify("Unfavしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        private static void UnfavTweetCore(AccountInfo d, TweetViewModel status)
        {
            if (ApiHelper.ExecApi(() => d.DestroyFavorites(status.Status.Id)) == null)
                throw new ApplicationException();
        }

        #endregion

        #region Retweet

        public static void Retweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            operationDispatcher.Enqueue(() => RetweetSink(infos, status));
        }

        private static InjectionPort<Tuple<AccountInfo, TweetViewModel>> retweetInjection =
            new InjectionPort<Tuple<AccountInfo, TweetViewModel>>(a => RetweetCore(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, TweetViewModel>> RetweetInjection
        {
            get { return retweetInjection.GetInterface(); }
        }

        public static void RetweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            var ts = status.Status as TwitterStatus;
            if (ts == null)
            {
                NotifyStorage.Notify("DirectMessageはRetweetできません。");
                return;
            }
            if (ts.RetweetedOriginal != null)
                status = TweetStorage.Get(ts.RetweetedOriginal.Id, true);
            if (status == null)
            {
                NotifyStorage.Notify("Retweet オリジナルデータが見つかりません。");
                return;
            }
            bool success = true;
            Parallel.ForEach(infos.Select(i => CheckFallback(i)).Where(i => i != null).Distinct(),
                d =>
                {
                    // リツイート状態更新
                    var ud = d.UserViewModel;
                    if (ud != null)
                        status.RegisterRetweeted(ud);
                    try
                    {
                        retweetInjection.Execute(new Tuple<AccountInfo, TweetViewModel>(d, status));
                    }
                    catch (Exception ex)
                    {
                        if (ud != null)
                            status.RemoveRetweeted(ud);
                        success = false;
                        NotifyStorage.Notify("Retweetに失敗しました: @" + d.ScreenName);
                        if (!(ex is ApplicationException))
                        {
                            ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                                "Retweet操作時にエラーが発生しました");
                        }
                    }
                });
            if (success)
                NotifyStorage.Notify("Retweetしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        private static AccountInfo CheckFallback(AccountInfo d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            AccountInfo origin = d;
            while (
                Setting.Instance.InputExperienceProperty.OfficialRetweetFallback &&
                IsAccountUnderControlled(d) &&
                !String.IsNullOrEmpty(d.AccountProperty.FallbackAccount))
            {
                var fallback = AccountStorage.Get(d.AccountProperty.FallbackAccount);
                if (fallback == null || fallback == origin)
                    break;
                d = fallback;
            }
            return d;
        }

        private static void RetweetCore(AccountInfo d, TweetViewModel status)
        {
            if (ApiHelper.ExecApi(() => d.Retweet(status.Status.Id)) == null)
                throw new ApplicationException();
        }

        #endregion

        #region Unretweet

        public static void Unretweet(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            operationDispatcher.Enqueue(() => UnretweetSink(infos, status));
        }

        private static InjectionPort<Tuple<AccountInfo, TweetViewModel>> unretweetInjection =
            new InjectionPort<Tuple<AccountInfo, TweetViewModel>>(a => RemoveRetweetCore(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, TweetViewModel>> UnretweetInjection
        {
            get { return unretweetInjection.GetInterface(); }
        }

        private static void UnretweetSink(IEnumerable<AccountInfo> infos, TweetViewModel status)
        {
            var ts = status.Status as TwitterStatus;
            if (ts == null)
            {
                NotifyStorage.Notify("DirectMessageはUnretweetできません。");
                return;
            }
            if (ts.RetweetedOriginal != null)
                status = TweetStorage.Get(ts.RetweetedOriginal.Id, true);
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
                    var ud = d.UserViewModel;
                    if (ud != null)
                        status.RegisterRetweeted(ud);
                    try
                    {
                        unretweetInjection.Execute(new Tuple<AccountInfo, TweetViewModel>(d, status));
                    }
                    catch (Exception ex)
                    {
                        if (ud != null)
                            status.RemoveRetweeted(ud);
                        success = false;
                        NotifyStorage.Notify("Retweetキャンセルに失敗しました: @" + d.ScreenName);
                        if (!(ex is ApplicationException))
                        {
                            ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                                "Retweetキャンセル操作時にエラーが発生しました");
                        }
                    }
                });
            if (success)
                NotifyStorage.Notify("Retweetをキャンセルしました: @" + status.Status.User.ScreenName + ": " + status.Status.Text);
        }

        private static void RemoveRetweetCore(AccountInfo d, TweetViewModel status)
        {
            // リツイートステータスの特定
            var rts = TweetStorage.GetAll(vm =>
                vm.Status.User.ScreenName == d.ScreenName && vm.Status is TwitterStatus &&
                ((TwitterStatus)vm.Status).RetweetedOriginal != null &&
                ((TwitterStatus)vm.Status).RetweetedOriginal.Id == status.Status.Id).FirstOrDefault();
            if (rts == null || ApiHelper.ExecApi(() => d.DestroyStatus(rts.Status.Id) == null))
                throw new ApplicationException();
        }

        #endregion

        #region Remove tweet

        public static void RemoveTweet(AccountInfo info, long tweetId)
        {
            var result = Task.Factory.StartNew(() =>
                        removeInjection.Execute(new Tuple<AccountInfo, long>(info, tweetId)));
            var ex = result.Exception;
            if (ex != null)
            {
                NotifyStorage.Notify("ツイートを削除できませんでした(@" + info.ScreenName + ")");
                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError, "ツイート削除時にエラーが発生しました");
            }
        }

        private static InjectionPort<Tuple<AccountInfo, long>> removeInjection =
            new InjectionPort<Tuple<AccountInfo, long>>(a => RemoveTweetSink(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, long>> RemoveInjection
        {
            get { return removeInjection.GetInterface(); }
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
                    if (tweet.InReplyToStatusId != 0)
                    {
                        var s = TweetStorage.Get(tweet.InReplyToStatusId);
                        if (s != null)
                            s.RemoveInReplyToThis(tweetId);
                    }
                    TweetStorage.Remove(tweetId);
                    NotifyStorage.Notify("削除しました:" + tweet.ToString());
                }
            }
            else
            {
                NotifyStorage.Notify("ツイートを削除できませんでした(@" + info.ScreenName + ")");
            }
        }

        #endregion

        #region Remove DirectMessage

        public static void RemoveDirectMessage(AccountInfo info, long tweetId)
        {
            var result = Task.Factory.StartNew(() =>
                removeDMInjection.Execute(new Tuple<AccountInfo, long>(info, tweetId)));

            var ex = result.Exception;
            if (ex != null)
            {
                NotifyStorage.Notify("ダイレクトメッセージを削除できませんでした(@" + info.ScreenName + ")");
                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError, "ダイレクトメッセージ削除時にエラーが発生しました");
            }
        }

        private static InjectionPort<Tuple<AccountInfo, long>> removeDMInjection =
            new InjectionPort<Tuple<AccountInfo, long>>(a => RemoveDMSink(a.Item1, a.Item2));

        public static IInjectionPort<Tuple<AccountInfo, long>> RemoveDMInjection
        {
            get { return removeDMInjection.GetInterface(); }
        }

        private static void RemoveDMSink(AccountInfo info, long tweetId)
        {
            var tweet = ApiHelper.ExecApi(() => info.DestroyDirectMessage(tweetId));
            if (tweet != null)
            {
                if (tweet.Id != tweetId)
                {
                    NotifyStorage.Notify("削除には成功しましたが、ダイレクトメッセージIDが一致しません。(" + tweetId.ToString() + " -> " + tweet.Id.ToString() + ")");
                }
                else
                {
                    TweetStorage.Remove(tweetId);
                    NotifyStorage.Notify("削除しました:" + tweet.ToString());
                }
            }
            else
            {
                NotifyStorage.Notify("ダイレクトメッセージを削除できませんでした(@" + info.ScreenName + ")");
            }
        }

        #endregion
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
