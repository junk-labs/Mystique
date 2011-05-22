using System;
using System.Collections.Generic;

namespace Inscribe.Data
{
    /// <summary>
    /// ストレージのベース実装
    /// </summary>
    public class SafeList<T> : ReaderWriterLockBase, IList<T>, ILockOperatable
    {
        /// <summary>
        /// データ保持リスト
        /// </summary>
        private List<T> internalList;

        /// <summary>
        /// マルチスレッドアクセス耐性を持ったリストを生成します。
        /// </summary>
        public SafeList()
        {
            internalList = new List<T>();
        }

        /// <summary>
        /// 保持しているアイテムの個数
        /// </summary>
        public int Count
        {
            get
            {
                using (ReaderLock())
                    return internalList.Count;
            }
        }

        public T this[int index]
        {
            get
            {
                using (ReaderLock())
                    return internalList[index];
            }
            set
            {
                using (WriterLock())
                    internalList[index] = value;
            }
        }

        /// <summary>
        /// アイテムを追加します。
        /// </summary>
        /// <param name="item">追加アイテム</param>
        public void Add(T item)
        {
            using (WriterLock())
                internalList.Add(item);
        }

        /// <summary>
        /// アイテム コレクションを追加します。<para />
        /// 多重列挙に対応したコレクションを指定する必要があります。
        /// </summary>
        /// <param name="item">追加アイテム コレクション</param>
        public void AddRange(IEnumerable<T> item)
        {
            using (WriterLock())
                internalList.AddRange(item);
        }

        /// <summary>
        /// アイテム コレクションを追加します。<para />
        /// 一度だけ列挙動作を行います。
        /// </summary>
        /// <remarks>
        /// 列挙操作中は内部ストレージがロックされるので、
        /// コストの大きい操作は登録しないようにしてください。
        /// </remarks>
        /// <param name="item">アイテムの列挙</param>
        public void AddRangeOnce(IEnumerable<T> item)
        {
            using (ReaderLock())
            {
                foreach (var i in item)
                    internalList.Add(i);
            }
        }

        /// <summary>
        /// シーケンスのうち、同じ要素が追加されていないものを追加します。
        /// </summary>
        public void AddRangeUnique(IEnumerable<T> item)
        {
            using (WriterLock())
            {
                foreach (var i in item)
                {
                    if (!this.Contains(i))
                        internalList.Add(i);
                }
            }
        }

        /// <summary>
        /// ストレージをロックせずにアイテムを追加します。<para />
        /// マルチスレッド操作に対してロックされません。通常はAddメソッドを使用してください。
        /// </summary>
        public void AddUnsafe(T item)
        {
            internalList.Add(item);
        }

        /// <summary>
        /// ストレージをロックせずにアイテムの列挙を追加します。<para />
        /// マルチスレッド操作に対してロックされません。通常はAddメソッドを使用してください。
        /// </summary>
        public void AddRangeUnsafe(IEnumerable<T> item)
        {
            internalList.AddRange(item);
        }

        /// <summary>
        /// アイテム コレクションを追加します。<para />
        /// 一度だけ列挙動作を行います。<para />
        /// マルチスレッド操作に対してロックされません。通常はAddOnceメソッドを使用してください。
        /// </summary>
        /// <param name="item">アイテムの列挙</param>
        public void AddRangeOnceUnsafe(IEnumerable<T> item)
        {
            foreach (var i in item)
                internalList.Add(i);
        }

        /// <summary>
        /// シーケンスのうち、同じ要素が追加されていないものを追加します。<para />
        /// マルチスレッド操作に対してロックされません。
        /// </summary>
        public void AddRangeUniqueUnsafe(IEnumerable<T> item)
        {
            foreach (var i in item)
            {
                if (!this.Contains(i))
                    internalList.Add(i);
            }
        }

