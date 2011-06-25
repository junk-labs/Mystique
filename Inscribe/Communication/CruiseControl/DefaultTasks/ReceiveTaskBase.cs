using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Dulcet.Twitter;
using Inscribe.Storage;
using Supervisor;

namespace Inscribe.Communication.CruiseControl.DefaultTasks
{
    public abstract class ReceiveTaskBase : ScheduledTask
    {
        private double newbiesRate = 1.0;

        private double timesPerTweet = 0.0;

        private DateTime previousReceived = DateTime.MinValue;

        public sealed override double Rate
        {
            get
            {
                var t = DateTime.Now.Subtract(previousReceived).TotalMilliseconds;
                var tp = t * newbiesRate;
                if(newbiesRate < TwitterDefine.MinNewbiesRate)
                    tp = t * TwitterDefine.MinNewbiesRate;
                return tp / (tp + this.ReceiveCount * timesPerTweet);
            }
        }

        protected abstract int ReceiveCount { get; }

        public sealed override void DoWork()
        {
            try
            {
                var received = GetTweets().ToArray();
                previousReceived = DateTime.Now;
                var newbiesCount = received.Count(s => TweetStorage.Contains(s.Id) == TweetExistState.Unreceived);
                received.ForEach(s => TweetStorage.Register(s));
                var oldest = received.LastOrDefault();
                var newest = received.FirstOrDefault();
                if (oldest != null && newest != null)
                {
                    newbiesRate = (double)newbiesCount / received.Length;
                    timesPerTweet = (double)(newest.CreatedAt.Subtract(oldest.CreatedAt)).TotalMilliseconds / received.Length;
                }
            }
            catch (WebException ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.TwitterError, "Twitterとの通信に失敗しました。");
            }
            catch (IOException ioex)
            {
                ExceptionStorage.Register(ioex, ExceptionCategory.TwitterError, "ネットワークでエラーが発生しました。");
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.InternalError, "ステータス受信時に内部エラーが発生しました。");
            }
        }

        protected abstract IEnumerable<TwitterStatusBase> GetTweets();
    }
}
