using System;
using System.Xml.Linq;
using Dulcet.Util;

namespace Dulcet.Twitter
{
    public sealed class TwitterList
    {
        #region ファクトリ メソッド

        public static TwitterList FromNode(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            return new TwitterList()
            {
                Id = node.Element("id").ParseLong(),
                Name = node.Element("name").ParseString(),
                FullName = node.Element("full_name").ParseString(),
                Slug = node.Element("slug").ParseString(),
                Description = node.Element("description").ParseString(),
                SubscriberCount = node.Element("subscriber_count").ParseLong(),
                PartialUri = node.Element("uri").ParseString(),
                Mode = node.Element("mode").ParseString(),
                User = TwitterUser.FromNode(node.Element("user")),
            };
        }

        #endregion

        /// <summary>
        /// List id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// List name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List full-name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// List slug
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// List description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List subscriber count
        /// </summary>
        public long SubscriberCount { get; set; }

        /// <summary>
        /// List member count
        /// </summary>
        public long MemberCount { get; set; }

        /// <summary>
        /// List partial uri
        /// </summary>
        public string PartialUri { get; set; }

        /// <summary>
        /// List open mode
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Is private this list
        /// </summary>
        public bool IsPrivate
        {
            get { return this.Mode == "private"; }
        }

        /// <summary>
        /// Parent user
        /// </summary>
        public TwitterUser User { get; set; }

        /// <summary>
        /// Twitter list instance
        /// </summary>
        private TwitterList() { }

        /// <summary>
        /// Get list fullname
        /// </summary>
        public override string ToString()
        {
            return this.FullName;
        }

        /// <summary>
        /// Compare by ID.
        /// </summary>
        public override bool Equals(object obj)
        {
            var list = obj as TwitterList;
            if (list == null)
                return false;
            else
                return list.Id == this.Id;
        }

        /// <summary>
        /// Equals id of myself
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)this.Id;
        }

        /// <summary>
        /// リスト名の比較を行います。<para />
        /// ハイフンとアンダーバーが同一視されます。
        /// </summary>
        public bool CompareListName(string name)
        {
            return this.Name.ToLower().Replace('_', '-').Equals(name.ToLower().Replace('_', '-'));
        }
    }
}
