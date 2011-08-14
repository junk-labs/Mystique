using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Pckles : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://pckles.com/");
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
