using System;
using System.Threading;
using Inscribe.Communication.Streaming.Connection;
using Inscribe.Model;
using Inscribe.Storage;

namespace Inscribe.Communication.Streaming
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
    /// User Streamsの受信クラス
    /// </summary>
    public class UserStreamsReceiver : IDisposable
    {
        private readonly AccountInfo linked;
        public AccountInfo LinkedAccountInfo
        {
            get { return this.linked; }
        }

        private UserStreamsConnection usConnection;

        private string[] _queries = null;
        /// <summary>
        /// このレシーバに関連付けるクエリー
        /// </summary>
        public string[] Queries
        {
            get { return this._queries ?? new string[0]; }
            set { this._queries = value; }
        }

        public UserStreamsReceiver(AccountInfo linkAccount)
        {
            this.linked = linkAccount;
            this.usConnection = null;
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
            lui.ConnectionState = ConnectionState.Disconnected;
            if (lui.AccoutProperty.UseUserStreams)
            {
                try
                {
                    lui.ConnectionState = ConnectionState.WaitNetwork;
                    new Robustness.NetworkTest().Test();
                    lui.ConnectionState = ConnectionState.WaitTwitter;
                    new Robustness.TwitterTest().Test();

                    lui.ConnectionState = ConnectionState.TryConnection;
                    // User Streams接続の開始
                    this.usConnection = UserStreamsConnection.Connect(lui, Queries);
                    lui.ConnectionState = ConnectionState.Connected;
                }
                catch (Exception e)
                {
                    ExceptionStorage.Register(e, ExceptionCategory.TwitterError, "User Streams接続に失敗しました。");
                    lui.ConnectionState = ConnectionState.Disconnected;
                }
            }
        }

        /// <summary>
        /// 受信を停止し、クリーンアップします。
        /// </summary>
        public void Dispose()
        {
            var us = Interlocked.Exchange(ref this.usConnection, null);
            if (us != null)
                us.Dispose();
        }
    }
}
