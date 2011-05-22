using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acuerdo.External.Shortener
{
    public interface IURLShortener
    {
        bool IsCompressed(string url);
        bool TryCompress(string url, out string compressed);
        string Name { get; }
    }

    public interface IURLExtractor
    {
        bool TryDecompress(string url, out string decompressed);
    }

    public abstract class URLShortenerBase : IURLShortener, IURLExtractor
    {
        public abstract bool IsCompressed(string url);
        public abstract bool TryCompress(string url, out string compressed);
        public abstract bool TryDecompress(string url, out string decompressed);
        public abstract string Name { get; }
    }
}
