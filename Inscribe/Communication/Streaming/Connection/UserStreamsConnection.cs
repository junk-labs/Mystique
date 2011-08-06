using System;
using System.Net;
using System.Threading;
using Dulcet.Twitter.Streaming;
using Inscribe.Configuration;
using Inscribe.Model;
using Inscribe.Storage;
using Inscribe.Threading;
using Dulcet.Twitter.Credential;

namespace Inscribe.Communication.Streaming.Connection
{
    public class UserStreamsConnection : IDisposable
    {

        #region Static Variables and Methods 

        static StreamingCore streamCore;
        static Thread streamPump;
        static UserStreamsConnection()
        {
            streamCore = new StreamingCore();
            streamCore.OnExceptionThrown += new Action<Exception>(streamCore_OnExceptionThrown);
            streamCore.OnDisconnected += new Action<Dulcet.Twitter.Credential.CredentialProvider, bool>(streamCore_OnDisconnected);
            ThreadHelper.Halt += () => streamPump.Abort();
            ThreadHelper.Halt += () => streamCore.Dispose();
            streamPump = new Thread(PumpTweets);
            streamPump.Start();
        }

        static void streamCore_OnExceptionThrown(Exception obj)
        {
            ExceptionStorage.Register(obj, ExceptionCategory.TwitterError, "User Streams接続でエラーが発生しました。");
        }

        static void streamCore_OnDisconnected(CredentialProvider provider, bool expected)
        {
            var info = provider as AccountInfo;
            if (info == null)
            {
                ExceptionStorage.Register(new Exception(), ExceptionCategory.InternalError, "プロバイダがアカウント情報を保持していません。");
                return;
            }

            info.ConnectionState = ConnectionState.Disconnected;

            // 再接続処理
            if (!expected)
            {
                UserStreamsReceiverManager.RefreshReceiver(info);
            }
        }

