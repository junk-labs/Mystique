using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Dulcet.Twitter;
using Inscribe.Communication.CruiseControl.Core;
using Inscribe.Storage;

namespace Inscribe.Communication.CruiseControl
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
                return Math.Sqrt(tp / (tp + this.ReceiveCount * timesPerTweet));
            }
        }

        protected abstract int ReceiveCount { get; }

        public sealed override void DoWork()
        {
            try
            {
                var received = (GetTweets() ?? new TwitterStatusBase[0]).ToArray();
                previousReceived = DateTime.Now;
                var newbiesCount = received.Count(s => TweetStorage.Contains(s.Id) == TweetExistState.Unreceived);
                received.ForEach(s => TweetStorage.Register(s));
                var pivotarray = received.Take(TwitterDefine.IntervalLookPrevious).ToArray();
                var pivot = pivotarray.LastOrDefault();
                var newest = received.FirstOrDefault();
                if (pivot != null && newest != null)
                {
                    newbiesRate = (double)newbiesCount / received.Length;
                    if (newbiesRate < TwitterDefine.MinNewbiesRate)
                        newbiesRate = TwitterDefine.MinNewbiesRate;
                    timesPerTweet = (double)(newest.CreatedAt.Subtract(pivot.CreatedAt)).TotalMilliseconds / pivotarray.Length;
                    if (timesPerTweet > TwitterDefine.TimesPerTweetMaximumValue)
                        timesPerTweet = TwitterDefine.TimesPerTweetMaximumValue;
                }
                else
                {
                    // 受信すべきタイムラインが無い
                    // 受信レートを最小にして様子を見る
                    newbiesRate = TwitterDefine.MinNewbiesRate;
                    timesPerTweet = TwitterDefine.TimesPerTweetMaximumValue;
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
