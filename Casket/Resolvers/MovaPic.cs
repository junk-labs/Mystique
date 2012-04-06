using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class MovaPic : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://movapic.com/pic/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
            {
                return "http://image.movapic.com/pic/m_" + url.Substring(23) + ".jpeg";
            }
            else
            {
                return null;
            }
        }
    }
}
