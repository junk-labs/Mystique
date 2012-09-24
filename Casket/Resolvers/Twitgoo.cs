using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Twitgoo : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://twitgoo.com/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return "http://twitgoo.com/show/img/" + url.Substring(19);
            else
                return null;
        }
    }
}