        /// <summary>
        /// ツイートのポンピングスレッド
        /// </summary>
        private static void PumpTweets()
        {
            try
            {
                foreach (var s in streamCore.EnumerateStreamingElements())
                {
                    try
                    {
                        RegisterStreamElement(s.Item1 as AccountInfo, s.Item2);
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        streamCore_OnExceptionThrown(e);
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, "メッセージポンピングシステムにエラーが発生しました。");
            }
        }

        /// <summary>
        /// ストリームエレメントを処理します。
        /// </summary>
        /// <param name="info">ストリームエレメントを受信したアカウント</param>
        /// <param name="elem">ストリームエレメント</param>
        private static void RegisterStreamElement(AccountInfo info, StreamingEvent elem)
        {
            switch (elem.Kind)
            {
                case ElementKind.Status:
                    // 通常ステータスを受信した
                    TweetStorage.Register(elem.Status);
                    break;
                case ElementKind.Favorite:
                    var avm = TweetStorage.Register(elem.Status);
                    if (avm == null) return;
                    var uavm = UserStorage.Get(elem.SourceUser);
                    if (avm.RegisterFavored(uavm))
                        EventStorage.OnFavored(avm, uavm);
                    break;
                case ElementKind.Unfavorite:
                    var rvm = TweetStorage.Register(elem.Status);
                    if (rvm == null) return;
                    var urvm = UserStorage.Get(elem.SourceUser);
                    if (rvm.RemoveFavored(urvm))
                        EventStorage.OnUnfavored(rvm, urvm);
                    break;
                case ElementKind.Delete:
                    TweetStorage.Remove(elem.DeletedStatusId);
                    break;
                case ElementKind.ListUpdated:
                case ElementKind.ListMemberAdded:
                case ElementKind.ListMemberRemoved:
                case ElementKind.ListSubscribed:
                case ElementKind.ListUnsubscribed:
                    // TODO: do something
                    break;
                case ElementKind.Follow:
                case ElementKind.Unfollow:
                    var affect = AccountStorage.Get(elem.SourceUser.ScreenName);
                    var effect = AccountStorage.Get(elem.TargetUser.ScreenName);
                    if (affect != null)
                    {
                        // Add/Remove followings
                        if (elem.Kind == ElementKind.Follow)
                            affect.RegisterFollowing(elem.TargetUser.NumericId);
                        else
                            affect.RemoveFollowing(elem.TargetUser.NumericId);
                    }
                    if (effect != null)
                    {
                        // Add/Remove followers
                        if (elem.Kind == ElementKind.Follow)
                            effect.RegisterFollower(elem.SourceUser.NumericId);
                        else
                            effect.RemoveFollower(elem.SourceUser.NumericId);
                    }
                    if (elem.Kind == ElementKind.Follow)
                        EventStorage.OnFollowed(UserStorage.Get(elem.SourceUser), UserStorage.Get(elem.TargetUser));
                    else
                        EventStorage.OnRemoved(UserStorage.Get(elem.SourceUser), UserStorage.Get(elem.TargetUser));
                    break;
                case ElementKind.Blocked:
                    if (info == null) break;
                    info.RemoveFollowing(elem.TargetUser.NumericId);
                    info.RemoveFollower(elem.TargetUser.NumericId);
                    info.RegisterBlocking(elem.TargetUser.NumericId);
                    // TODO: notify events
                    break;
                case ElementKind.Unblocked:
                    if (info == null) break;
                    info.RemoveBlocking(elem.TargetUser.NumericId);
                    // TODO: Notify events
                    break;
            }
        }

        public static UserStreamsConnection Connect(AccountInfo account, string[] queries)
        {
            if (streamCore == null)
                throw new InvalidOperationException("Stream core is not initialized.");
            return new UserStreamsConnection(account, queries);
        }

        #endregion // Static Variables and Methods

        private StreamingConnection connection = null;

        private UserStreamsConnection(AccountInfo account, string[] queries)
        {
            string track = null;
            if (queries != null && queries.Length > 0)
            {
                track = String.Join(",", queries);
            }

            try
            {
                Connect(account, track);
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                throw new Exception("@" + account.ScreenName + ": 接続できませんでした。", e);
            }
        }

        /// <summary>
        /// ストリーミングAPIへ接続します。<para />
        /// 接続に失敗した場合、規定アルゴリズムに沿って自動で再接続を試みます。<para />
        /// それでも失敗する場合はfalseを返します。この場合、再接続を試みてはいけません。
        /// </summary>
        /// <param name="track">トラック対象キーワード</param>
        private void Connect(AccountInfo info, string track)
        {
            try
            {
                int failureCount = 0;
                while (true)
                {
                    // 接続
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("Start connection User Streams with track:" + track);
                        connection = streamCore.ConnectNew(
                            info, StreamingDescription.ForUserStreams(
                            track: track, repliesAll: info.AccoutProperty.UserStreamsRepliesAll));
                    }
                    catch (WebException we)
                    {
                        if ((int)we.Status == 420)
                        {
                            // 多重接続されている
                            throw new WebException("@" + info.ScreenName + ": 多重接続されています。接続できません。");
                        }

                        if (we.Status == WebExceptionStatus.Timeout) // タイムアウト例外なら再試行する
                        {
                            NotifyStorage.Notify("@" + info.ScreenName + ": User Streams接続がタイムアウトしました。再試行します...");
                        }
                        else
                        {
                            string descText = we.Status.ToString();
                            if (we.Status == WebExceptionStatus.ProtocolError)
                            {
                                var hwr = we.Response as HttpWebResponse;
                                if (hwr != null)
                                {
                                    descText += " - " + hwr.StatusCode + " : " + hwr.StatusDescription;
                                }
                            }
                            throw new WebException("接続に失敗しました。(" + descText + ")");
                        }
                    }

                    if (connection != null)
                    {
                        // Connection succeed
                        return;
                    }

                    if (failureCount > 0)
                    {
                        if (failureCount > Setting.Instance.ConnectionProperty.UserStreamsConnectionFailedMaxWaitSec)
                        {
                            throw new WebException("User Streamsへの接続に失敗しました。");
                        }
                        else
                        {
                            NotifyStorage.Notify("@" + info.ScreenName + ": User Streamsへの接続に失敗。再試行まで" + (failureCount / 1000).ToString() + "秒待機...", failureCount / 1000);

                            // ウェイトカウント
                            Thread.Sleep(failureCount);
                            NotifyStorage.Notify("@" + info.ScreenName + ": User Streamsへの接続を再試行します...");
                        }
                    }
                    else
                    {
                        NotifyStorage.Notify("@" + info.ScreenName + ": User Streamsへの接続に失敗しました。再試行します...");
                    }

                    // 最初に失敗したらすぐに再接続
                    // ２回目以降はその倍に増やしていく
                    // 300を超えたら接続失敗で戻る
                    if (failureCount == 0)
                    {
                        failureCount = Setting.Instance.ConnectionProperty.UserStreamsConnectionFailedInitialWaitSec;
                    }
                    else
                    {
                        failureCount *= 2;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new WebException("User Streamsへの接続に失敗しました。", e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UserStreamsConnection()
        {
            Dispose(false);
        }

        bool disposed = false;
        private void Dispose(bool disposing)
        {
            if (this.disposed) return;
            this.disposed = true;
            if(this.connection != null)
                this.connection.Dispose();
        }
    }
}
