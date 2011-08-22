using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dulcet.Twitter.Streaming;
using System.Threading;
using Dulcet.Twitter.Credential;
using Inscribe.Threading;
using Inscribe.Storage;
using Inscribe.Model;
using System.Net;
using Inscribe.Configuration;
using Dulcet.Twitter.Rest;
using System.Net.NetworkInformation;

namespace Inscribe.Communication.UserStreams
{
    public class UserStreamsConnection : IDisposable
    {
        #region Global thread

        static StreamingCore streamingCore;

        static Thread pumpThread;
        static UserStreamsConnection()
        {
            streamingCore = new StreamingCore();
            streamingCore.OnExceptionThrown += new Action<Exception>(streamingCore_OnExceptionThrown);
            streamingCore.OnDisconnected += new Action<StreamingConnection>(streamingCore_OnDisconnected);
            ThreadHelper.Halt += () =>
            {
                try
                {
                    streamingCore.Dispose();
                }
                catch { }
                try
                {
                    pumpThread.Abort();
                }
                catch { }
            };
            pumpThread = new Thread(PumpTweets);
            pumpThread.Start();
        }

        #region Handlers

        static void streamingCore_OnDisconnected(StreamingConnection con)
        {
            ConnectionManager.NotifyDisconnected(con.Provider as AccountInfo, con);
        }

        private static void streamingCore_OnExceptionThrown(Exception ex)
        {
            ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                "User Streams接続にエラーが発生しました。");
        }

        #endregion

        private static void PumpTweets()
        {
            try
            {
                foreach (var s in streamingCore.EnumerateStreamingElements())
                {
                    try
                    {
                        RegisterStreamElement(s.Item1 as AccountInfo, s.Item2);
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        ExceptionStorage.Register(ex, ExceptionCategory.TwitterError,
                            "User Streamsのツイート処理中にエラーが発生しました");
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                ExceptionStorage.Register(e, ExceptionCategory.InternalError, 
                    "メッセージポンピングシステムにエラーが発生しました。");
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

        #endregion

        internal AccountInfo AccountInfo;

        internal UserStreamsConnection(AccountInfo info)
        {
            this.AccountInfo = info;
        }

        /// <summary>
        /// 追加受信クエリを設定して、User Streams受信を開始します。<para />
        /// 接続に失敗したらあっさり例外吐きます。
        /// </summary>
        internal void Connect(String[] queries)
        {
            string track = null;
            if (queries != null && queries.Length > 0)
            {
                track = queries.JoinString(",");
            }

            try
            {
                this.ConnectCore(AccountInfo, track);
            }
            catch (ThreadAbortException) { }
            catch
            {
                throw;
            }
        }

        private StreamingConnection connection = null;

        /// <summary>
        /// ストリーミングAPIへ接続します。<para />
        /// 接続に失敗した場合、規定アルゴリズムに沿って自動で再接続を試みます。<para />
        /// それでも失敗する場合はfalseを返します。この場合、再接続を試みてはいけません。
        /// </summary>
        /// <param name="track">トラック対象キーワード</param>
        private void ConnectCore(AccountInfo info, string track)
        {
            try
            {
                int failureWaitSec = 0;
                while (true)
                {
                    // 接続
                    try
                    {
                        using (var n = NotifyStorage.NotifyManually("@" + info.ScreenName + ": インターネットへの接続を確認しています..."))
                        {
                            while (!NetworkInterface.GetIsNetworkAvailable())
                            {
                                info.ConnectionState = ConnectionState.WaitNetwork;
                                ConnectionManager.OnConnectionStateChanged(EventArgs.Empty);
                                // ネットワークが利用可能になるまでポーリング
                                Thread.Sleep(10000);
                            }
                        }

                        using (var n = NotifyStorage.NotifyManually("@" + info.ScreenName + ": 接続のテストをしています..."))
                        {
                            info.ConnectionState = ConnectionState.WaitTwitter;
                            ConnectionManager.OnConnectionStateChanged(EventArgs.Empty);
                            try
                            {
                                if (!ApiHelper.ExecApi(() => info.Test()))
                                    throw new Exception();
                            }
                            catch
                            {
                                throw new WebException("Twitterが応答を停止しています。");
                            }
                        }

                        using (var n = NotifyStorage.NotifyManually("@" + info.ScreenName + ": 接続しています..."))
                        {
                            info.ConnectionState = ConnectionState.TryConnection;
                            ConnectionManager.OnConnectionStateChanged(EventArgs.Empty);
                            System.Diagnostics.Debug.WriteLine("User Streams Connection with Track: " + track);
                            connection = streamingCore.ConnectNew(
                                info, StreamingDescription.ForUserStreams(TwitterDefine.UserStreamsTimeout,
                                track: track, repliesAll: info.AccoutProperty.UserStreamsRepliesAll));
                        }
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
                            if (we.Status == WebExceptionStatus.UnknownError)
                            {
                                descText = we.Message;
                            }
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
                    catch
                    {
                        throw;
                    }

                    if (connection != null)
                    {
                        // Connection successful
                        info.ConnectionState = ConnectionState.Connected;
                        ConnectionManager.OnConnectionStateChanged(EventArgs.Empty);
                        return;
                    }

                    if (failureWaitSec > 0)
                    {
                        if (failureWaitSec > Setting.Instance.ConnectionProperty.UserStreamsConnectionFailedMaxWaitSec)
                        {
                            throw new WebException("User Streamsへの接続に失敗しました。");
                        }
                        else
                        {
                            NotifyStorage.Notify("@" + info.ScreenName + ": User Streamsへの接続に失敗。再試行まで" + failureWaitSec.ToString() + "秒待機...", failureWaitSec / 1000);

                            // ウェイトカウント
                            Thread.Sleep(failureWaitSec * 1000);
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
                    if (failureWaitSec == 0)
                    {
                        failureWaitSec = Setting.Instance.ConnectionProperty.UserStreamsConnectionFailedInitialWaitSec;
                    }
                    else
                    {
                        failureWaitSec *= 2;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                this.AccountInfo.ConnectionState = ConnectionState.Disconnected;
                ConnectionManager.OnConnectionStateChanged(EventArgs.Empty);
                throw new WebException("User Streamsへの接続に失敗しました。", e);
            }
        }

        /// <summary>
        /// 内包しているStreamingConnectionが渡されたインスタンスと同一であるか確認します。
        /// </summary>
        internal bool CheckUsingConnection(StreamingConnection con)
        {
            return this.connection == con;
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
            if (this.connection != null)
                this.connection.Dispose();
            this.connection = null;
        }

        public bool IsAlive
        {
            get
            {
                if (this.connection == null)
                    this.disposed = true;
                else if (!this.connection.IsAlive) // Connection finalized
                    this.Dispose();
                return !this.disposed;
            }
        }
    }
}
