﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ObservableImmutable
{
#   pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#   pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#   pragma warning disable CS8604 // Possible null reference argument.
#   pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
    public class ObservableImmutableList<T> : ObservableCollectionObject, IList, ICollection, IEnumerable, IList<T>, IImmutableList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region Private

        private readonly object _syncRoot;
        private ImmutableList<T> _items;

        #endregion Private

        #region Constructors

        public ObservableImmutableList() : this(Array.Empty<T>(), LockTypeEnum.SpinWait)
        {
        }

        public ObservableImmutableList(IEnumerable<T> items) : this(items, LockTypeEnum.SpinWait)
        {
        }

        public ObservableImmutableList(LockTypeEnum lockType) : this(Array.Empty<T>(), lockType)
        {
        }

        public ObservableImmutableList(IEnumerable<T> items, LockTypeEnum lockType) : base(lockType)
        {
            _syncRoot = new object();
            _items = ImmutableList<T>.Empty.AddRange(items);
        }

        #endregion Constructors

        #region Thread-Safe Methods

        #region General

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryOperation(Func<ImmutableList<T>, ImmutableList<T>> operation) => TryOperation(operation, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DoOperation(Func<ImmutableList<T>, ImmutableList<T>> operation) => DoOperation(operation, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        #region Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryOperation(Func<ImmutableList<T>, ImmutableList<T>> operation, NotifyCollectionChangedEventArgs args)
        {
            try
            {
                if (TryLock())
                {
                    ImmutableList<T>? oldList = _items;
                    ImmutableList<T>? newItems = operation(oldList);

                    if (newItems == null)
                    {
                        // user returned null which means he cancelled operation
                        return false;
                    }

                    _items = newItems;

                    if (args != null)
                        RaiseNotifyCollectionChanged(args);
                    return true;
                }
            }
            finally
            {
                Unlock();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryOperation(Func<ImmutableList<T>, KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>> operation)
        {
            try
            {
                if (TryLock())
                {
                    ImmutableList<T>? oldList = _items;
                    KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs> kvp = operation(oldList);
                    ImmutableList<T>? newItems = kvp.Key;
                    NotifyCollectionChangedEventArgs? args = kvp.Value;

                    if (newItems == null)
                    {
                        // user returned null which means he cancelled operation
                        return false;
                    }

                    _items = newItems;

                    if (args != null)
                        RaiseNotifyCollectionChanged(args);
                    return true;
                }
            }
            finally
            {
                Unlock();
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoOperation(Func<ImmutableList<T>, ImmutableList<T>> operation, NotifyCollectionChangedEventArgs args)
        {
            bool result;

            try
            {
                Lock();
                ImmutableList<T>? oldItems = _items;
                ImmutableList<T>? newItems = operation(_items);

                if (newItems == null)
                {
                    // user returned null which means he cancelled operation
                    return false;
                }

                result = (_items = newItems) != oldItems;

                if (args != null)
                    RaiseNotifyCollectionChanged(args);
            }
            finally
            {
                Unlock();
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DoOperation(Func<ImmutableList<T>, KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>> operation)
        {
            bool result;

            try
            {
                Lock();
                ImmutableList<T>? oldItems = _items;
                KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs> kvp = operation(_items);
                ImmutableList<T>? newItems = kvp.Key;
                NotifyCollectionChangedEventArgs? args = kvp.Value;

                if (newItems == null)
                {
                    // user returned null which means he cancelled operation
                    return false;
                }

                result = (_items = newItems) != oldItems;

                if (args != null)
                    RaiseNotifyCollectionChanged(args);
            }
            finally
            {
                Unlock();
            }

            return result;
        }

        #endregion Helpers

        #endregion General

        #region Specific

        public bool DoInsert(Func<ImmutableList<T>, KeyValuePair<int, T>> valueProvider) => DoOperation
                (
                currentItems =>
                    {
                        KeyValuePair<int, T> kvp = valueProvider(currentItems);
                        ImmutableList<T>? newItems = currentItems.Insert(kvp.Key, kvp.Value);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kvp.Value, kvp.Key));
                    }
                );

        public bool DoAdd(Func<ImmutableList<T>, T> valueProvider) => DoOperation
                (
                currentItems =>
                    {
                        T value;
                        ImmutableList<T>? newItems = _items.Add(value = valueProvider(currentItems));
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, currentItems.Count));
                    }
                );

        public bool DoAddRange(Func<ImmutableList<T>, IEnumerable<T>> valueProvider) => DoOperation
                (
                currentItems =>
                    currentItems.AddRange(valueProvider(currentItems))
                );

        public bool DoRemove(Func<ImmutableList<T>, T> valueProvider) => DoRemoveAt
                (
                currentItems =>
                    currentItems.IndexOf(valueProvider(currentItems))
                );

        public bool DoRemoveAt(Func<ImmutableList<T>, int> valueProvider) => DoOperation
                (
                currentItems =>
                    {
                        int index = valueProvider(currentItems);
                        T? value = currentItems[index];
                        ImmutableList<T>? newItems = currentItems.RemoveAt(index);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));
                    }
                );

        public bool DoSetItem(Func<ImmutableList<T>, KeyValuePair<int, T>> valueProvider) => DoOperation
                (
                currentItems =>
                    {
                        KeyValuePair<int, T> kvp = valueProvider(currentItems);
                        T? newValue = kvp.Value;
                        int index = kvp.Key;
                        T? oldValue = currentItems[index];
                        ImmutableList<T>? newItems = currentItems.SetItem(kvp.Key, newValue);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldValue, newValue, index));
                    }
                );

        public bool TryInsert(Func<ImmutableList<T>, KeyValuePair<int, T>> valueProvider) => TryOperation
                (
                currentItems =>
                    {
                        KeyValuePair<int, T> kvp = valueProvider(currentItems);
                        ImmutableList<T>? newItems = currentItems.Insert(kvp.Key, kvp.Value);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, kvp.Value, kvp.Key));
                    }
                );

        public bool TryAdd(Func<ImmutableList<T>, T> valueProvider) => TryOperation
                (
                currentItems =>
                    {
                        T value;
                        ImmutableList<T>? newItems = _items.Add(value = valueProvider(currentItems));
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, currentItems.Count));
                    }
                );

        public bool TryAddRange(Func<ImmutableList<T>, IEnumerable<T>> valueProvider) => TryOperation
                (
                currentItems =>
                    currentItems.AddRange(valueProvider(currentItems))
                );

        public bool TryRemove(Func<ImmutableList<T>, T> valueProvider) => TryRemoveAt
                (
                currentItems =>
                    currentItems.IndexOf(valueProvider(currentItems))
                );

        public bool TryRemoveAt(Func<ImmutableList<T>, int> valueProvider) => TryOperation
                (
                currentItems =>
                    {
                        int index = valueProvider(currentItems);
                        T? value = currentItems[index];
                        ImmutableList<T>? newItems = currentItems.RemoveAt(index);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));
                    }
                );

        public bool TrySetItem(Func<ImmutableList<T>, KeyValuePair<int, T>> valueProvider) => TryOperation
                (
                currentItems =>
                    {
                        KeyValuePair<int, T> kvp = valueProvider(currentItems);
                        T? newValue = kvp.Value;
                        int index = kvp.Key;
                        T? oldValue = currentItems[index];
                        ImmutableList<T>? newItems = currentItems.SetItem(kvp.Key, newValue);
                        return new KeyValuePair<ImmutableList<T>, NotifyCollectionChangedEventArgs>(newItems, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldValue, newValue, index));
                    }
                );

        #endregion Specific

        public ImmutableList<T> ToImmutableList() => _items;

        #region IEnumerable<T>

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable<T>

        #endregion Thread-Safe Methods

        #region Non Thead-Safe Methods

        #region IList

        /// <inheritdoc/>
        public int Add(object? value)
        {
            var val = (T)value;
            Add(val);
            return IndexOf(val);
        }

        /// <inheritdoc/>
        public bool Contains(object value) => Contains((T)value);

        /// <inheritdoc/>
        public int IndexOf(object value) => IndexOf((T)value);

        /// <inheritdoc/>
        public void Insert(int index, object value) => Insert(index, (T)value);

        /// <inheritdoc/>
        public bool IsFixedSize => false;

        /// <inheritdoc/>
        public void Remove(object value) => Remove((T)value);

        /// <inheritdoc/>
        void IList.RemoveAt(int index) => RemoveAt(index);

        /// <inheritdoc/>
        object IList.this[int index]
        {
#           pragma warning disable CS8603 // Possible null reference return.
            get => this[index];
#           pragma warning restore CS8603 // Possible null reference return.
#           pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
            set => SetItem(index, (T)value);
#           pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index) => _items.ToArray().CopyTo(array, index);

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => _syncRoot;

        #endregion IList

        #region IList<T>

        /// <inheritdoc/>
        public int IndexOf(T item) => _items.IndexOf(item);

        /// <inheritdoc/>
        void IList<T>.Insert(int index, T item) => Insert(index, item);

        /// <inheritdoc/>
        void IList<T>.RemoveAt(int index) => RemoveAt(index);

        /// <inheritdoc/>
        public T this[int index]
        {
            get => _items[index];
            set => SetItem(index, value);
        }

        /// <inheritdoc/>
        void ICollection<T>.Add(T item) => Add(item);

        /// <inheritdoc/>
        void IList.Clear() => Clear();

        /// <inheritdoc/>
        void ICollection<T>.Clear() => Clear();

        /// <inheritdoc/>
        public bool Contains(T item) => _items.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public int Count => _items.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            if (!_items.Contains(item))
                return false;

            _items = _items.Remove(item);
            RaiseNotifyCollectionChanged();
            return true;
        }

        #endregion IList<T>

        #region IImmutableList<T>

        /// <inheritdoc/>
        public IImmutableList<T> Add(T value)
        {
            int index = _items.Count;
            _items = _items.Add(value);
            RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> AddRange(IEnumerable<T> items)
        {
            _items = _items.AddRange(items);
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> Clear()
        {
            _items = _items.Clear();
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) => _items.IndexOf(item, index, count, equalityComparer);

        /// <inheritdoc/>
        public IImmutableList<T> Insert(int index, T element)
        {
            _items = _items.Insert(index, element);
            RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, element, index));
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> InsertRange(int index, IEnumerable<T> items)
        {
            _items = _items.InsertRange(index, items);
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer) => _items.LastIndexOf(item, index, count, equalityComparer);

        /// <inheritdoc/>
        public IImmutableList<T> Remove(T value, IEqualityComparer<T> equalityComparer)
        {
            int index = _items.IndexOf(value, equalityComparer);
            RemoveAt(index);
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> RemoveAll(Predicate<T> match)
        {
            _items = _items.RemoveAll(match);
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> RemoveAt(int index)
        {
            T? value = _items[index];
            _items = _items.RemoveAt(index);
            RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value, index));
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> RemoveRange(int index, int count)
        {
            _items = _items.RemoveRange(index, count);
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            _items = _items.RemoveRange(items, equalityComparer);
            RaiseNotifyCollectionChanged();
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
        {
            int index = _items.IndexOf(oldValue, equalityComparer);
            SetItem(index, newValue);
            return this;
        }

        /// <inheritdoc/>
        public IImmutableList<T> SetItem(int index, T value)
        {
            T? oldItem = _items[index];
            _items = _items.SetItem(index, value);
            RaiseNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, value, index));
            return this;
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < _items.Count; ++i)
                action(_items[i]);
        }

        #endregion IImmutableList<T>

        #endregion Non Thead-Safe Methods
    }
#   pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
#   pragma warning restore CS8604 // Possible null reference argument.
#   pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#   pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
