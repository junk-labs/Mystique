using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Inscribe.Text;

namespace Inscribe.Storage
{
    public static class HashtagStorage
    {
        private static object hashtagLocker = new object();
        private static HashSet<string> hashtagSet = new HashSet<string>();

        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized)
                throw new InvalidOperationException("HashtagStorage is already initialized.");
            _initialized = true;
            TweetStorage.TweetStorageChanged += new EventHandler<TweetStorageChangedEventArgs>(TweetStorage_TweetStorageChanged);
        }

        static void TweetStorage_TweetStorageChanged(object sender, TweetStorageChangedEventArgs e)
        {
            if (e.ActionKind == TweetActionKind.Added)
            {
                Task.Factory.StartNew(() =>
                {
                    RegularExpressions.HashRegex.Matches(e.Tweet.TweetText)
                        .OfType<Match>()
                        .Select(m => m.Value)
                        .ForEach(t => AddHashtag(t));
                });
            }
        }

        /// <summary>
        /// ハッシュタグを追加します。先頭の#や空白はトリムされます。
        /// </summary>
        public static bool AddHashtag(string hashtag)
        {
            hashtag = hashtag.Trim().TrimStart('#');
            lock (hashtagLocker)
            {
                return hashtagSet.Add(hashtag);
            }
        }

        /// <summary>
        /// ハッシュタグ一覧を取得します。(#は含みません。)
        /// </summary>
        public static string[] GetHashtags()
        {
            lock (hashtagLocker)
            {
                return hashtagSet.ToArray();
            }
        }
    }
}
