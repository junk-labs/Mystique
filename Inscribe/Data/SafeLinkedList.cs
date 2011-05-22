using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Inscribe.Data
{
    public class SafeLinkedList<T> : ReaderWriterLockBase, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private LinkedList<T> internalList;

        public SafeLinkedList()
        {
            internalList = new LinkedList<T>();
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
            CopyTo(array, index);
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
