using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Droplr : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://d.pr/i/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return url.EndsWith("+") ? url : url + "+";
            else
                return null;
        }
    }
}
