using System.Collections.Generic;
using System.Security.Cryptography;

namespace System.Linq
{
    public static partial class EnumerableEx
    {
        public static IEnumerable<T> Unfold<T>(this Func<T, T> func, T initial)
        {
            var value = initial;
            yield return value;
            while (true)
            {
                value = func(value);
                yield return value;
            }
        }

        public static IOrderedEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source)
        {
            return Shuffle(source, RandomNumberGenerator.Create());
        }

        public static IOrderedEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source, RandomNumberGenerator rng)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var bytes = new byte[4];

            return source.OrderBy(delegate(TSource e)
            {
                rng.GetBytes(bytes);

                return BitConverter.ToInt32(bytes, 0);
            });
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, T original, T replace)
        {
            foreach (var t in source)
            {
                if (t.Equals(original))
                    yield return replace;
                else
                    yield return t;
            }
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> source, Func<T, bool> replPredicate, T replace)
        {
            foreach (var t in source)
            {
                if (replPredicate(t))
                    yield return replace;
                else
                    yield return t;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> execute)
        {
            foreach (var i in source)
                execute(i);
        }
    }
}
