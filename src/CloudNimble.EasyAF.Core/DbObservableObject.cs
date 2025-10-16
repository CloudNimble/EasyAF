using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CloudNimble.EasyAF.Core
{

    /// <summary>
    /// A base class for Entity Framework objects to implement <see cref="INotifyPropertyChanged" />, <see cref="IChangeTracking" />, 
    /// and <see cref="IRevertibleChangeTracking" /> in front-end development.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/2363801/what-would-be-the-best-way-to-implement-change-tracking-on-an-object
    /// </remarks>
    public class DbObservableObject : EasyObservableObject, IChangeTracking, IRevertibleChangeTracking
    {

        #region Properties

        /// <summary>
        /// Specifies whether or not the object has changed.
        /// </summary>
        /// <remarks>
        /// Setting this manually allows you to override the default behavior in case your app needs it.
        /// </remarks>
        [JsonIgnore]
        public bool IsChanged { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public bool IsGraphChanged => RecurseGraphInternal(this, a => a.IsChanged, true)?.Any(c => c == true) ?? false;

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> OriginalValues { get; private set; }

        /// <summary>
        /// Specifies whether or not property value changes should be tracked.
        /// </summary>
        /// <remarks>
        /// To track changes, call <see cref="TrackChanges(bool)" />. PropertyChanged events will still be fired, regardless of this setting.
        /// </remarks>
        [JsonIgnore]
        public bool ShouldTrackChanges { get; internal set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DbObservableObject()
        {
            OriginalValues = new();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the <see cref="OriginalValues"/> list and sets <see cref="IsChanged"/> to <see langword="false"/>.
        /// </summary>
        public void AcceptChanges()
        {
            OriginalValues.Clear();
            IsChanged = false;
        }

        /// <summary>
        /// Clears the <see cref="OriginalValues"/> list and sets <see cref="IsChanged"/> to <see langword="false"/>, and optionally traverses the object graph to call <see cref="AcceptChanges()"/> on any children.
        /// </summary>
        /// <param name="goDeep"></param>
        public void AcceptChanges(bool goDeep)
        {
            var visited = new HashSet<DbObservableObject>();
            RecurseGraphInternal(this, obj => obj.AcceptChanges(), goDeep, visited);
        }

        /// <summary>
        /// Sets any child relationships (0..1:1 or 1:*) to null.
        /// </summary>
        /// <remarks>This is typically used to clean an entity before it is POSTed or PUT over an OData API.</remarks>
        public void ClearRelationships()
        {
            foreach (var child in GetRelatedEntityProperties())
            {
                child.SetValue(this, null);
            }

            foreach (var child in GetRelatedEntityCollectionProperties())
            {
                child.SetValue(this, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<PropertyInfo> GetRelatedEntityProperties() => 
            GetType().GetProperties().Where(c => c.PropertyType.IsSubclassOf(typeof(EasyObservableObject)));

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PropertyInfo> GetRelatedEntityCollectionProperties() => 
            GetType().GetProperties().Where(c => c.PropertyType.IsGenericType && c.PropertyType.GetGenericArguments().FirstOrDefault().IsSubclassOf(typeof(EasyObservableObject)));

        /// <summary>
        /// Loops through the <see cref="OriginalValues"/> list, sets any property that has changed back to the value it had when <see cref="TrackChanges(bool)"/> was called,
        /// clears the <see cref="OriginalValues"/> list, and sets <see cref="IsChanged"/> to <see langword="false"/>.
        /// </summary>
        public void RejectChanges()
        {
            foreach (var property in OriginalValues)
            {
                GetType().GetRuntimeProperty(property.Key).SetValue(this, property.Value);
            }
            AcceptChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="goDeep"></param>
        public void RejectChanges(bool goDeep)
        {
            var visited = new HashSet<DbObservableObject>();
            RecurseGraphInternal(this, obj => obj.RejectChanges(), goDeep, visited);
        }

        /// <summary>
        /// Assigns a new value to the property. Then, raises the PropertyChanged event if needed. 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="propertyName">The name of the property that changed.</param>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        protected internal override void Set<T>(string propertyName, ref T field, T newValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (EqualityComparer<T>.Default.Equals(field, newValue)) return;

            if (ShouldTrackChanges && !OriginalValues.ContainsKey(propertyName))
            {
                OriginalValues[propertyName] = field;
                IsChanged = true;
            }

            field = newValue;
            RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Loops through the keys in the <see cref="OriginalValues"/> list and returns an <see cref="ExpandoObject"/> containing JUST the new values for the properties that changed.
        /// </summary>
        /// <param name="deepTracking"></param>
        /// <returns>An <see cref="ExpandoObject"/> containing JUST the new values for the properties that changed.</returns>
        /// <remarks>If the object implements <see cref="IIdentifiable{T}"/>, then the payload will always include the ID.</remarks>
        public ExpandoObject ToDeltaPayload(bool deepTracking = false)
        {
            return ToDeltaPayloadInternal(this, deepTracking);
        }

        /// <summary>
        /// Starts tracking property value changes for every property, optionally activating this behavior for the entire object graph.
        /// </summary>
        /// <param name="deepTracking">
        /// When <see langword="true"/>, loops recursively through the object graph and calls <see cref="TrackChanges(bool)"/> on every object that 
        /// inherits from <see cref="EasyObservableObject"/>.
        /// </param>
        public void TrackChanges(bool deepTracking = false)
        {
            if (!deepTracking)
            {
                ShouldTrackChanges = true;
                return;
            }

            var visited = new HashSet<DbObservableObject>();
            RecurseGraphInternal(this, obj => { obj.ShouldTrackChanges = true; }, deepTracking, visited);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="deepTracking"></param>
        /// <returns></returns>
        protected internal ExpandoObject ToDeltaPayloadInternal(DbObservableObject obj, bool deepTracking = false)
        {
            Ensure.ArgumentNotNull(obj, nameof(obj));

            var result = new ExpandoObject();
            var type = obj.GetType();

            //RWM: Delta payloads will need to have the object ID to know what changed.
            if (type.GetInterface(typeof(IIdentifiable<Guid>).Name) is not null)
            {
                result.TryAdd(nameof(IIdentifiable<Guid>.Id), (this as IIdentifiable<Guid>).Id);
            }

            foreach (var prop in obj.OriginalValues)
            {
                result.TryAdd(prop.Key, type.GetProperty(prop.Key).GetValue(obj));
            }
            if (!deepTracking) return result;

            foreach (var child in obj.GetRelatedEntityProperties())
            {
                var value = (DbObservableObject)child.GetValue(obj);
                if (value is not null && value.IsGraphChanged)
                {
                    result.TryAdd(child.Name, ToDeltaPayloadInternal(value, deepTracking));
                }
            }

            foreach (var child in obj.GetRelatedEntityCollectionProperties())
            {
                var list = (IEnumerable<DbObservableObject>)child.GetValue(obj);
                if (list is not null)
                {
                    var newList = new List<ExpandoObject>();
                    foreach (var item in list.Where(c => c.IsGraphChanged))
                    {
                        newList.Add(ToDeltaPayloadInternal(item, deepTracking));
                    }
                    result.TryAdd(child.Name, newList);
                }

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        /// <param name="goDeep"></param>
        /// <param name="visited"></param>
        protected internal static void RecurseGraphInternal(DbObservableObject obj, Action<DbObservableObject> action, bool goDeep = false,
            HashSet<DbObservableObject> visited = null)
        {
            Ensure.ArgumentNotNull(obj, nameof(obj));
            Ensure.ArgumentNotNull(action, nameof(action));

            visited ??= new HashSet<DbObservableObject>();
            if (visited.Contains(obj)) return;

            visited.Add(obj);
            action?.Invoke(obj);
            if (!goDeep) return;

            foreach (var child in obj.GetRelatedEntityProperties())
            {
                var value = (DbObservableObject)child.GetValue(obj);
                if (value is not null)
                {
                    RecurseGraphInternal(value, action, goDeep, visited);
                }
            }

            foreach (var child in obj.GetRelatedEntityCollectionProperties())
            {
                var list = (IEnumerable<DbObservableObject>)child.GetValue(obj);
                if (list is not null)
                {
                    foreach (var item in list)
                    {
                        RecurseGraphInternal(item, action, goDeep, visited);
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        /// <param name="goDeep"></param>
        /// <param name="visited"></param>
        /// <remarks>
        /// if you want these results to be in a "proper" order, you may need to run a "Reverse" on the resulting enumerable.
        /// </remarks>
        protected internal static IEnumerable<T> RecurseGraphInternal<T>(DbObservableObject obj, Func<DbObservableObject, T> func, bool goDeep = false,
            HashSet<DbObservableObject> visited = null)
        {
            Ensure.ArgumentNotNull(obj, nameof(obj));
            Ensure.ArgumentNotNull(func, nameof(func));

            visited ??= new HashSet<DbObservableObject>();
            if (visited.Contains(obj)) yield break;

            visited.Add(obj);
            if (!goDeep)
            {
                yield return func.Invoke(obj);
            }

            foreach (var child in obj.GetRelatedEntityProperties())
            {
                var value = (DbObservableObject)child.GetValue(obj);
                if (value is not null)
                {
                    foreach (var result in RecurseGraphInternal(value, func, goDeep, visited))
                    {
                        yield return result;
                    }
                }
            }

            foreach (var child in obj.GetRelatedEntityCollectionProperties())
            {
                var list = (IEnumerable<DbObservableObject>)child.GetValue(obj);
                if (list is not null)
                {
                    foreach (var item in list)
                    {
                        foreach (var result in RecurseGraphInternal(item, func, goDeep, visited))
                        {
                            yield return result;
                        }
                    }
                }
            }

            yield return func.Invoke(obj);

        }

        #endregion

    }

}
