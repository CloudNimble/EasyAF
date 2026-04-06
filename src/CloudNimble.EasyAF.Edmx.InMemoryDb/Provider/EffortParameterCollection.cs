// --------------------------------------------------------------------------------------------
// <copyright file="EffortParameterCollection.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Provider
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    /// <summary>
    ///     Represents a collection of <see cref="EffortParameter"/> associated with a 
    ///     <see cref="EffortCommand"/>.
    /// </summary>
    public sealed class EffortParameterCollection : DbParameterCollection
    {
        private object syncRoot;
        private List<EffortParameter> internalCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EffortParameterCollection" /> 
        ///     class.
        /// </summary>
        public EffortParameterCollection()
        {
            syncRoot = new object();
            internalCollection = new List<EffortParameter>();
        }

        /// <summary>
        ///     Adds a <see cref="T:EffortParameter" /> item with the specified value to the 
        ///     <see cref="T:EffortParameterCollection" />.
        /// </summary>
        /// <param name="value">
        ///     The <see cref="P:EffortParameter.Value" /> of the 
        ///     <see cref="T:EffortParameter" /> to add to the collection.
        /// </param>
        /// <returns>
        ///     The index of the <see cref="T:EffortParameter" /> object in the collection.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     The provided parameter object is incompatible
        /// </exception>
        public override int Add(object value)
        {
            var parameter = value as EffortParameter;

            if (parameter is null)
            {
                throw new ArgumentException("The provided parameter object is incompatible");
            }

            internalCollection.Add(parameter);
            return internalCollection.Count - 1;
        }

        /// <summary>
        ///     Adds an array of items with the specified values to the 
        /// <see cref="T:EffortParameterCollection" />.
        /// </summary>
        ///     <param name="values">An array of values of type 
        ///     <see cref="T:EffortParameter" /> to add to the collection.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     The provided parameter object is incompatible
        /// </exception>
        public override void AddRange(Array values)
        {
            var parameters = new List<EffortParameter>();

            foreach (var value in values)
            {
                var parameter = value as EffortParameter;

                if (parameter is null)
                {
                    throw new ArgumentException(
                        "The provided parameter object is incompatible");
                }

                parameters.Add(parameter);
            }

            internalCollection.AddRange(parameters);
        }

        /// <summary>
        ///     Removes all <see cref="T:EffortParameter" /> values from the
        ///     <see cref="T:EffortParameterCollection" />.
        /// </summary>
        public override void Clear()
        {
            internalCollection.Clear();
        }

        /// <summary>
        ///     Indicates whether a <see cref="T:EffortParameter" /> with the specified name
        ///     exists in the collection.
        /// </summary>
        /// <param name="value">
        ///     The name of the <see cref="T:EffortParameterr" /> to look for in the 
        ///     collection.
        /// </param>
        /// <returns>
        ///     true if the <see cref="T:EffortParameter" /> is in the collection; otherwise
        ///     false.
        /// </returns>
        public override bool Contains(string value)
        {
            return internalCollection.Any(p => p.ParameterName == value);
        }

        /// <summary>
        ///     Indicates whether a <see cref="T:EffortParameter" /> with the specified 
        ///     <see cref="P:EffortParameter.Value" /> is contained in the collection.
        /// </summary>
        /// <param name="value">
        ///     The <see cref="P:EffortParameter.Value" /> of the 
        ///     <see cref="T:EffortParameter" /> to look for in the collection.</param>
        /// <returns>
        ///     true if the <see cref="T:EffortParameter" /> is in the collection; otherwise
        ///     false.
        /// </returns>
        public override bool Contains(object value)
        {
            return internalCollection.Contains(value as EffortParameter);
        }

        /// <summary>
        ///     Copies an array of items to the collection starting at the specified index.
        /// </summary>
        /// <param name="array">
        ///     The array of items to copy to the collection.
        /// </param>
        /// <param name="index">
        ///     The index in the collection to copy the items.
        /// </param>
        public override void CopyTo(Array array, int index)
        {
            for (var i = 0; i < internalCollection.Count; i++)
            {
                array.SetValue(internalCollection[i], index + i);
            }
        }

        /// <summary>
        ///     Specifies the number of items in the collection.
        /// </summary>
        /// <returns>
        ///     The number of items in the collection.
        /// </returns>
        public override int Count
        {
            get
            {
                return internalCollection.Count;
            }
        }

        /// <summary>
        ///     Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator" />
        ///     method, which supports a simple iteration over a collection by a .NET Framework
        ///     data provider.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> that can be used to iterate
        ///     through the collection.
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return internalCollection.GetEnumerator();
        }

        /// <summary>
        ///     Returns <see cref="T:EffortParameter" /> the object with the specified name.
        /// </summary>
        /// <param name="parameterName">
        ///     The name of the <see cref="T:EffortParameter" /> in the collection.
        /// </param>
        /// <returns>
        ///     The <see cref="T:EffortParameter" /> the object with the specified name.
        /// </returns>
        protected override DbParameter GetParameter(string parameterName)
        {
            return internalCollection.FirstOrDefault(p => p.ParameterName == parameterName);
        }

        /// <summary>
        ///     Returns the <see cref="T:EffortParameterr" /> object at the specified index in 
        ///     the collection.
        /// </summary>
        /// <param name="index">
        ///     The index of the <see cref="T:EffortParameter" /> in the collection.
        /// </param>
        /// <returns>
        ///     The <see cref="T:EffortParameter" /> object at the specified index in the 
        ///     collection.
        /// </returns>
        protected override DbParameter GetParameter(int index)
        {
            return internalCollection[index];
        }

        /// <summary>
        ///     Returns the index of the <see cref="T:EffortParameter" /> object with the 
        ///     specified name.
        /// </summary>
        /// <param name="parameterName">
        ///     The name of the <see cref="T:EffortParameter" /> object in the collection.
        /// </param>
        /// <returns>
        ///     The index of the <see cref="T:EffortParameter" /> object with the specified 
        ///     name.
        /// </returns>
        public override int IndexOf(string parameterName)
        {
            for (var i = 0; i < internalCollection.Count; i++)
            {
                if (internalCollection[i].ParameterName == parameterName)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Returns the index of the specified <see cref="T:EffortParameter" /> object.
        /// </summary>
        /// <param name="value">
        ///     The <see cref="T:EffortParameter" /> object in the collection.
        /// </param>
        /// <returns>
        ///     The index of the specified <see cref="T:EffortParameter" /> object.
        /// </returns>
        public override int IndexOf(object value)
        {
            return internalCollection.IndexOf(value as EffortParameter);
        }

        /// <summary>
        ///     Inserts the specified index of the <see cref="T:EffortParameter" /> object with
        ///     the specified name into the collection at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The index at which to insert the <see cref="T:EffortParameter" /> object.
        /// </param>
        /// <param name="value">
        ///     The <see cref="T:EffortParameter" /> object to insert into the collection.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     The provided parameter object is incompatible
        /// </exception>
        public override void Insert(int index, object value)
        {
            var parameter = value as EffortParameter;

            if (parameter is null)
            {
                throw new ArgumentException("The provided parameter object is incompatible");
            }

            internalCollection.Insert(index, parameter);
        }

        /// <summary>
        ///     Specifies whether the collection is a fixed size.
        /// </summary>
        /// <returns>
        ///     true if the collection is a fixed size; otherwise false.
        /// </returns>
        public override bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        ///     Specifies whether the collection is read-only.
        /// </summary>
        /// <returns>
        ///     true if the collection is read-only; otherwise false.
        /// </returns>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Specifies whether the collection is synchronized.
        /// </summary>
        /// <returns>
        ///     true if the collection is synchronized; otherwise false.
        /// </returns>
        public override bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        ///     Removes the specified <see cref="T:EffortParameter" /> object from the 
        ///     collection.
        /// </summary>
        /// <param name="value">
        ///     The <see cref="T:EffortParameter" /> object to remove.
        /// </param>
        public override void Remove(object value)
        {
            internalCollection.Remove(value as EffortParameter);
        }

        /// <summary>
        ///     Removes the <see cref="T:EffortParameter" /> object with the specified name 
        ///     from the collection.
        /// </summary>
        /// <param name="parameterName">
        ///     The name of the <see cref="T:EffortParameter" /> object to remove.
        /// </param>
        public override void RemoveAt(string parameterName)
        {
            var parameter =
                internalCollection.FirstOrDefault(p => p.ParameterName == parameterName);

            internalCollection.Remove(parameter);
        }

        /// <summary>
        ///     Removes the <see cref="T:EffortParameter" /> object at the specified from the 
        ///     collection.
        /// </summary>
        /// <param name="index">
        ///     The index where the <see cref="T:EffortParameter" /> object is located.
        /// </param>
        public override void RemoveAt(int index)
        {
            internalCollection.RemoveAt(index);
        }

        /// <summary>
        ///     Sets the <see cref="T:EffortParameter" /> object with the specified name to 
        ///     new value.
        /// </summary>
        /// <param name="parameterName">
        ///     The name of the <see cref="T:EffortParameter" /> object in the collection.
        /// </param>
        /// <param name="value">
        ///     The new <see cref="T:EffortParameter" /> value.
        /// </param>
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Sets the <see cref="T:System.Data.Common.DbParameter" /> object at the
        ///     specified index to a new value.
        /// </summary>
        /// <param name="index">
        ///     The index where the <see cref="T:System.Data.Common.DbParameter" /> object is 
        ///     located.
        /// </param>
        /// <param name="value">
        ///     The new <see cref="T:System.Data.Common.DbParameter" /> value.
        /// </param>
        protected override void SetParameter(int index, DbParameter value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Specifies the <see cref="T:System.Object" /> to be used to synchronize access
        ///     to the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Object" /> to be used to synchronize access to the 
        ///     <see cref="T:EffortParameterrCollection" />.
        /// </returns>
        public override object SyncRoot
        {
            get { return syncRoot; }
        }
    }
}
