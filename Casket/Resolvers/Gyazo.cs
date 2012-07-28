using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Gyazo : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://gyazo.com/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return url + ".png";
            else
                return null;
        }
    }
}
