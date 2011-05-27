using System;
using System.Threading;
using Inscribe.Communication.MainTimeline.Connection;
using Inscribe.Model;
using Inscribe.Storage;

namespace Inscribe.Communication.MainTimeline
{
    [Flags()]
    public enum ReceiveKinds
    {
        Home = 1,
        Mentions = 2,
        DirectMessages = 4,
        Favorites = 8,
        MyTweets = 16,
    }

    /// <summary>
    /// タイムラインの受信クラス
    /// </summary>
    public class Receiver : IDisposable
    {
        private readonly AccountInfo linked;
        public AccountInfo LinkedAccountInfo
        {
            get { return this.linked; }
        }

        private UserStreamsConnection usConnection;
        private RestConnection restConnection;

        /// <summary>
        /// このレシーバに関連付けるクエリー
        /// </summary>
        public string[] Queries { get; set; }

        public Receiver(AccountInfo linkAccount)
        {
            this.linked = linkAccount;
            this.restConnection = new RestConnection(linkAccount);
            this.usConnection = null;
        }

        /// <summary>
        /// 指定したタイムラインを強制的に受信します。
        /// </summary>
        public void ReceiveTimeline(ReceiveKinds receives)
        {
            var rc = this.restConnection;
            if (rc == null) return;
            if (receives.HasFlag(ReceiveKinds.Home))
                rc.ReceiveHomeTimeline();
            if (receives.HasFlag(ReceiveKinds.Mentions))
                rc.ReceiveMentions();
            if (receives.HasFlag(ReceiveKinds.DirectMessages))
                rc.ReceiveDirectMessages();
            if (receives.HasFlag(ReceiveKinds.Favorites))
                rc.ReceiveFavors();
            if (receives.HasFlag(ReceiveKinds.MyTweets))
                rc.ReceiveMyTweets();
        }

        /// <summary>
        /// 現在の設定状態に合わせて接続状態を更新します。
        /// </summary>
        public void UpdateConnection()
        {
            var lui = this.LinkedAccountInfo;
            // 一旦User Streamsを停止する
            var usc = Interlocked.Exchange(ref this.usConnection, null);
            if (usc != null)
                usc.Dispose();
            lui.UserStreamsConnectionState = ConnectionState.Disconnected;
            if (lui.AccoutProperty.ConnectionKind == Configuration.Elements.ConnectionKind.None)
            {
                // REST定期受信を停止
                this.restConnection.HomeReceiveInterval = 0;
                this.restConnection.MentionReceiveInterval = 0;
                this.restConnection.DirectMessageReceiveInterval = 0;
                this.restConnection.FavoredReceiveInterval = 0;
                lui.RestConnectionState = ConnectionState.Disconnected;
            }
            else
            {
                // RESTポーリング
                this.restConnection.HomeReceiveInterval = lui.AccoutProperty.RestHomeInterval;
                this.restConnection.MentionReceiveInterval = lui.AccoutProperty.RestMentionsInterval;
                this.restConnection.DirectMessageReceiveInterval = lui.AccoutProperty.RestDirectMessagesInterval;
                this.restConnection.FavoredReceiveInterval = lui.AccoutProperty.RestFavoredInterval;
                lui.RestConnectionState = ConnectionState.Connected;
                if (lui.AccoutProperty.ConnectionKind == Configuration.Elements.ConnectionKind.UserStreams)
                {
                    try
                    {
                        lui.UserStreamsConnectionState = ConnectionState.WaitNetwork;
                        new Robustness.NetworkTest().Test();
                        lui.UserStreamsConnectionState = ConnectionState.WaitTwitter;
                        new Robustness.TwitterTest().Test();

                        lui.UserStreamsConnectionState = ConnectionState.TryConnection;
                        // User Streams接続の開始
                        this.usConnection = UserStreamsConnection.Connect(lui, Queries);
                        lui.UserStreamsConnectionState = ConnectionState.Connected;
                        // REST ポーリングインターバルのオーバーライト
                        this.restConnection.HomeReceiveInterval = lui.AccoutProperty.UserStreamsOverrideRestHomeInterval ?? TwitterDefine.UserStreamsRestHomeInterval;
                        this.restConnection.MentionReceiveInterval = lui.AccoutProperty.UserStreamsOverrideRestMentionsInterval ?? TwitterDefine.UserStreamsRestMentionsInterval;
                        this.restConnection.DirectMessageReceiveInterval = lui.AccoutProperty.UserStreamsOverrideRestDirectMessagesInterval ?? TwitterDefine.UserStreamsRestDirectMessagesInterval;
                        this.restConnection.FavoredReceiveInterval = lui.AccoutProperty.UserStreamsOverrideRestFavoredInterval ?? TwitterDefine.UserStreamsRestFavoredInterval;
                    }
                    catch (Exception e)
                    {
                        ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "User Streams接続に失敗しました。");
                    }
                }
            }
        }

        /// <summary>
        /// 受信を停止し、クリーンアップします。
        /// </summary>
        public void Dispose()
        {
            var rc = Interlocked.Exchange(ref this.restConnection, null);
            if (rc != null)
                rc.Dispose();
            var us = Interlocked.Exchange(ref this.usConnection, null);
            if (us != null)
                us.Dispose();
        }
    }
}
