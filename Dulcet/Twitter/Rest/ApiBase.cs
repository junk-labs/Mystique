using System.Collections.Generic;
using System.Xml.Linq;
using Dulcet.Twitter.Credential;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Twitter api uri (v1)
        /// </summary>
        private static string TwitterUri { get { return "http://api.twitter.com/1/"; } }

        /// <summary>
        /// Formatting target uri and request api
        /// </summary>
        /// <remarks>
        /// For twitter api version 1
        /// </remarks>
        private static XDocument RequestAPIv1(this CredentialProvider provider, string partial, CredentialProvider.RequestMethod method, IEnumerable<KeyValuePair<string, string>> param)
        {
            var target = TwitterUri + (partial.EndsWith("/") ? partial.Substring(1) : partial);
            return provider.RequestAPI(target, method, param);
        }

    }
}
