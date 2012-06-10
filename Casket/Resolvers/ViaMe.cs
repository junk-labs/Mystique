using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class ViaMe : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://via.me/");
        }

        public string Resolve(string url)
        {
            return url + "/thumb/r600x600";
        }
    }
}
