using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Inscribe.Util
{
    public class SafeLinkedList<T> : ReaderWriterLockBase, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private LinkedList<T> internalList;

        public SafeLinkedList()
        {
            this.internalList = new LinkedList<T>();
        }

        void ICollection<T>.Add(T item)
        {
            AddLast(item);
        }

        public void AddFirst(T item)
        {
            using (WriterLock())
                internalList.AddLast(item);
        }

        public void AddLast(T item)
        {
            using (WriterLock())
                internalList.AddLast(item);
        }

        public void AddBefore(LinkedListNode<T> node, T item)
        {
            using (WriterLock())
                internalList.AddBefore(node, item);
        }

        public void AddAfter(LinkedListNode<T> node, T item)
        {
            using (WriterLock())
                internalList.AddAfter(node, item);
        }

        public void Clear()
        {
            using (WriterLock())
                internalList.Clear();
        }

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

        public int Count
        {
            get
            {
                using (ReaderLock())
                    return internalList.Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            using (WriterLock())
                return internalList.Remove(item);
        }

        public void ReplaceItem(Func<T, bool> predicate, T newItem)
        {
            using (ReaderLock())
            {
                var current = this.internalList.First;
                while (current != null)
                {
                    current.Value = predicate(current.Value) ? newItem : current.Value;
                    current = current.Next;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            using (ReaderLock())
                return internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            var castedArray = array as T[];
            if (array == null)
                throw new ArgumentException("array is uncastable to T[].");
            using (ReaderLock())
                internalList.CopyTo(castedArray, index);
        }

        /// <summary>
        /// コレクションを配列として返します。<para />
        /// スレッドセーフに操作されます。
        /// </summary>
        public T[] ToArray()
        {
            using (ReaderLock())
                return internalList.ToArray();
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }
    }
}
