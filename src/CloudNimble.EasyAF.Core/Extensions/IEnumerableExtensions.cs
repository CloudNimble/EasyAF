using CloudNimble.EasyAF.Core;
using System.Linq;

namespace System.Collections.Generic
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_IEnumerableExtensions
    {

        /// <summary>
        /// Loops through the entries in a given <see cref="IEnumerable{T}"/> and accepts all current changes for each entry.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="goDeep"></param>
        public static void AcceptChanges<T>(this IEnumerable<T> enumerable, bool goDeep = false) where T : DbObservableObject
        {
            Ensure.ArgumentNotNull(enumerable, nameof(enumerable));
            foreach (var obj in enumerable)
            {
                obj.AcceptChanges(goDeep);
            }
        }

        /// <summary>
        /// Returns a <see cref="int"/> representing the number of objects in the enumerable that have changes.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="checkGraph"></param>
        /// <returns></returns>
        public static int ChangedCount<T>(this IEnumerable<T> enumerable, bool checkGraph = false) where T : DbObservableObject
        {
            return enumerable.Count(c => checkGraph ? c.IsGraphChanged : c.IsChanged);
        }

        /// <summary>
        /// Returns a <see cref="bool" /> if a list of <see cref="IIdentifiable{T}.Id"/>s from the given <see cref="IEnumerable{T}"/> contains
        /// the specified value.
        /// </summary>
        /// <param name="list">The <see cref="List{IIdentifiable}"/> to check for the given ID value.</param>
        /// <param name="idValue">The value to check for.</param>
        public static bool ContainsId<T, TId>(this IEnumerable<T> list, TId idValue)
            where T : class, IIdentifiable<TId>
            where TId : struct
        {
            return list is not null && list.Select(c => c.Id).Contains(idValue);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> if any <see cref="DbObservableObject" /> in the <see cref="IEnumerable{T}"/> has changes.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="checkGraph"></param>
        /// <returns></returns>
        public static bool ContentsAreChanged<T>(this IEnumerable<T> enumerable, bool checkGraph = false) where T : DbObservableObject
        {
            return enumerable.Any(c => checkGraph ? c.IsGraphChanged : c.IsChanged);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> if any <see cref="DbObservableObject" /> in the <see cref="IEnumerable{T}"/> has changes.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="predicate"></param>
        /// <param name="checkGraph"></param>
        /// <returns></returns>
        public static bool ContentsAreChanged<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, bool checkGraph = false) where T : DbObservableObject
        {
            return enumerable.Where(predicate).ContentsAreChanged(checkGraph);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> if any <see cref="DbObservableObject" /> in the <see cref="IEnumerable{T}"/> has changes.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="foreignList">The list of related objects that we want to filter the <paramref name="enumerable"/> down to.</param>
        /// <param name="foreignIdFunc">
        /// The property from the <paramref name="enumerable"/> that points to the <see cref="IIdentifiable{T}.Id"/> for the objects in <paramref name="foreignList"/>.
        /// </param>
        /// <param name="checkGraph"></param>
        /// <returns></returns>
        public static bool ContentsAreChanged<T, TForeign, TId>(this IEnumerable<T> enumerable, IEnumerable<TForeign> foreignList, Func<T, TId> foreignIdFunc, bool checkGraph = false)
            where T : DbObservableObject, IIdentifiable<TId>
            where TForeign : DbObservableObject, IIdentifiable<TId>
            where TId : struct
        {
            return enumerable.Where(c => foreignList.ContainsId(foreignIdFunc.Invoke(c))).ContentsAreChanged(checkGraph);
        }

        /// <summary>
        /// For a given <see cref="IEnumerable{T}"/>, filter down the result to the changed items in <paramref name="enumerable"/> 
        /// whose foreign keys appear in the <paramref name="foreignList"/>.
        /// </summary>
        /// <param name="enumerable">The list we want to check for changes in.</param>
        /// <param name="foreignList">The list of related objects that we want to filter the <paramref name="enumerable"/> down to.</param>
        /// <param name="foreignIdFunc">
        /// The property from the <paramref name="enumerable"/> that points to the <see cref="IIdentifiable{T}.Id"/> for the objects in <paramref name="foreignList"/>.
        /// </param>
        public static IEnumerable<T> FilterForChanges<T, TForeign, TId>(this IEnumerable<T> enumerable, IEnumerable<TForeign> foreignList, Func<T, TId> foreignIdFunc)
            where T : DbObservableObject, IIdentifiable<TId>
            where TForeign : DbObservableObject, IIdentifiable<TId>
            where TId : struct
        {
            Ensure.ArgumentNotNull(enumerable, nameof(enumerable));
            Ensure.ArgumentNotNull(foreignList, nameof(foreignList));
            return enumerable.Where(c => foreignList.ContainsId(foreignIdFunc.Invoke(c)) && c.IsChanged);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> specifying whether or not the <see cref="IEnumerable{T}"/> has any items in it.
        /// </summary>
        /// <typeparam name="T">The type of the items inside the <see cref="IEnumerable"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to check.</param>
        /// <returns></returns>
        public static bool None<T>(this IEnumerable<T> source)
        {
            if (source is not null)
            {
                return !source.Any();
            }

            return true;
        }

        /// <summary>
        /// Returns a <see cref="bool" /> specifying whether or not the <see cref="IEnumerable{T}"/> has any items in it.
        /// </summary>
        /// <typeparam name="T">The type of the items inside the <see cref="IEnumerable"/>.</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to check.</param>
        /// <param name="predicate">A set of additional parameters to check against.</param>
        /// <returns></returns>
        public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source is not null)
            {
                return !source.Any(predicate);
            }

            return true;
        }

        /// <summary>
        /// Loops through the entries in a given <see cref="IEnumerable{T}" /> and clears all current changes for each entry.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="goDeep"></param>
        public static void RejectChanges<T>(this IEnumerable<T> enumerable, bool goDeep = false) where T : DbObservableObject
        {
            Ensure.ArgumentNotNull(enumerable, nameof(enumerable));
            foreach (var obj in enumerable)
            {
                obj.RejectChanges(goDeep);
            }
        }

        /// <summary>
        /// Returns a <see cref="List{T}" /> where the <see cref="DbObservableObject">DbObservableObjects</see> have <see cref="DbObservableObject.TrackChanges"/> turned on.
        /// </summary>
        /// <param name="enumerable">The list of objects to turn change tracking on for.</param>
        /// <param name="deepTracking"></param>
        /// <returns></returns>
        public static List<T> ToTrackedList<T>(this IEnumerable<T> enumerable, bool deepTracking = false) where T : DbObservableObject
        {
            Ensure.ArgumentNotNull(enumerable, nameof(enumerable));
            foreach (var obj in enumerable)
            {
                obj.TrackChanges(deepTracking);
            }
            return enumerable.ToList();
        }

    }

}
