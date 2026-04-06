using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// A base class for objects to implement <see cref="INotifyPropertyChanged" />.
    /// Provides strongly-typed property change notifications and automatic property setting with change detection.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Person : EasyObservableObject
    /// {
    ///     private string _name;
    ///     private int _age;
    /// 
    ///     public string Name
    ///     {
    ///         get => _name;
    ///         set => Set(nameof(Name), ref _name, value);
    ///     }
    /// 
    ///     public int Age
    ///     {
    ///         get => _age;
    ///         set => Set(() => Age, ref _age, value);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class EasyObservableObject : INotifyPropertyChanged, IDisposable
    {

        #region Private Members

        private bool disposedValue;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyObservableObject"/> class.
        /// </summary>
        public EasyObservableObject()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a deep copy of the current object using JSON serialization.
        /// </summary>
        /// <typeparam name="T">The type of object to clone. Must inherit from <see cref="EasyObservableObject"/>.</typeparam>
        /// <returns>A new instance of type <typeparamref name="T"/> that is a deep copy of the current object.</returns>
        /// <exception cref="JsonException">Thrown when the object cannot be serialized or deserialized.</exception>
        public T Clone<T>() where T : EasyObservableObject
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize<T>(this as T));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Provides access to the PropertyChanged event handler to derived classes.
        /// </summary>
        protected internal PropertyChangedEventHandler PropertyChangedHandler => PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <remarks>
        /// If the propertyName parameter does not correspond to an existing property on the current class, an exception is thrown in DEBUG configuration only.
        /// </remarks>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected internal virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new NotSupportedException("Raising the PropertyChanged event with an empty string or null is not supported.");
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the PropertyChanged event if needed.
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyExpression">An expression identifying the property that changed.</param>
        protected internal virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression is null) return;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((propertyExpression.Body as MemberExpression).Member.Name));
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the PropertyChanged event if needed. 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyExpression">An expression identifying the property that changed.</param>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        protected internal void Set<T>(Expression<Func<T>> propertyExpression, ref T field, T newValue)
        {
            Ensure.ArgumentNotNull(propertyExpression, nameof(propertyExpression));
            Set((propertyExpression.Body as MemberExpression).Member.Name, ref field, newValue);
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the PropertyChanged event if needed. 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        protected internal virtual void Set<T>(string propertyName, ref T field, T newValue)
        {
            Ensure.ArgumentNotNull(propertyName, nameof(propertyName));

            if (EqualityComparer<T>.Default.Equals(field, newValue)) return;

            field = newValue;
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="EasyObservableObject"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected internal virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~EasyObservableObject()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }

}
