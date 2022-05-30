﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VolumeControl.WPF.Collections
{
    /// <inheritdoc cref="ObservableCollection{T}"/>
    public class ObservableList<T> : ObservableCollection<T>
    {
        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
            base.OnPropertyChanged(new("Count"));
            base.OnPropertyChanged(new("Items[]"));
        }
        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        public void AddRange(IEnumerable<T> range)
        {
            foreach (T item in range) Add(item);
        }
    }
}
