using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dulcet.Twitter;
using Dulcet.Twitter.Rest;
using Inscribe.Configuration;
using Inscribe.Model;
using Inscribe.Storage;
using Inscribe.Threading;

namespace Inscribe.Communication.MainTimeline.Connection
{
    public class RestConnection : IDisposable
    {
        private readonly AccountInfo credential;

        private Timer receiveTimer = null;

        private object syncRoot = new object();

        private long homeReceiveCount = 0;

        private long mentionReceiveCount = 0;

        private long dmReceiveCount = 0;

        private long favReceiveCount = 0;
        
        public bool IsActivateRestReceiving { get; set; }

        public long HomeReceiveInterval { get; set; }

        public long MentionReceiveInterval { get; set; }

        public long DirectMessageReceiveInterval { get; set; }

        public long FavoredReceiveInterval { get; set; }

        public RestConnection(AccountInfo credential)
        {
            this.credential = credential;
            this.receiveTimer = new Timer(TimerTick, null, 1, 1);
            ThreadHelper.Halt += Dispose;
        }

        /// <summary>
        /// Twitter APIが利用可能であるかテストします。
        /// </summary>
        public bool Test()
        {
            return ApiHelper.ExecApi(() => credential.Test());
        }

        #region Endless Summer

        public IEnumerable<TwitterStatus> ReceiveHomeTimeline()
        {
            return ApiHelper.ExecApi(() => credential.GetHomeTimeline(count: Setting.Instance.ConnectionProperty.ApiTweetReceiveCount));
        }

        public IEnumerable<TwitterStatus> ReceiveMentions()
        {
            return ApiHelper.ExecApi(() => credential.GetMentions(count: Setting.Instance.ConnectionProperty.ApiTweetReceiveCount));
        }

        public IEnumerable<TwitterStatus> ReceiveMyTweets()
        {
            return ApiHelper.ExecApi(() => credential.GetUserTimeline(count: Setting.Instance.ConnectionProperty.ApiTweetReceiveCount));
        }

        public IEnumerable<TwitterDirectMessage> ReceiveDirectMessages()
        {
            return ApiHelper.ExecApi(() => credential.GetDirectMessages(count: Setting.Instance.ConnectionProperty.ApiTweetReceiveCount));
        }

        public IEnumerable<TwitterStatus> ReceiveFavors()
        {
            return ApiHelper.ExecApi(() => credential.GetFavorites());
        }

        #endregion

        private QueueTaskDispatcher twitterDispatcher = new QueueTaskDispatcher(2);

        private void TimerTick(object o)
        {
            if (!IsActivateRestReceiving) return;
            lock (syncRoot)
            {
                if (HomeReceiveInterval > 0)
                {
                    homeReceiveCount++;
                    if (homeReceiveCount > HomeReceiveInterval)
                    {
                        homeReceiveCount = 0;
                        twitterDispatcher.Enqueue(() => ReceiveHomeTimeline().ForEach(s => TweetStorage.Register(s)));
                    }
                }
                if (MentionReceiveInterval > 0)
                {
                    mentionReceiveCount++;
                    if (mentionReceiveCount > MentionReceiveInterval)
                    {
                        mentionReceiveCount = 0;
                        twitterDispatcher.Enqueue(() => ReceiveMentions().ForEach(s => TweetStorage.Register(s)));
                    }
                }
                if (DirectMessageReceiveInterval > 0)
                {
                    dmReceiveCount++;
                    if (dmReceiveCount > DirectMessageReceiveInterval)
                    {
                        dmReceiveCount = 0;
                        twitterDispatcher.Enqueue(() => ReceiveDirectMessages().ForEach(s => TweetStorage.Register(s)));
                    }
                }
                if (FavoredReceiveInterval > 0)
                {
                    favReceiveCount++;
                    if (favReceiveCount > FavoredReceiveInterval)
                    {
                        favReceiveCount = 0;
                        twitterDispatcher.Enqueue(() => ReceiveFavors().ForEach(s => TweetStorage.Register(s)));
                    }
                }
            }
        }

        /// <summary>
        /// 定期的な受信のためのタイマーを停止します。
        /// </summary>
        public void Dispose()
        {
            ThreadHelper.Halt -= Dispose;
            IsActivateRestReceiving = false;
            receiveTimer.Dispose();
            receiveTimer = null;
        }
    }
}
