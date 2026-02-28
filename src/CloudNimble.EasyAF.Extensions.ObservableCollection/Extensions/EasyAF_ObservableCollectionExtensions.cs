using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Collections.ObjectModel
{

    /// <summary>
    /// Adds bulk-operation extension members to <see cref="ObservableCollection{T}"/> using C# 14 extension syntax.
    /// Each method manipulates the inner <see cref="Collection{T}.Items"/> list directly and raises a single
    /// change notification instead of one per item.
    /// </summary>
    public static class EasyAF_ObservableCollectionExtensions
    {

        extension<T>(ObservableCollection<T> collection)
        {

            /// <summary>
            /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
            /// </summary>
            /// <param name="items">The items to add. The collection itself cannot be <see langword="null"/>, but it can contain elements that are <see langword="null"/> if <typeparamref name="T"/> is a reference type.</param>
            /// <param name="mode">Specifies how the change notification is raised. Defaults to <see cref="CollectionChangeNotificationMode.Batched"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
            public void AddRange(IEnumerable<T> items, CollectionChangeNotificationMode mode = CollectionChangeNotificationMode.Batched)
            {
                collection.InsertRange(collection.Count, items, mode);
            }

            /// <summary>
            /// Inserts the elements of the specified collection at the specified index in the <see cref="ObservableCollection{T}"/>.
            /// </summary>
            /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
            /// <param name="items">The items to insert. The collection itself cannot be <see langword="null"/>, but it can contain elements that are <see langword="null"/> if <typeparamref name="T"/> is a reference type.</param>
            /// <param name="mode">Specifies how the change notification is raised. Defaults to <see cref="CollectionChangeNotificationMode.Batched"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Collection{T}.Count"/>.</exception>
            public void InsertRange(int index, IEnumerable<T> items, CollectionChangeNotificationMode mode = CollectionChangeNotificationMode.Batched)
            {
                ArgumentNullException.ThrowIfNull(items);

                if (index < 0 || index > collection.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var itemsList = items is IList<T> list ? list : new List<T>(items);
                if (itemsList.Count == 0)
                {
                    return;
                }

                var innerList = CollectionAccessor<T>.GetItems(collection);
                for (var i = 0; i < itemsList.Count; i++)
                {
                    innerList.Insert(index + i, itemsList[i]);
                }

                RaiseChangeNotification(
                    collection,
                    mode,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)itemsList, index),
                    countChanged: true);
            }

            /// <summary>
            /// Removes a range of elements from the <see cref="ObservableCollection{T}"/>.
            /// </summary>
            /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
            /// <param name="count">The number of elements to remove.</param>
            /// <param name="mode">Specifies how the change notification is raised. Defaults to <see cref="CollectionChangeNotificationMode.Batched"/>.</param>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0, or <paramref name="count"/> is less than 0.</exception>
            /// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the collection.</exception>
            public void RemoveRange(int index, int count, CollectionChangeNotificationMode mode = CollectionChangeNotificationMode.Batched)
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
                if (index + count > collection.Count)
                {
                    throw new ArgumentException("The specified index and count do not denote a valid range of elements in the collection.");
                }

                if (count == 0)
                {
                    return;
                }

                var innerList = CollectionAccessor<T>.GetItems(collection);
                var removedItems = new T[count];
                for (var i = 0; i < count; i++)
                {
                    removedItems[i] = innerList[index];
                    innerList.RemoveAt(index);
                }

                RaiseChangeNotification(
                    collection,
                    mode,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)removedItems, index),
                    countChanged: true);
            }

            /// <summary>
            /// Replaces a range of elements in the <see cref="ObservableCollection{T}"/> with the elements from the specified collection.
            /// </summary>
            /// <param name="index">The zero-based starting index of the range of elements to replace.</param>
            /// <param name="count">The number of elements to remove before inserting.</param>
            /// <param name="items">The items to insert in place of the removed elements. The collection itself cannot be <see langword="null"/>.</param>
            /// <param name="mode">Specifies how the change notification is raised. Defaults to <see cref="CollectionChangeNotificationMode.Batched"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0, or <paramref name="count"/> is less than 0.</exception>
            /// <exception cref="ArgumentException"><paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the collection.</exception>
            public void ReplaceRange(int index, int count, IEnumerable<T> items, CollectionChangeNotificationMode mode = CollectionChangeNotificationMode.Batched)
            {
                ArgumentNullException.ThrowIfNull(items);

                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
                if (index + count > collection.Count)
                {
                    throw new ArgumentException("The specified index and count do not denote a valid range of elements in the collection.");
                }

                var newItems = items is IList<T> list ? list : new List<T>(items);

                var innerList = CollectionAccessor<T>.GetItems(collection);
                var oldItems = new T[count];
                for (var i = 0; i < count; i++)
                {
                    oldItems[i] = innerList[index];
                    innerList.RemoveAt(index);
                }

                for (var i = 0; i < newItems.Count; i++)
                {
                    innerList.Insert(index + i, newItems[i]);
                }

                var countChanged = newItems.Count != count;

                RaiseChangeNotification(
                    collection,
                    mode,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList)newItems, (IList)oldItems, index),
                    countChanged: countChanged);
            }

        }

        private static void RaiseChangeNotification<T>(
            ObservableCollection<T> collection,
            CollectionChangeNotificationMode mode,
            NotifyCollectionChangedEventArgs batchedArgs,
            bool countChanged)
        {
            if (mode == CollectionChangeNotificationMode.Reset)
            {
                ObservableCollectionAccessor<T>.OnCollectionChanged(collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                ObservableCollectionAccessor<T>.OnCollectionChanged(collection, batchedArgs);
            }

            if (countChanged)
            {
                ObservableCollectionAccessor<T>.OnPropertyChanged(collection, new PropertyChangedEventArgs("Count"));
            }
            ObservableCollectionAccessor<T>.OnPropertyChanged(collection, new PropertyChangedEventArgs("Item[]"));
        }

        /// <summary>
        /// Provides zero-overhead access to the protected <see cref="Collection{T}.Items"/> property
        /// via <see cref="UnsafeAccessorAttribute"/>. The generic parameter <typeparamref name="T"/>
        /// is at the class level (ELEMENT_TYPE_VAR) as required by .NET 9+ for open-generic member lookup.
        /// </summary>
        private static class CollectionAccessor<T>
        {
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Items")]
            public static extern IList<T> GetItems(Collection<T> collection);
        }

        /// <summary>
        /// Provides zero-overhead access to the protected <c>OnCollectionChanged</c> and
        /// <c>OnPropertyChanged</c> methods on <see cref="ObservableCollection{T}"/>
        /// via <see cref="UnsafeAccessorAttribute"/>.
        /// </summary>
        private static class ObservableCollectionAccessor<T>
        {
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnCollectionChanged")]
            public static extern void OnCollectionChanged(
                ObservableCollection<T> collection, NotifyCollectionChangedEventArgs e);

            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnPropertyChanged")]
            public static extern void OnPropertyChanged(
                ObservableCollection<T> collection, PropertyChangedEventArgs e);
        }

    }

}
