
namespace System.Collections.Concurrent
{
    public static class ConcurrentDictionaryExtension
    {
        /// <summary>
        /// キーがまだ存在しない場合は ConcurrentDictionary&lt;TKey, TValue&gt; にキーと値のペアを追加し、
        /// キーが既に存在する場合は ConcurrentDictionary&lt;TKey, TValue&gt; のキーと値のペアを更新します。
        /// </summary>
        /// <typeparam name="TKey">ディクショナリ内のキーの型。</typeparam>
        /// <typeparam name="TValue">ディクショナリ内の値の型。</typeparam>
        /// <param name="dict">設定先ディクショナリ</param>
        /// <param name="key">追加する、または値を更新するキー</param>
        /// <param name="value">設定する新しい値></param>
        public static void AddOrUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict,
            TKey key, TValue value)
        {
            dict.AddOrUpdate(key, value, (k, v) => value);
        }

        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key)
        {
            TValue removed;
            return dict.TryRemove(key, out removed);
        }
    }
}
