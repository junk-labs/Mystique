using System.Collections.Generic;
using Dulcet.Twitter.Credential;

namespace Dulcet.Twitter.Rest
{
    public static partial class Api
    {
        /// <summary>
        /// Favorites a tweet
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">the id of the tweet to favorite.</param>
        public static TwitterStatus CreateFavorites(this CredentialProvider provider, long id)
        {
            var para = CreateParamList();
            para.Add(new KeyValuePair<string, string>("id", id.ToString()));
            return provider.GetStatus("favorites/create.json", CredentialProvider.RequestMethod.POST, para);
        }

        /// <summary>
        /// Unfavorites a tweet
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">the id of the tweet to unffavorite.</param>
        public static TwitterStatus DestroyFavorites(this CredentialProvider provider, long id)
        {
            var para = CreateParamList();
            para.Add(new KeyValuePair<string, string>("id", id.ToString()));
            return provider.GetStatus("favorites/destroy.json", CredentialProvider.RequestMethod.POST, para);
        }

        //http://twitter.com/statuses/retweet
        /// <summary>
        /// Retweet status
        /// </summary>
        /// <param name="provider">credential provider</param>
        /// <param name="id">status id</param>
        public static TwitterStatus Retweet(this CredentialProvider provider, long id)
        {
            return provider.GetStatus("statuses/retweet/" + id + ".json", CredentialProvider.RequestMethod.POST, CreateParamList());
        }

    }
}
