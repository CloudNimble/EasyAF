// All credits for ObjectDataLoader (CloudNimble.EasyAF.Edmx.InMemoryDb.Extra): Chris Rodgers
// GitHub: https://github.com/christophano

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.DataLoaders.ObjectDataLoader
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a collection of object data entities.
    /// </summary>
    /// <typeparam name="T">The type of entity that this table stores.</typeparam>
    /// <seealso cref="IList{T}" />
    public class ObjectDataTable<T> : IList<T>
    {
        private readonly IList<T> list = new List<T>();
        private readonly IDictionary<Type, string> discriminators = new Dictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDataTable{T}"/> class.
        /// </summary>
        internal ObjectDataTable() { }

        /// <summary>
        /// Gets or sets the discriminator column name.
        /// </summary>
        /// <value>
        /// The discriminator column name.
        /// </value>
        public string DiscriminatorColumn { get; set; } = "Discriminator";

        /// <summary>
        /// Adds a discriminator value for the given type.
        /// </summary>
        /// <typeparam name="TType">The type of entity.</typeparam>
        /// <param name="discriminator">The discriminator value.</param>
        public void AddDiscriminator<TType>(string discriminator) where TType : T
        {
            if (!discriminators.ContainsKey(typeof(TType)))
            {
                discriminators.Add(typeof(TType), discriminator);
            }
            discriminators[typeof(TType)] = discriminator;
        }

        /// <summary>
        /// Gets the discriminator value for the given type.
        /// </summary>
        /// <returns>The discriminator value.</returns>
        internal string GetDiscriminator(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var type = item.GetType();
            string discriminator;
            if (!discriminators.TryGetValue(type, out discriminator))
            {
                discriminator = type.Name;
                discriminators.Add(type, discriminator);
            }
            return discriminator;
        }

        #region IList<T>

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly => list.IsReadOnly;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            list.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            list.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        #endregion
    }
}
