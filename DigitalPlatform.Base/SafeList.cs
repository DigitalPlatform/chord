using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DigitalPlatform.Common
{
    public class SafeList<T> : IList<T>
    {
        internal ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        List<T> _list = new List<T>();

        public T Find(Predicate<T> match)
        {
            _lock.EnterReadLock();
            try
            {
                return _list.Find(match);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return ((IList<T>)_list)[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    ((IList<T>)_list)[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public int Count
        {
            get
            {
                    return ((IList<T>)_list).Count;
                /*
                _lock.EnterReadLock();
                try
                {
                }
                finally
                {
                    _lock.ExitReadLock();
                }
                */
            }
        }

        public bool IsReadOnly => ((IList<T>)_list).IsReadOnly;

        public void Add(T item)
        {
            _lock.EnterWriteLock();
            try
            {

                ((IList<T>)_list).Add(item);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {

                ((IList<T>)_list).Clear();

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return ((IList<T>)_list).Contains(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lock.EnterReadLock();
            try
            {
                ((IList<T>)_list).CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // return ((IList<T>)_list).GetEnumerator();

            // 适合集合内元素不太多的情况
            List<T> temp = new List<T>(this);

            foreach (T o in temp)
            {
                yield return o;
            }
        }

        public int IndexOf(T item)
        {
            _lock.EnterReadLock();
            try
            {
                return ((IList<T>)_list).IndexOf(item);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Insert(int index, T item)
        {
            _lock.EnterWriteLock();
            try
            {
                ((IList<T>)_list).Insert(index, item);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T item)
        {
            _lock.EnterWriteLock();
            try
            {

                return ((IList<T>)_list).Remove(item);

            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            _lock.EnterWriteLock();
            try
            {
                ((IList<T>)_list).RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // return ((IList<T>)_list).GetEnumerator();

            // 适合集合内元素不太多的情况
            List<T> temp = new List<T>(this);

            foreach (T o in temp)
            {
                yield return o;
            }
        }
    }
}
