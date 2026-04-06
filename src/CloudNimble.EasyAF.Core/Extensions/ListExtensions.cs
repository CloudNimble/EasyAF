using CloudNimble.EasyAF.Core;
using System.Linq;

namespace System.Collections.Generic
{

    /// <summary>
    /// 
    /// </summary>
    public static class EasyAF_ListExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="oldInstance"></param>
        /// <param name="newInstance"></param>
        /// <returns></returns>
        public static IList<T> ReplaceTracked<T>(this IList<T> list, T oldInstance, T newInstance) where T : DbObservableObject
        {
            Ensure.ArgumentNotNull(list, nameof(list));
            Ensure.ArgumentNotNull(oldInstance, nameof(oldInstance));
            // RWM: The new instance can indeed be null, so we're not going to check that one.

            newInstance.TrackChanges();
            list[list.IndexOf(oldInstance)] = newInstance;
            return list;
        }

    }

}
