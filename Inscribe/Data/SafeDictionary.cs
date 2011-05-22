using System;
using System.Collections.Generic;
using System.Linq;

namespace Inscribe.Data
{
    public class SafeDictionary<TKey, TValue> : ReaderWriterLockBase, IDictionary<TKey, TValue>, ILockOperatable
    {
        private Dictionary<TKey, TValue> internalDictionary;

        public SafeDictionary()
        {
            internalDictionary = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            using(WriterLock())
            {
                internalDictionary.Add(key, value);
            }
        }

        public void AddUnsafe(TKey key, TValue value)
        {
            internalDictionary.Add(key, value);
        }

        public bool AddOrUpdate(TKey key, TValue value)
        {
            using (WriterLock())
            {
                if (internalDictionary.ContainsKey(key))
                {
                    internalDictionary[key] = value;
                    return false;
                }
                else
                {
                    internalDictionary.Add(key, value);
                    return true;
                }
            }
        }

        public KeyValuePair<TKey, TValue>[] ToArray()
        {
            using (ReaderLock())
            {
                return internalDictionary.ToArray();
            }
        }

        public TKey[] KeyToArray()
        {
            using (ReaderLock())
            {
                return internalDictionary.Keys.ToArray();
            }
        }

        public TValue[] ValueToArray()
        {
            using (ReaderLock())
            {
                return internalDictionary.Values.ToArray();
            }
        }

        internal IEnumerable<TValue> ValueToArray(Func<TValue, bool> predicate)
        {
            using (ReaderLock())
            {
                return internalDictionary.Select((p) => p.Value).Where(predicate).ToArray();
            }
        }

        internal IEnumerable<TValue> ValueToArrayParallel(Func<TValue, bool> predicate)
        {
            using (ReaderLock())
            {
                return internalDictionary.AsParallel()
                    .Select(p => p.Value).Where(predicate).ToArray();
            }
        }

        public IEnumerable<TKey> KeyToList()
        {
            using (ReaderLock())
            {
                return internalDictionary.Keys.ToList();
            }
        }

        public IEnumerable<TValue> ValueToList()
        {
            using (ReaderLock())
            {
                return internalDictionary.Values.ToList();
            }
        }

        public IEnumerable<TValue> ValueToList(Func<TValue, bool> predicate)
        {
            using (ReaderLock())
            {
                return internalDictionary.Select((p)=>p.Value).Where(predicate).ToList();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return internalDictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            using(WriterLock())
            {
                return internalDictionary.Remove(key);
            }
        }

        public bool RemoveUnsafe(TKey key)
        {
            return internalDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return internalDictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return internalDictionary[key];
            }
            set
            {
                internalDictionary[key] = value;
            }
        }

        #region ICollection<KeyValuePair<TKey,TValue>> メンバー

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            using(WriterLock())
            {
                internalDictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return internalDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (ReaderLock())
            {
                internalDictionary.ToArray().CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            using(WriterLock())
            {
                return internalDictionary.Remove(item.Key);
            }
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> メンバー

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable メンバー

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region ILockOperatable メンバー

        public void LockOperate(Delegate operation, params object[] arguments)
        {
            using (WriterLock())
            {
                operation.DynamicInvoke(arguments);
            }
        }

        public T LockOperate<T>(Delegate operation, params object[] arguments)
        {
            using (WriterLock())
            {
                return (T)operation.DynamicInvoke(arguments);
            }
        }

        #endregion

    }
}