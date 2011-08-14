using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Imgly : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://img.ly/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return "http://img.ly/show/full/" + url.Substring(14);
            else
                return null;
        }
    }
}
