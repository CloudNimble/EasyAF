using System.Collections.Specialized;

namespace System.Collections.ObjectModel
{

    /// <summary>
    /// Specifies how bulk-change notifications are raised on an <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public enum CollectionChangeNotificationMode
    {

        /// <summary>
        /// Raises a single event with the proper action (Add, Remove, or Replace)
        /// and populates <c>NewItems</c>/<c>OldItems</c> with all affected items.
        /// </summary>
        /// <remarks>
        /// This is the default. Note: WPF's <c>ListCollectionView</c> does not support
        /// multi-item <c>NewItems</c>/<c>OldItems</c> and will throw. Use <see cref="Reset"/>
        /// for WPF data-binding scenarios.
        /// </remarks>
        Batched,

        /// <summary>
        /// Raises a single <see cref="NotifyCollectionChangedAction.Reset"/> event.
        /// Compatible with all UI frameworks including WPF.
        /// </summary>
        Reset

    }

}