        /// <summary>
        /// 同じ要素が追加されていない場合は、アイテムを追加します。
        /// </summary>
        public bool AddUnique(T item)
        {
            using (WriterLock())
            {
                if (!this.Contains(item))
                {
                    internalList.Add(item);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// すべての要素を削除します。
        /// </summary>
        public void Clear()
        {
            using (WriterLock())
                internalList.Clear();
        }

        /// <summary>
        /// 指定したアイテムを取り除きます。
        /// </summary>
        public bool Remove(T item)
        {
            using (WriterLock())
                return internalList.Remove(item);
        }

        /// <summary>
        /// 指定したインデックスに存在するアイテムを取り除きます。
        /// </summary>
        public void RemoveAt(int index)
        {
            using (WriterLock())
            {
                if (internalList.Count <= index)
                    throw new ArgumentOutOfRangeException("index");
                internalList.RemoveAt(index);
            }
        }

        /// <summary>
        /// 指定した述語条件に一致するすべての要素を取り除きます。
        /// </summary>
        public void RemoveAll(Predicate<T> match)
        {
            using (WriterLock())
                internalList.RemoveAll(match);
        }

        /// <summary>
        /// すべての要素を削除します。<para />
        /// マルチスレッド操作に対してロックされません。通常はClearメソッドを使用してください。
        /// </summary>
        public void ClearUnsafe()
        {
            internalList.Clear();
        }

        /// <summary>
        /// 指定したアイテムを取り除きます。<para />
        /// マルチスレッド操作に対してロックされません。通常はRemoveメソッドを使用してください。
        /// </summary>
        public bool RemoveUnsafe(T item)
        {
            return internalList.Remove(item);
        }

        /// <summary>
        /// 指定したインデックスに存在するアイテムを取り除きます。<para />
        /// マルチスレッド操作に対してロックされません。通常はRemoveAtメソッドを使用してください。
        /// </summary>
        public void RemoveAtUnsafe(int index)
        {
            if (internalList.Count <= index)
                throw new ArgumentOutOfRangeException("index");
            internalList.RemoveAt(index);
        }

        /// <summary>
        /// 指定した述語条件に一致するすべての要素を取り除きます。<para />
        /// マルチスレッド操作に対してロックされません。通常はRemoveAllメソッドを使用してください。
        /// </summary>
        public void RemoveAllUnsafe(Predicate<T> match)
        {
            internalList.RemoveAll(match);
        }

        /// <summary>
        /// 要素を配列化します。
        /// </summary>
        public T[] ToArray()
        {
            using (ReaderLock())
                return ToArrayUnsafe();
        }

        /// <summary>
        /// 要素を配列化します。<para />
        /// マルチスレッド操作に対してロックされません。通常はToArrayメソッドを使用してください。
        /// </summary>
        public T[] ToArrayUnsafe()
        {
            return internalList.ToArray();
        }

        public List<T> ToList()
        {
            using (WriterLock())
                return ToListUnsafe();
        }

        public List<T> ToListUnsafe()
        {
            return new List<T>(internalList);
        }

        /// <summary>
        /// 規定の比較子を利用してリスト内の要素を並べ替えます。
        /// </summary>
        public void Sort()
        {
            using (WriterLock())
                SortUnsafe();
        }

        /// <summary>
        /// 規定の比較子を利用してリスト内の要素を並べ替えます。<para />
        /// マルチスレッド操作に対してロックされません。通常はToArrayメソッドを使用してください。
        /// </summary>
        public void SortUnsafe()
        {
            internalList.Sort();
        }

        /// <summary>
        /// 指定したSystem.Comparison&lt;T&gt;を利用して、リスト内の要素を並べ替えます。
        /// </summary>
        /// <param name="comparison">要素の比較に使用するSystem.Comparison&lt;T&gt;</param>
        public void Sort(Comparison<T> comparison)
        {
            using (WriterLock())
                SortUnsafe(comparison);
        }

        /// <summary>
        /// 指定したSystem.Comparison&lt;T&gt;を利用して、リスト内の要素を並べ替えます。<para />
        /// マルチスレッド操作に対してロックされません。通常はToArrayメソッドを使用してください。
        /// </summary>
        /// <param name="comparison">要素の比較に使用するSystem.Comparison&lt;T&gt;</param>
        public void SortUnsafe(Comparison<T> comparison)
        {
            internalList.Sort(comparison);
        }

        /// <summary>
        /// 指定した比較子を利用して、リスト内の要素を並べ替えます。
        /// </summary>
        /// <param name="comparison">要素の比較に使用する比較子</param>
        public void Sort(IComparer<T> comparer)
        {
            using (WriterLock())
                SortUnsafe(comparer);
        }

        /// <summary>
        /// 指定した比較子を利用して、リスト内の要素を並べ替えます。<para />
        /// マルチスレッド操作に対してロックされません。通常はToArrayメソッドを使用してください。
        /// </summary>
        /// <param name="comparison">要素の比較に使用する比較子</param>
        public void SortUnsafe(IComparer<T> comparer)
        {
            internalList.Sort(comparer);
        }

        #region IEnumerable メンバー

        /// <summary>
        /// 格納しているアイテムを取得します。
        /// </summary>
        public IEnumerable<T> Items
        {
            get { return internalList; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ILockOperatable メンバー

        public void LockOperate(Delegate operation, params object[] arguments)
        {
            using (WriterLock())
                operation.DynamicInvoke(arguments);
        }

        public TResult LockOperate<TResult>(Delegate operation, params object[] arguments)
        {
            return (TResult)operation.DynamicInvoke(arguments);
        }

        #endregion

        #region IList<T> メンバー

        public int IndexOf(T item)
        {
            using (ReaderLock())
                return internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            using (WriterLock())
                internalList.Insert(index, item);
        }

        public void InsertUnsafe(int index, T item)
        {
            internalList.Insert(index, item);
        }

        #endregion

        #region ICollection<T> メンバー

        public bool Contains(T item)
        {
            using (ReaderLock())
                return internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            using (ReaderLock())
                internalList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 常に更新可能です。
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}