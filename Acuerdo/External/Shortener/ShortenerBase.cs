using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acuerdo.External.Shortener
{
    [Obsolete("Url shortener infrastructure is no more supported in Krile.")]
    public interface IUrlShortener
    {
        bool IsCompressed(string url);
        bool TryCompress(string url, out string compressed);
        string Name { get; }
    }

    public interface IUriExtractor
    {
        bool TryDecompress(string url, out string decompressed);
    }

    [Obsolete("Url shortener infrastructure is no more supported in Krile.")]
    public abstract class UrlShortenerBase : IUrlShortener, IUriExtractor
    {
        public abstract bool IsCompressed(string url);
        public abstract bool TryCompress(string url, out string compressed);
        public abstract bool TryDecompress(string url, out string decompressed);
        public abstract string Name { get; }
    }
}
