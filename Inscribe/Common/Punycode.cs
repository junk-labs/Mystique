using System.Linq;
using System.Text;

namespace System.Web
{
    /// <summary>
    /// ぷにぷにこーど♡
    /// </summary>
    /// <seealso cref="http://ja.wikipedia.org/wiki/Punycode" />
    public static class Punycode
    {
        const int Base = 36;
        const int TMin = 1;
        const int TMax = 26;
        const int Skew = 38;
        const int Damp = 700;
        const int InitN = 128; // 0x80, Non-ASCII char
        const int InitBias = 72;
        const char Delim = '-';

        public static Uri PunyEncode(this Uri uri)
        {
            string host = uri.Host.Split('.').Select(PunyEncodeSink).JoinString(".");
            var ancidx = uri.OriginalString.IndexOf("#");
            string anchor = String.Empty;
            if (ancidx >= 0)
                anchor = uri.OriginalString.Substring(ancidx);
            return new Uri(uri.Scheme + "://" + host + uri.PathAndQuery + anchor);
        }

        private static string PunyEncodeSink(string particle)
        {
            // 実装が謎い
            throw new NotImplementedException();
            /*
            int n = InitN;
            int delta = 0;
            int bias = InitBias;
            StringBuilder output = new StringBuilder();
            */
        }

        public static Uri PunyDecode(this Uri uri)
        {
            string host = uri.Host.Split('.').Select(PunyDecodeSink).JoinString(".");
            var ancidx = uri.OriginalString.IndexOf("#");
            string anchor = String.Empty;
            if (ancidx >= 0)
                anchor = uri.OriginalString.Substring(ancidx);
            return new Uri(uri.Scheme + "://" + host + uri.PathAndQuery + anchor);
        }

        private static string PunyDecodeSink(string particle)
        {
            if (particle == null)
                throw new ArgumentNullException("particle");
            if (!particle.StartsWith("xn--")) // Punycode must starts with "xn--"
                return particle;
            particle = particle.Substring(4);
            int i = 0; // counter
            int n = 0x80; // codepoint
            StringBuilder output = new StringBuilder();
            int sep = particle.LastIndexOf(Delim);
            if (sep > -1)
            {
                output.Append(particle.Substring(0, sep));
                particle = particle.Substring(sep + 1);
            }
            int prefidx = 0; // particle referencing index
            int bias = InitBias;
            while (prefidx < particle.Length)
            {
                int oldi = i;
                int w = 1;
                int k = Base;
                while (true)
                {
                    int digit = GetDigit(particle[prefidx++]);
                    if (digit >= Base)
                        throw new FormatException("Punycode bad input.");
                    i = checked(i + digit * w);
                    int t =
                        k <= bias ? TMin :
                        k >= bias + TMax ? TMax :
                        k - bias;
                    if (digit < t)
                        break;
                    w = checked(w * (Base - t));
                    k += Base;
                }
                bias = Adapt(i - oldi, output.Length + 1, oldi == 0);
                var cl = output.Length + 1;
                n = checked(n + i / cl);
                i %= cl;
                output.Insert(i, Convert.ToChar(n));
                i++;
            }
            return output.ToString();
        }

        private static int GetDigit(char c)
        {
            return
                c - 48 < 10 ? c - 22 :
                c - 65 < 26 ? c - 65 :
                c - 97 < 26 ? c - 97 :
                Base;
        }

        private static int Adapt(int delta, int numpoints, bool isFirst)
        {
            delta = isFirst ? delta / Damp : delta >> 1;
            delta += delta / numpoints;
            int k;
            for (k = 0; delta > ((Base - TMin) * TMax) / 2; k += Base)
                delta /= Base - TMin;
            return k + (Base - TMin + 1) * delta / (delta + Skew);
        }
    }
}
