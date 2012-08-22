using Acuerdo.External.Uploader;

namespace Casket.Resolvers
{
    public class Photozou : IResolver
    {
        public bool IsResolvable(string url)
        {
            return url.StartsWith("http://photozou.jp/");
        }

        public string Resolve(string url)
        {
            if (IsResolvable(url))
                return "http://photozou.jp/bin/photo/" + url.Split('/')[6] + "/org.bin";
            else
                return null;
        }
    }
}
