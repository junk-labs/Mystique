using System;
using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter.Credential;
using Dulcet.Util;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get list statuses with full params
        /// </summary>
        public static IEnumerable<TwitterStatus> GetListStatuses(this CredentialProvider provider, string userScreenName, string listSlug, string sinceId = null, string maxId = null, long? perPage = null, long? page = null, bool? includeRts = null, bool? includeEntities = null)
        {
            listSlug = listSlug.Replace("_", "-");

            var para = new List<KeyValuePair<string, string>>();

            if (!String.IsNullOrEmpty(listSlug))
                para.Add(new KeyValuePair<string, string>("slug", listSlug));

            if (!String.IsNullOrEmpty(userScreenName))
                para.Add(new KeyValuePair<string, string>("owner_screen_name", userScreenName));

            if (!String.IsNullOrEmpty(sinceId))
                para.Add(new KeyValuePair<string, string>("since_id", sinceId));

            if (!String.IsNullOrEmpty(maxId))
                para.Add(new KeyValuePair<string, string>("max_id", maxId));

            if (perPage != null)
                para.Add(new KeyValuePair<string, string>("per_page", perPage.ToString()));

            if (page != null)
                para.Add(new KeyValuePair<string, string>("page", page.ToString()));

            if (includeRts != null && includeRts.Value)
                para.Add(new KeyValuePair<string, string>("include_rts", "true"));

            if (includeEntities != null && includeEntities.Value)
                para.Add(new KeyValuePair<string, string>("include_entities", "true"));

            return provider.GetTimeline("lists/statuses.json", para);
        }

        /// <summary>
        /// Get list members
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user's id</param>
        /// <param name="slug">list id</param>
        public static IEnumerable<TwitterUser> GetListMembersAll(this CredentialProvider provider, string userScreenName, string slug)
        {
            slug = slug.Replace("_", "-");

            var para = new List<KeyValuePair<string, string>>();
            if (!String.IsNullOrEmpty(slug))
                para.Add(new KeyValuePair<string, string>("slug", slug));
            if (!String.IsNullOrEmpty(userScreenName))
                para.Add(new KeyValuePair<string, string>("owner_screen_name", userScreenName));

            return provider.GetUsersAll("lists/members.json", para);
        }

        /// <summary>
        /// Get list with full params
        /// </summary>
        private static IEnumerable<TwitterList> GetLists(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para, long? cursor, out long prevCursor, out long nextCursor)
        {
            var npara = new List<KeyValuePair<string, string>>();
            foreach (var pair in para)
            {
                npara.Add(pair);
            }
            if (cursor != null)
            {
                npara.Add(new KeyValuePair<string, string>("cursor", cursor.ToString()));
            }

            prevCursor = 0;
            nextCursor = 0;

            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, npara);
            if (doc == null)
                return null;
            var nc = doc.Root.Element("next_cursor");
            if (nc != null)
                nextCursor = (long)nc.ParseLong();
            var pc = doc.Root.Element("previous_cursor");
            if (pc != null)
                prevCursor = (long)pc.ParseLong();

            System.Diagnostics.Debug.WriteLine("Lists :::" + Environment.NewLine + doc);

            return doc.Root.Elements().Select(TwitterList.FromNode).Where(n => n != null);
        }

        /// <summary>
        /// Get list enumerations all
        /// </summary>
        private static IEnumerable<TwitterList> GetListsAll(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            while (n_cursor != 0)
            {
                var lists = provider.GetLists(partialUri, para, c_cursor, out p, out n_cursor);
                if (lists != null)
                    foreach (var u in lists)
                        yield return u;
                c_cursor = n_cursor;
            }
        }

        /// <summary>
        /// Get lists you following
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">target user id</param>
        public static IEnumerable<TwitterList> GetFollowingListsAll(this CredentialProvider provider, string userScreenName)
        {
            var partialUri = "lists/list.json";
            var para = new List<KeyValuePair<string, string>>();
            if (!String.IsNullOrEmpty(userScreenName))
                para.Add(new KeyValuePair<string, string>("screen_name", userScreenName));
            return provider.GetListsAll(partialUri, para);
        }

        /// <summary>
        /// Get single list data
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="userScreenName">list owner user's id</param>
        /// <param name="listSlug">list slug</param>
        public static TwitterList GetList(this CredentialProvider provider, string userScreenName, string listSlug)
        {
            var para = new List<KeyValuePair<string, string>>();
            if (!String.IsNullOrEmpty(userScreenName))
                para.Add(new KeyValuePair<string, string>("owner_screen_name", userScreenName));
            if (!String.IsNullOrEmpty(listSlug))
                para.Add(new KeyValuePair<string, string>("slug", listSlug));
            var list = provider.RequestAPIv1("lists/show.json",
                 CredentialProvider.RequestMethod.GET, para).Root;
            if (list != null)
                return TwitterList.FromNode(list);
            return null;
        }
    }
}
