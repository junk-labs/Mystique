using System;
using System.Xml.Linq;
using Dulcet.Util;

namespace Dulcet.Twitter
{
    public class TwitterUser
    {
        #region ファクトリ メソッド

        public static TwitterUser FromNode(XElement node)
        {
            if (node == null || node.Element("id") == null)
                throw new ArgumentNullException("node");
            // IDだけが存在して、screen_nameが取得できないことがある
            return new TwitterUser()
            {
                NumericId = node.Element("id").ParseLong(),
                ScreenName = node.Element("screen_name").ParseString() ?? node.Element("id").ParseString(), // screen_nameが無いことがある
                UserName = node.Element("name").ParseString(),
                Location = node.Element("location").ParseString(),
                Bio = node.Element("description").ParseString(),
                ProfileImage = node.Element("profile_image_url").ParseUri(),
                Web = node.Element("url").ParseString(),
                IsProtected = node.Element("protected").ParseBool(),
                IsVerified = node.Element("verified").ParseBool(),
                Tweets = node.Element("statuses_count").ParseLong(),
                Favorites = node.Element("favourites_count").ParseLong(),
                Followings = node.Element("friends_count").ParseLong(),
                Followers = node.Element("followers_count").ParseLong(),
                Listed = node.Element("listed_count").ParseLong(),
                CreatedAt = node.Element("created_at").ParseDateTime("ddd MMM d HH':'mm':'ss zzz yyyy"),
            };
        }

        #endregion

        private TwitterUser() { }

        /// <summary>
        /// 数値ID
        /// </summary>
        public long NumericId { get; set; }

        /// <summary>
        /// スクリーン名(@ID)
        /// </summary>
        public string ScreenName { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// ユーザーの場所
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// ユーザーのプロフィール情報
        /// </summary>
        public string Bio { get; set; }

        protected Uri _ProfileImage;
        /// <summary>
        /// ユーザーのプロフィール画像URL
        /// </summary>
        public Uri ProfileImage
        {
            get
            {
                return this._ProfileImage;
            }
            set
            {
                if((value.Host == "pbs.twimg.com") && (value.Scheme == "http"))
                {
                    this._ProfileImage = new Uri(value.OriginalString.Replace("http://", "https://"));
                }
                else
                {
                    this._ProfileImage = value;
                }
            }
        }

        /// <summary>
        /// ユーザーのWeb
        /// </summary>
        public string Web { get; set; }

        /// <summary>
        /// プロテクトユーザーであるか
        /// </summary>
        public bool IsProtected { get; set; }

        /// <summary>
        /// 公式認証されたユーザーであるか
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// フォロワーの数
        /// </summary>
        public long Followers { get; set; }

        /// <summary>
        /// フォローの数
        /// </summary>
        public long Followings { get; set; }

        /// <summary>
        /// アカウント作成日
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// お気に入りに登録したツイートの数
        /// </summary>
        public long Favorites { get; set; }

        /// <summary>
        /// リストに入っている数
        /// </summary>
        public long Listed { get; set; }

        /// <summary>
        /// ツイート数
        /// </summary>
        public long Tweets { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as TwitterUser);
        }

        public bool Equals(TwitterUser user)
        {
            return user != null && user.NumericId == this.NumericId;
        }

        public override int GetHashCode()
        {
            return (int)this.NumericId;
        }

        /// <summary>
        /// Krile内のインスタンスの新旧比較に利用します。
        /// </summary>
        public readonly DateTime CreatedTimestamp = DateTime.Now;
    }
}
