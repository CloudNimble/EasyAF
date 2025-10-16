using System.Collections.Generic;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// Provides an equality comparer for objects that implement <see cref="IIdentifiable{T}"/>.
    /// Compares objects based on their Id property values for equality and hash code generation.
    /// </summary>
    /// <typeparam name="T">The type of the identifier used by the identifiable objects.</typeparam>
    public class IIdentifiableEqualityComparer<T> : IEqualityComparer<IIdentifiable<T>> where T : struct
    {

        /// <summary>
        /// Determines whether the specified <see cref="IIdentifiable{T}"/> objects are equal by comparing their Id properties.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if the objects are equal (including both being null), false otherwise.</returns>
        public bool Equals(IIdentifiable<T> x, IIdentifiable<T> y)
        {
            if (x is null)
            {
                return y is null;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

#pragma warning disable CA1062 // Validate arguments of public methods
            return x.Id.ToString() == y.Id.ToString();
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="IIdentifiable{T}"/> object based on its Id property.
        /// </summary>
        /// <param name="obj">The object for which to get a hash code.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when obj is null.</exception>
        public int GetHashCode(IIdentifiable<T> obj)
        {
            Ensure.ArgumentNotNull(obj, nameof(obj));

            var x = 31;
            x = x * 17 + obj.Id.GetHashCode();
            return x;
        }

    }

}
