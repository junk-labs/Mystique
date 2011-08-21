using System.Collections.Generic;
using System.Linq;
using Dulcet.Twitter.Credential;
using Dulcet.Util;
using System;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Get users
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsers(this CredentialProvider provider, string partialUri, IEnumerable<KeyValuePair<string, string>> para, out long prevCursor, out long nextCursor)
        {
            prevCursor = 0;
            nextCursor = 0;
            var doc = provider.RequestAPIv1(partialUri, CredentialProvider.RequestMethod.GET, para);
            if (doc == null)
                return null; // request returns error ?
            List<TwitterUser> users = new List<TwitterUser>();
            var nc = doc.Root.Element("next_cursor");
            if (nc != null)
                nextCursor = (long)nc.ParseLong();
            var pc = doc.Root.Element("previous_cursor");
            if (pc != null)
                prevCursor = (long)pc.ParseLong();
            System.Diagnostics.Debug.WriteLine("GetUser:::" + Environment.NewLine + doc.ToString());
            return doc.Root.Element("users").Elements().Select(n => TwitterUser.FromNode(n)).Where(s => s != null);
        }

        /// <summary>
        /// Get users with full params
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsers(this CredentialProvider provider, string partialUri, long? userId, string screenName, long? cursor, out long prevCursor, out long nextCursor)
        {
            List<KeyValuePair<string, string>> para = new List<KeyValuePair<string, string>>();
            if (userId != null)
            {
                para.Add(new KeyValuePair<string, string>("user_id", userId.ToString()));
            }
            if (screenName != null)
            {
                para.Add(new KeyValuePair<string, string>("screen_name", screenName));
            }
            if (cursor != null)
            {
                para.Add(new KeyValuePair<string, string>("cursor", cursor.ToString()));
            }
            return provider.GetUsers(partialUri, para, out prevCursor, out nextCursor);
        }

        /// <summary>
        /// Get users with use cursor params
        /// </summary>
        private static IEnumerable<TwitterUser> GetUsersAll(this CredentialProvider provider, string partialUri, long? userId, string screenName)
        {
            long n_cursor = -1;
            long c_cursor = -1;
            long p;
            while (n_cursor != 0)
            {
                var users = provider.GetUsers(partialUri, userId, screenName, c_cursor, out p, out n_cursor);
                if (users != null)
                    foreach (var u in users)
                        yield return u;
                c_cursor = n_cursor;
            }
        }

    }
}
